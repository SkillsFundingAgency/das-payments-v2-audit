using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.DurableTask;
using Microsoft.DurableTask.Client;
using SFA.DAS.Payments.Application.Infrastructure.Logging;
using SFA.DAS.Payments.Audit.ArchiveService.Infrastructure.Configuration;
using SFA.DAS.Payments.Audit.ArchiveService.Orchestrators;
using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;

namespace SFA.DAS.Payments.Audit.ArchiveService.Helpers
{
    public class TriggerHelper : ITriggerHelper
    {
        private readonly IPeriodEndArchiveConfiguration _config;

        public TriggerHelper(IPeriodEndArchiveConfiguration config)
        {
            _config = config;
        }

        public async Task<HttpResponseData> StartOrchestrator(
            HttpRequestData req,
            DurableTaskClient starter,
            IPaymentLogger log)
        {
            try
            {
                const string orchestratorName = nameof(PeriodEndArchiveOrchestrator);

                using var reader = new StreamReader(req.Body);
                var messageJson = await reader.ReadToEndAsync();

                if (string.IsNullOrWhiteSpace(messageJson))
                {
                    var response = req.CreateResponse(HttpStatusCode.BadRequest);
                    await response.WriteStringAsync("Request body cannot be empty.");
                    return response;
                }

                // Check whether an instance of this orchestrator is already running.
                var existingInstances = starter.GetAllInstancesAsync(
                    new OrchestrationQuery
                    {
                        Statuses =
                        [
                            OrchestrationRuntimeStatus.Pending,
                            OrchestrationRuntimeStatus.Running,
                            OrchestrationRuntimeStatus.ContinuedAsNew
                        ]
                    });

                await foreach (var instance in existingInstances)
                {
                    if (instance.InstanceId.StartsWith($"{orchestratorName}-", StringComparison.OrdinalIgnoreCase))
                    {
                        var response = req.CreateResponse(HttpStatusCode.Conflict);

                        await response.WriteStringAsync($"An instance of {orchestratorName} is already running.");

                        log.LogInfo($"An instance of {orchestratorName} is already running.");

                        return response;
                    }
                }

                log.LogInfo($"Clearing down previous {orchestratorName} runs");

                await StatusHelper.ClearCurrentStatus(starter, log);

                log.LogInfo($"Triggering {orchestratorName}");

                // Preserve the old instance ID format.
                var instanceId = $"{orchestratorName}-{Guid.NewGuid()}";

                var options = new StartOrchestrationOptions
                {
                    InstanceId = instanceId
                };

                var input = new PeriodEndArchiveOrchestrationInput
                {
                    MessageJson = messageJson,
                    SleepDelay = _config.SleepDelay
                };

                await starter.ScheduleNewOrchestrationInstanceAsync(orchestratorName, input, options);

                log.LogInfo($"Started orchestration with ID = '{instanceId}'.");

                // Isolated equivalent of CreateCheckStatusResponse.
                return await starter.CreateCheckStatusResponseAsync(req, instanceId);
            }
            catch (Exception ex)
            {
                log.LogError("Failed starting orchestrator", ex);

                return req.CreateResponse(HttpStatusCode.Conflict);
            }
        }
    }
}