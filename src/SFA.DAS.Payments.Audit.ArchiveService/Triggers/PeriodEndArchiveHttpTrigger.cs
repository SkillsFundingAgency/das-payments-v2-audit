using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;
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
        private readonly IPaymentLogger log;
        private readonly ITriggerHelper _triggerHelper;

        public PeriodEndArchiveHttpTrigger(IPaymentLogger log, ITriggerHelper triggerHelper)
        {
            this.log = log;
            _triggerHelper = triggerHelper;
        }

        [Function(nameof(PeriodEndArchiveHttpTrigger))]
        public async Task<HttpResponseData> HttpStart(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = "orchestrators/PeriodEndArchiveOrchestrator")]
            HttpRequestData req, 
            [DurableClient] DurableTaskClient starter)
        {
            try
            {
                if (req.Method.Equals("POST", StringComparison.OrdinalIgnoreCase))
                {
                    return await _triggerHelper.StartOrchestrator(req, starter, log);
                }

                var query = System.Web.HttpUtility.ParseQueryString(req.Url.Query);

                var jobId = query["jobId"];

                if (!long.TryParse(jobId, out _))
                {
                    throw new Exception($"Error in PeriodEndArchiveHttpTrigger. Invalid jobId.");
                }

                var stateResponse = await StatusHelper.GetCurrentJobs(starter) ?? new ArchiveRunInformation();

                if (stateResponse.JobId != jobId)
                {
                    stateResponse.JobId = jobId;
                    stateResponse.InstanceId = string.Empty;
                    stateResponse.Status = "Queued";
                }

                var response = req.CreateResponse(HttpStatusCode.OK);

                await response.WriteStringAsync(JsonConvert.SerializeObject(stateResponse));

                return response;
            }
            catch (Exception ex)
            {
                log.LogError("Error in PeriodEndArchiveHttpTrigger", ex);

                var response = req.CreateResponse(HttpStatusCode.InternalServerError);

                await response.WriteStringAsync(ex.Message);

                return response;
            }
        }
    }
}