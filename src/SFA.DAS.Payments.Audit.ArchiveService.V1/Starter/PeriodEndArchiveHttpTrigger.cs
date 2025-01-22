using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.DurableTask;
using Microsoft.DurableTask.Client;
using Microsoft.Extensions.Logging;
using SFA.DAS.Payments.Audit.ArchiveService.V1.Helper;
using SFA.DAS.Payments.Audit.ArchiveService.V1.Orchestrators;
using SFA.DAS.Payments.Model.Core.Audit;
using SFA.DAS.Payments.Monitoring.Jobs.Messages.Commands;
using static System.Runtime.InteropServices.JavaScript.JSType;

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
                    var badRequestResponse = req.CreateResponse(System.Net.HttpStatusCode.BadRequest);
                    await badRequestResponse.WriteAsJsonAsync("Error in ReadFromJsonAsync");
                    return badRequestResponse;
                }

                if (req.Method == "post")
                {
                    await _entityHelper.ClearCurrentStatus(client, StatusHelper.EntityState.add);

                    string instanceId = await client.ScheduleNewOrchestrationInstanceAsync(nameof(PeriodEndArchiveOrchestrator)
                        , input: periodEndFcsHandOverJob
                        , new StartOrchestrationOptions());


                    return await client.CreateCheckStatusResponseAsync(req, instanceId);
                }

                var stateResponse = await _entityHelper.GetCurrentJobs(client) ?? new ArchiveRunInformation();
                if (stateResponse.JobId != periodEndFcsHandOverJob.JobId.ToString())
                {
                    stateResponse.JobId = periodEndFcsHandOverJob.JobId.ToString();
                    stateResponse.InstanceId = string.Empty;
                    stateResponse.Status = "Queued";
                }

                var response = req.CreateResponse(System.Net.HttpStatusCode.OK);
                await response.WriteAsJsonAsync(stateResponse);
                return response;

            }
            catch (Exception ex)
            {
                string error = $"Error while executing {nameof(PeriodEndArchiveHttpTrigger)} ";
                logger.LogError(ex, error, ex.Message);

                var response = req.CreateResponse(System.Net.HttpStatusCode.BadRequest);
                await response.WriteAsJsonAsync(error);
                return response;
            }
        }
    }
}
