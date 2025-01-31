using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.DurableTask;
using Microsoft.DurableTask.Client;
using Microsoft.Extensions.Logging;
using SFA.DAS.Payments.Audit.ArchiveService.Helper;
using SFA.DAS.Payments.Audit.ArchiveService.Orchestrators;
using SFA.DAS.Payments.Model.Core.Audit;
using SFA.DAS.Payments.Monitoring.Jobs.Messages.Commands;
using System.Text.Json;
using System.Web;


namespace SFA.DAS.Payments.Audit.ArchiveService.Starter
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
                if (req.Method == "POST")
                {
                    var periodEndFcsHandOverJob = JsonSerializer.Deserialize<RecordPeriodEndFcsHandOverCompleteJob>
                        (await req.ReadAsStringAsync());

                    if (periodEndFcsHandOverJob == null)
                    {
                        string error = "Request payload is null";

                        _logger.LogError(error);
                        return await BuildErrorResponse(req, error);
                    }

                    if (periodEndFcsHandOverJob.CollectionPeriod is 0 || periodEndFcsHandOverJob.CollectionYear is 0)
                    {
                        string error = $"Error in {nameof(PeriodEndArchiveHttpTrigger)}. CollectionPeriod or CollectionYear is invalid. CollectionPeriod: {periodEndFcsHandOverJob.CollectionPeriod}. CollectionYear: {periodEndFcsHandOverJob.CollectionYear}";
                        _logger.LogError(error);

                        return await BuildErrorResponse(req, error);
                    }

                    await _entityHelper.ClearCurrentStatus(client, StatusHelper.EntityState.add);

                    string instanceId = await client.ScheduleNewOrchestrationInstanceAsync(nameof(PeriodEndArchiveOrchestrator)
                        , input: periodEndFcsHandOverJob
                        , new StartOrchestrationOptions());


                    return await client.CreateCheckStatusResponseAsync(req, instanceId);
                }
                else if (req.Method == "GET")
                {
                    var queryParams = HttpUtility.ParseQueryString(req.Url.Query);
                    var jobId = queryParams.Get("jobId");

                    if (jobId is null)
                    {
                        string error = $"Method not supported in: {nameof(PeriodEndArchiveHttpTrigger)} jobId is not been passed with the request.";
                        _logger.LogError(error);

                        return await BuildErrorResponse(req, error);
                    }

                    var stateResponse = await _entityHelper.GetCurrentJobs(client) ?? new ArchiveRunInformation();
                    if (stateResponse.JobId != jobId)
                    {
                        stateResponse.JobId = jobId;
                        stateResponse.InstanceId = string.Empty;
                        stateResponse.Status = "Queued";
                    }

                    return await BuildOkResponse(req, stateResponse);
                }
                else
                {
                    string error = $"Method not supported in: {nameof(PeriodEndArchiveHttpTrigger)} ";
                    _logger.LogError(error);
                    return await BuildErrorResponse(req, error);
                }

            }
            catch (Exception ex)
            {
                string error = $"Error while executing {nameof(PeriodEndArchiveHttpTrigger)} ";
                _logger.LogError(ex, error, ex.Message);

                return await BuildErrorResponse(req, error);
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
