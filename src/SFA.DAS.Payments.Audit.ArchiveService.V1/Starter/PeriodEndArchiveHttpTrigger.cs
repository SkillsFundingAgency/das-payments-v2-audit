using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.DurableTask;
using Microsoft.DurableTask.Client;
using Microsoft.Extensions.Logging;
using SFA.DAS.Payments.Audit.ArchiveService.V1.Orchestrators;
using SFA.DAS.Payments.Monitoring.Jobs.Messages.Commands;

namespace SFA.DAS.Payments.Audit.ArchiveService.V1.Starter
{
    public static class PeriodEndArchiveHttpTrigger
    {
        [Function(nameof(PeriodEndArchiveHttpTrigger))]
        public static async Task<HttpResponseData> HttpTriggerArchivePeriodEnd(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post")] HttpRequestData req,
            [DurableClient] DurableTaskClient client,
            FunctionContext executionContext)
        {
            ILogger logger = executionContext.GetLogger(nameof(PeriodEndArchiveHttpTrigger));
            RecordPeriodEndFcsHandOverCompleteJob periodEndFcsHandOverJob = await req.ReadFromJsonAsync<RecordPeriodEndFcsHandOverCompleteJob>();
            if (periodEndFcsHandOverJob == null)
            {
                return req.CreateResponse(System.Net.HttpStatusCode.BadRequest);
            }

            // Function input comes from the request content.
            string instanceId = await client.ScheduleNewOrchestrationInstanceAsync(nameof(PeriodEndArchiveOrchestrator)
                , input: periodEndFcsHandOverJob
                , new StartOrchestrationOptions());

            logger.LogInformation("Started orchestration with ID = '{instanceId}'.", instanceId);

            return await client.CreateCheckStatusResponseAsync(req, instanceId);
        }
    }
}
