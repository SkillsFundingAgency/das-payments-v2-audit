using System;
using System.IO;
using System.Net;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.DurableTask.Client;
using Microsoft.DurableTask;
using SFA.DAS.Payments.Application.Infrastructure.Logging;
using SFA.DAS.Payments.Audit.ArchiveService.Orchestrators;
using SFA.DAS.Payments.Audit.ArchiveService.Triggers;

namespace SFA.DAS.Payments.Audit.ArchiveService.Helpers
{
    public class TriggerHelper : ITriggerHelper
    {
        public async Task<HttpResponseData> StartOrchestrator(HttpRequestData req, DurableTaskClient starter, IPaymentLogger log)
        {
            try
            {
                const string orchestratorName = nameof(PeriodEndArchiveOrchestrator);

                using var reader = new StreamReader(req.Body);
                var messageJson = await reader.ReadToEndAsync();

                log.LogInfo($"Clearing down previous {orchestratorName} runs");

                await StatusHelper.ClearCurrentStatus(starter, log);

                log.LogInfo($"Triggering {orchestratorName}");

                var instanceId = await starter.ScheduleNewOrchestrationInstanceAsync(orchestratorName, messageJson);

                if (string.IsNullOrEmpty(instanceId))
                {
                    var response = req.CreateResponse(HttpStatusCode.Conflict);

                    await response.WriteStringAsync(
                        $"An error occurred starting [{orchestratorName}], no instance id was returned.");

                    return response;
                }

                log.LogInfo($"Started orchestration with ID = '{instanceId}'.");

                var responseMessage = req.CreateResponse(HttpStatusCode.Accepted);

                await responseMessage.WriteStringAsync(
                    $"Started orchestrator [{orchestratorName}] with ID [{instanceId}]");

                return responseMessage;
            }
            catch (Exception ex)
            {
                log.LogError("Failed starting orchestrator", ex);

                return req.CreateResponse(HttpStatusCode.Conflict);
            }
        }
    }
}
