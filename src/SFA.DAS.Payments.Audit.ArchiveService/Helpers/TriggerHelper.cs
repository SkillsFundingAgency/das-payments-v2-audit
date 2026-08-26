using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.DurableTask;
using Microsoft.DurableTask.Client;
using SFA.DAS.Payments.Application.Infrastructure.Logging;
using SFA.DAS.Payments.Audit.ArchiveService.Orchestrators;
using SFA.DAS.Payments.Audit.ArchiveService.Triggers;

namespace SFA.DAS.Payments.Audit.ArchiveService.Helpers
{
    public class TriggerHelper : ITriggerHelper
    {
        public async Task<HttpResponseData> StartOrchestrator(
            HttpRequestData req,
            DurableTaskClient starter,
            IPaymentLogger log
        )
        {
            try
            {
                const string orchestratorName = nameof(PeriodEndArchiveOrchestrator);
                const string triggerName = nameof(PeriodEndArchiveHttpTrigger);

                using var reader = new StreamReader(req.Body);
                var messageJson = await reader.ReadToEndAsync();

                var hasRunningInstance = await HasRunningInstances(triggerName, orchestratorName, starter, log);

                if (hasRunningInstance)
                {
                    var message = $"An instance of {orchestratorName} is already running.";
                    log.LogInfo(message);
                    var conflictResponse = req.CreateResponse(HttpStatusCode.Conflict);
                    await conflictResponse.WriteStringAsync(message);
                    return conflictResponse;
                }

                log.LogInfo($"Clearing down previous {orchestratorName} runs");
                await StatusHelper.ClearCurrentStatus(starter, log);

                log.LogInfo($"Triggering {orchestratorName}");
                var instanceId = await starter.ScheduleNewOrchestrationInstanceAsync(
                    orchestratorName, messageJson,
                    new StartOrchestrationOptions { InstanceId = $"{orchestratorName}-{Guid.NewGuid()}" });

                if (string.IsNullOrEmpty(instanceId))
                {
                    var message =
                        $"An error occurred starting [{orchestratorName}], no instance id was returned.";
                    log.LogInfo(message);
                    var errorResponse = req.CreateResponse(HttpStatusCode.Conflict);
                    await errorResponse.WriteStringAsync(message);
                    return errorResponse;
                }

                log.LogInfo($"Started orchestration with ID = '{instanceId}'.");
                var responseHttpMessage = starter.CreateCheckStatusResponse(req, instanceId);
                if (responseHttpMessage == null)
                {
                    var message =
                        $"An error occurred getting the status of [{orchestratorName}] for instance Id [{instanceId}].";
                    log.LogInfo(message);
                    var errorResponse = req.CreateResponse(HttpStatusCode.Conflict);
                    await errorResponse.WriteStringAsync(message);
                    return errorResponse;
                }

                responseHttpMessage.Body.Position = 0;
                string content;
                using (var responseReader = new StreamReader(responseHttpMessage.Body, leaveOpen: true))
                {
                    content = await responseReader.ReadToEndAsync();
                }

                var newContent = $"Started orchestrator [{orchestratorName}] with ID [{instanceId}]\n\n{content}\n\n";

                responseHttpMessage.Body.SetLength(0);
                await responseHttpMessage.WriteStringAsync(newContent);

                return responseHttpMessage;
            }
            catch (Exception)
            {
                return req.CreateResponse(HttpStatusCode.Conflict);
            }
        }

        public async Task<bool> HasRunningInstances(string orchestratorName,
            string instanceIdPrefix, DurableTaskClient starter, IPaymentLogger log)
        {
            log.LogInfo($"Checking for running instances of {orchestratorName}");

            var query = new OrchestrationQuery
            {
                InstanceIdPrefix = instanceIdPrefix,
                Statuses = new[]
                {
                    OrchestrationRuntimeStatus.Pending,
                    OrchestrationRuntimeStatus.Running,
                    OrchestrationRuntimeStatus.ContinuedAsNew
                }
            };

            await foreach (var _ in starter.GetAllInstancesAsync(query))
            {
                return true;
            }

            return false;
        }
    }
}
