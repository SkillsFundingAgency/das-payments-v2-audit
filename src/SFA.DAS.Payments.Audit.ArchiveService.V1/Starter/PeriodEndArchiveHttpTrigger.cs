using System.Text.Json;
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
        private ILogger<PeriodEndArchiveHttpTrigger> _logger;
        private readonly IEntityHelper _entityHelper;

        public PeriodEndArchiveHttpTrigger(IEntityHelper entityHelper
            , ILogger<PeriodEndArchiveHttpTrigger> logger)
        {
            _entityHelper = entityHelper;
            _logger = logger;
        }

        [Function(nameof(PeriodEndArchiveHttpTrigger))]
        public async Task<HttpResponseData> HttpTriggerArchivePeriodEnd(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = "orchestrators/PeriodEndArchiveOrchestrator")] HttpRequestData req,
            [DurableClient] DurableTaskClient client,
            FunctionContext executionContext)
        {
            _logger = executionContext.GetLogger<PeriodEndArchiveHttpTrigger>();
            try
            {
                RecordPeriodEndFcsHandOverCompleteJob periodEndFcsHandOverJob = JsonSerializer.Deserialize<RecordPeriodEndFcsHandOverCompleteJob>
                    (await req.ReadAsStringAsync());

                if (periodEndFcsHandOverJob == null)
                {
                    string error = "Request payload is null";
                    HttpResponseData badRequestResponse = await BuildErrorResponse(req, error);
                    _logger.LogError(error);
                    return badRequestResponse;
                }

                if (req.Method == "POST")
                {
                    if (periodEndFcsHandOverJob.CollectionPeriod is 0 || periodEndFcsHandOverJob.CollectionYear is 0)
                    {
                        string error = $"Error in {nameof(PeriodEndArchiveHttpTrigger)}. CollectionPeriod or CollectionYear is invalid. CollectionPeriod: {periodEndFcsHandOverJob.CollectionPeriod}. CollectionYear: {periodEndFcsHandOverJob.CollectionYear}";
                        _logger.LogError(error);

                        HttpResponseData badRequestResponse = await BuildErrorResponse(req, error);
                        return badRequestResponse;
                    }

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

                HttpResponseData response = await BuildOkResponse(req, stateResponse);
                return response;

            }
            catch (Exception ex)
            {
                string error = $"Error while executing {nameof(PeriodEndArchiveHttpTrigger)} ";
                _logger.LogError(ex, error, ex.Message);

                HttpResponseData badRequestResponse = await BuildErrorResponse(req, error);
                return badRequestResponse;
            }
        }

        private static async Task<HttpResponseData> BuildErrorResponse(HttpRequestData req, string errorMessage)
        {
            var badRequestResponse = req.CreateResponse(System.Net.HttpStatusCode.BadRequest);
            await badRequestResponse.WriteStringAsync(errorMessage);
            return badRequestResponse;
        }

        private static async Task<HttpResponseData> BuildOkResponse(HttpRequestData req, ArchiveRunInformation stateResponse)
        {
            string responseString = JsonSerializer.Serialize(stateResponse);
            var response = req.CreateResponse(System.Net.HttpStatusCode.OK);
            response.Headers.Add("Content-Type", "application/json");
            await response.WriteStringAsync(responseString);
            return response;
        }
    }
}
