using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using SFA.DAS.Payments.Application.Infrastructure.Logging;
using SFA.DAS.Payments.Audit.ArchiveService.Orchestrators;
using SFA.DAS.Payments.Audit.ArchiveService.Triggers;

namespace SFA.DAS.Payments.Audit.ArchiveService.Helpers
{
    public class TriggerHelper : ITriggerHelper
    {
        public async Task<HttpResponseMessage> StartOrchestrator(
            HttpRequestMessage req,
            IDurableOrchestrationClient starter,
            IDurableEntityClient client,
            IPaymentLogger log)
        {
            try
            {
                const string orchestratorName = nameof(PeriodEndArchiveOrchestrator);
                const string triggerName = nameof(PeriodEndArchiveHttpTrigger);
                var messageJson = await req.Content?.ReadAsStringAsync()!;

                // If no durable starter provided, fallback to in-memory behaviour
                if (starter == null)
                {
                    // Check current in-memory run status
                    var current = StatusHelper.GetCurrentJobs();
                    if (current != null && (current.Status == StatusHelper.ArchiveStatus.InProgress.ToString()
                                             || current.Status == StatusHelper.ArchiveStatus.Queued.ToString()))
                    {
                        var responseMessage = new HttpResponseMessage(HttpStatusCode.Conflict)
                            { Content = new StringContent($"An instance of {orchestratorName} is already running.") };
                        log.LogInfo(await responseMessage.Content.ReadAsStringAsync());
                        return responseMessage;
                    }

                    log.LogInfo($"Clearing down previous {orchestratorName} runs");
                    StatusHelper.ClearCurrentStatus(log);

                    var jobId = Guid.NewGuid().ToString();
                    StatusHelper.UpdateCurrentJobStatus(new Model.Core.Audit.ArchiveRunInformation
                    {
                        JobId = jobId,
                        InstanceId = jobId,
                        Status = StatusHelper.ArchiveStatus.Queued.ToString()
                    });

                    var response = new HttpResponseMessage(HttpStatusCode.Accepted)
                    {
                        Content = new StringContent($"Started orchestrator [{orchestratorName}] with ID [{jobId}]\n\n\n\n")
                    };
                    return response;
                }

                var existingInstances = await GetRunningInstances(triggerName, orchestratorName, starter, log);

                if (existingInstances != null && existingInstances.DurableOrchestrationState.Any())
                {
                    var responseMessage = new HttpResponseMessage(HttpStatusCode.Conflict)
                        { Content = new StringContent($"An instance of {orchestratorName} is already running.") };
                    log.LogInfo(await responseMessage.Content.ReadAsStringAsync());
                    return responseMessage;
                }

                log.LogInfo($"Clearing down previous {orchestratorName} runs");
                await StatusHelper.ClearCurrentStatus(client, log);

                log.LogInfo($"Triggering {orchestratorName}");
                var instanceId = await starter.StartNewAsync(orchestratorName, $"{orchestratorName}-{Guid.NewGuid()}", messageJson);
                if (string.IsNullOrEmpty(instanceId))
                {
                    var responseMessage = new HttpResponseMessage(HttpStatusCode.Conflict)
                    {
                        Content = new StringContent($"An error occurred starting [{orchestratorName}], no instance id was returned.")
                    };
                    log.LogInfo(await responseMessage.Content.ReadAsStringAsync());
                    return responseMessage;
                }

                log.LogInfo($"Started orchestration with ID = '{instanceId}'.");
                var responseHttpMessage = starter.CreateCheckStatusResponse(req, instanceId);
                if (responseHttpMessage == null)
                {
                    var responseMessage = new HttpResponseMessage(HttpStatusCode.Conflict)
                    {
                        Content = new StringContent($"An error occurred getting the status of [{orchestratorName}] for instance Id [{instanceId}].")
                    };
                    log.LogInfo(await responseMessage.Content.ReadAsStringAsync());
                    return responseMessage;
                }

                var content = await responseHttpMessage.Content.ReadAsStringAsync();
                var newContent = $"Started orchestrator [{orchestratorName}] with ID [{instanceId}]\n\n{content}\n\n";
                responseHttpMessage.Content = new StringContent(newContent);

                return responseHttpMessage;
            }
            catch (Exception)
            {
                return new HttpResponseMessage(HttpStatusCode.Conflict);
            }
        }

        public async Task<OrchestrationStatusQueryResult> GetRunningInstances(string orchestratorName,
            string instanceIdPrefix, IDurableOrchestrationClient starter, IPaymentLogger log)
        {
            log.LogInfo($"Checking for running instances of {orchestratorName}");

            var runningInstances = await starter.ListInstancesAsync(new OrchestrationStatusQueryCondition
            {
                InstanceIdPrefix = instanceIdPrefix,
                RuntimeStatus = new[]
                {
                    OrchestrationRuntimeStatus.Pending,
                    OrchestrationRuntimeStatus.Running,
                    OrchestrationRuntimeStatus.ContinuedAsNew
                }
            }, CancellationToken.None);

            return runningInstances;
        }
    }
}
