using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using Serilog;
using SFA.DAS.Payments.Application.Infrastructure.Logging;
using SFA.DAS.Payments.Audit.ArchiveService.Orchestrators;
using SFA.DAS.Payments.Audit.ArchiveService.Triggers;
using ILogger = Microsoft.Extensions.Logging.ILogger;

namespace SFA.DAS.Payments.Audit.ArchiveService.Helpers
{
    public class TriggerHelper : ITriggerHelper
    {
        public async Task<HttpResponseMessage> StartOrchestrator(
            HttpRequestMessage req,
            IDurableOrchestrationClient starter,
            ILogger log,
            IDurableEntityClient client
        )
        {
            try
            {
                const string orchestratorName = nameof(PeriodEndArchiveOrchestrator);
                const string triggerName = nameof(PeriodEndArchiveHttpTrigger);
                var messageJson = await req.Content?.ReadAsStringAsync()!;

                var existingInstances = await GetRunningInstances(triggerName, orchestratorName, starter, log);

                if (existingInstances != null && existingInstances.DurableOrchestrationState.Any())
                {
                    var responseMessage = new HttpResponseMessage(HttpStatusCode.Conflict)
                        { Content = new StringContent($"An instance of {orchestratorName} is already running.") };
                    log.Log(LogLevel.Information, await responseMessage.Content.ReadAsStringAsync());
                    return responseMessage;
                }

                log.Log(LogLevel.Information, $"Clearing down previous {orchestratorName} runs");
                await StatusHelper.ClearCurrentStatus(client, log);

                log.Log(LogLevel.Information, $"Triggering {orchestratorName}");
                var instanceId =
                    await starter.StartNewAsync(orchestratorName, $"{orchestratorName}-{Guid.NewGuid()}", messageJson);
                if (string.IsNullOrEmpty(instanceId))
                {
                    var responseMessage = new HttpResponseMessage(HttpStatusCode.Conflict)
                    {
                        Content = new StringContent(
                            $"An error occurred starting [{orchestratorName}], no instance id was returned.")
                    };
                    log.Log(LogLevel.Information, await responseMessage.Content.ReadAsStringAsync());
                    return responseMessage;
                }

                log.Log(LogLevel.Information, $"Started orchestration with ID = '{instanceId}'.");
                var responseHttpMessage = starter.CreateCheckStatusResponse(req, instanceId);
                if (responseHttpMessage == null)
                {
                    var responseMessage = new HttpResponseMessage(HttpStatusCode.Conflict)
                    {
                        Content = new StringContent(
                            $"An error occurred getting the status of [{orchestratorName}] for instance Id [{instanceId}].")
                    };
                    log.Log(LogLevel.Information, await responseMessage.Content.ReadAsStringAsync());
                    return responseMessage;
                }

                var content = await responseHttpMessage.Content.ReadAsStringAsync();
                var newContent = $"Started orchestrator [{orchestratorName}] with ID [{instanceId}]\n\n{content}\n\n";
                responseHttpMessage.Content = new StringContent(newContent);

                return responseHttpMessage;
            }
            catch (Exception ex)
            {
                return new HttpResponseMessage(HttpStatusCode.Conflict);
            }
        }

        public async Task<OrchestrationStatusQueryResult> GetRunningInstances(string orchestratorName,
            string instanceIdPrefix, IDurableOrchestrationClient starter, ILogger log)
        {
            log.Log(LogLevel.Information, $"Checking for running instances of {orchestratorName}");

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