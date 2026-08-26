using System;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Newtonsoft.Json;
using SFA.DAS.Payments.Application.Infrastructure.Logging;
using SFA.DAS.Payments.Audit.ArchiveService.Helpers;
using SFA.DAS.Payments.Model.Core.Audit;
using System.Net.Http;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;

namespace SFA.DAS.Payments.Audit.ArchiveService.Triggers
{
    public class PeriodEndArchiveHttpTrigger
    {
        private readonly IPaymentLogger _log;

        public PeriodEndArchiveHttpTrigger(IPaymentLogger log)
        {
            _log = log;
        }

        // Back-compat static entry used by existing unit tests (in-process model).
        public static async Task<HttpResponseMessage> HttpStart(HttpRequestMessage req,
            IDurableOrchestrationClient starter,
            IDurableEntityClient entityClient,
            IPaymentLogger log)
        {
            var helper = new TriggerHelper();
            return await helper.StartOrchestrator(req, starter, entityClient, log);
        }

        [Function("PeriodEndArchiveHttpTrigger")]
        public async Task<HttpResponseData> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = "orchestrators/PeriodEndArchiveOrchestrator")] HttpRequestData req,
            FunctionContext context)
        {
            try
            {
                if (req.Method == "POST")
                {
                    var body = await new System.IO.StreamReader(req.Body).ReadToEndAsync();

                    // TODO: Migrated Durable start logic. For now return Accepted with payload echo.
                    var response = req.CreateResponse(HttpStatusCode.Accepted);
                    await response.WriteStringAsync($"Received request to start orchestrator. Payload: {body}");
                    return response;
                }

                // GET: return current status placeholder or queued state
                var query = req.Url.Query;
                var urlParam = System.Web.HttpUtility.ParseQueryString(query).Get("jobId");

                if (!long.TryParse(urlParam, out _))
                {
                    var error = req.CreateResponse(HttpStatusCode.BadRequest);
                    await error.WriteStringAsync("Invalid or missing jobId query parameter");
                    return error;
                }

                var stateResponse = new ArchiveRunInformation
                {
                    JobId = urlParam,
                    InstanceId = string.Empty,
                    Status = "Queued"
                };

                var ok = req.CreateResponse(HttpStatusCode.OK);
                ok.Headers.Add("Content-Type", "application/json");
                await ok.WriteStringAsync(JsonConvert.SerializeObject(stateResponse));
                return ok;
            }
            catch (Exception ex)
            {
                _log?.LogError("Error in PeriodEndArchiveHttpTrigger", ex);
                var resp = req.CreateResponse(HttpStatusCode.InternalServerError);
                await resp.WriteStringAsync(ex.Message);
                return resp;
            }
        }
    }
}