using System;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.DurableTask.Client;
using Newtonsoft.Json;
using SFA.DAS.Payments.Application.Infrastructure.Logging;
using SFA.DAS.Payments.Audit.ArchiveService.Helpers;
using SFA.DAS.Payments.Model.Core.Audit;

namespace SFA.DAS.Payments.Audit.ArchiveService.Triggers
{
    public class PeriodEndArchiveHttpTrigger
    {
        private readonly ITriggerHelper triggerHelper;
        private readonly IPaymentLogger log;

        public PeriodEndArchiveHttpTrigger(ITriggerHelper triggerHelper, IPaymentLogger log)
        {
            this.triggerHelper = triggerHelper;
            this.log = log;
        }

        [Function(nameof(PeriodEndArchiveHttpTrigger))]
        public async Task<HttpResponseData> HttpStart(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post",
                Route = "orchestrators/PeriodEndArchiveOrchestrator")]
            HttpRequestData req,
            [DurableClient] DurableTaskClient starter
        )
        {
            try
            {
                if (req.Method.Equals("POST", StringComparison.OrdinalIgnoreCase))
                {
                    if (req.Body != null && req.Body.Length > 0)
                    {
                        return await triggerHelper.StartOrchestrator(req, starter, log);
                    }

                    throw new Exception(
                        $"Error in PeriodEndArchiveHttpTrigger. Request content is null. Request: {req}");
                }

                var urlParam = HttpUtility.ParseQueryString(req.Url.Query).Get("jobId");

                //Ensure the jobId is a valid long
                if (!long.TryParse(urlParam, out _))
                {
                    throw new Exception(
                        $"Error in PeriodEndArchiveHttpTrigger. Invalid jobId. Request: {req}");
                }

                //GET: Get the current status of the job
                var stateResponse = await StatusHelper.GetCurrentJobs(starter) ?? new ArchiveRunInformation();

                if (stateResponse.JobId != urlParam)
                {
                    stateResponse.JobId = urlParam;
                    stateResponse.InstanceId = string.Empty;
                    stateResponse.Status = "Queued";
                }

                var response = req.CreateResponse(HttpStatusCode.OK);
                response.Headers.Add("Content-Type", "application/json; charset=utf-8");
                await response.WriteStringAsync(JsonConvert.SerializeObject(stateResponse));
                return response;
            }

            catch (Exception ex)
            {
                log.LogError("Error in PeriodEndArchiveHttpTrigger", ex);
                var errorResponse = req.CreateResponse(HttpStatusCode.InternalServerError);
                await errorResponse.WriteStringAsync(ex.Message);
                return errorResponse;
            }
        }
    }
}
