using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.DurableTask;
using Microsoft.DurableTask.Client;
using Microsoft.Extensions.Logging;
using SFA.DAS.Payments.Audit.ArchiveService.V1.Helper;
using SFA.DAS.Payments.Audit.ArchiveService.V1.Orchestrators;
using SFA.DAS.Payments.Monitoring.Jobs.Messages.Commands;

namespace SFA.DAS.Payments.Audit.ArchiveService.V1.Starter
{
    public class PeriodEndArchiveHttpTrigger
    {
        private readonly IEntityHelper _entityHelper;
        public PeriodEndArchiveHttpTrigger(IEntityHelper entityHelper)
        {
            _entityHelper = entityHelper;
        }

        [Function(nameof(PeriodEndArchiveHttpTrigger))]
        public async Task<HttpResponseData> HttpTriggerArchivePeriodEnd(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = "orchestrators/PeriodEndArchiveOrchestrator")] HttpRequestData req,
            [DurableClient] DurableTaskClient client,
            FunctionContext executionContext)
        {
            ILogger logger = executionContext.GetLogger(nameof(PeriodEndArchiveHttpTrigger));
            try
            {
                RecordPeriodEndFcsHandOverCompleteJob periodEndFcsHandOverJob = await req.ReadFromJsonAsync<RecordPeriodEndFcsHandOverCompleteJob>();
                if (periodEndFcsHandOverJob == null)
                {
                    return req.CreateResponse(System.Net.HttpStatusCode.BadRequest);
                }

                await _entityHelper.ClearCurrentStatus(client, StatusHelper.EntityState.add);

                string instanceId = await client.ScheduleNewOrchestrationInstanceAsync(nameof(PeriodEndArchiveOrchestrator)
                    , input: periodEndFcsHandOverJob
                    , new StartOrchestrationOptions());

                logger.LogInformation($"Started orchestration with ID = '{instanceId}'.");

                return await client.CreateCheckStatusResponseAsync(req, instanceId);
            }
            catch (Exception ex)
            {
                string error = $"Error while executing {nameof(PeriodEndArchiveHttpTrigger)} ";
                logger.LogError(ex, error, ex.Message);
                return req.CreateResponse(System.Net.HttpStatusCode.BadRequest);
            }
        }
    }
}
