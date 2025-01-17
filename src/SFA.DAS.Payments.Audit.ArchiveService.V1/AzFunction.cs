using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.DurableTask;
using Microsoft.DurableTask.Client;
using Microsoft.Extensions.Logging;

namespace SFA.DAS.Payments.Audit.ArchiveService.V1
{
    public static class AzFunction
    {
        [Function(nameof(RunOrchestrator))]
        public static async Task<List<string>> RunOrchestrator(
            [OrchestrationTrigger] TaskOrchestrationContext context)
        {
            ILogger logger = context.CreateReplaySafeLogger(nameof(RunOrchestrator));
            using (logger.BeginScope(new Dictionary<string, object> { ["OrchestrationInstanceId"] = context.InstanceId }))
            {

                logger.LogInformation("Saying hello.");
                var outputs = new List<string>();

                // Replace name and input with values relevant for your Durable Functions Activity
                outputs.Add(await context.CallActivityAsync<string>(nameof(ActivityFunction), "Tokyo"));
                outputs.Add(await context.CallActivityAsync<string>(nameof(ActivityFunction), "Seattle"));
                outputs.Add(await context.CallActivityAsync<string>(nameof(ActivityFunction), "London"));

                logger.LogInformation("Completed orchestration with ID = '{instanceId}'.", context.InstanceId);
                return outputs;
            }
        }

        [Function(nameof(ActivityFunction))]
        public static string ActivityFunction([ActivityTrigger] string name, string instanceId, FunctionContext executionContext)
        {
            ILogger logger = executionContext.GetLogger("SayHello");
            using (logger.BeginScope(new Dictionary<string, object> { ["OrchestrationInstanceId"] = instanceId }))
            {
                logger.LogInformation("Saying hello to {name}.", name);
                return $"Hello {name}!";
            }
        }

        [Function(nameof(HttpStart))]
        public static async Task<HttpResponseData> HttpStart(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post")] HttpRequestData req,
            [DurableClient] DurableTaskClient client,
            FunctionContext executionContext)
        {
            ILogger logger = executionContext.GetLogger(nameof(HttpStart));
            // Function input comes from the request content.
            string instanceId = await client.ScheduleNewOrchestrationInstanceAsync(nameof(RunOrchestrator));

            using (logger.BeginScope(new Dictionary<string, object> { ["OrchestrationInstanceId"] = instanceId }))
            {

                logger.LogInformation("Started orchestration with ID = '{instanceId}'.", instanceId);

                // Returns an HTTP 202 response with an instance management payload.
                // See https://learn.microsoft.com/azure/azure-functions/durable/durable-functions-http-api#start-orchestration
                return await client.CreateCheckStatusResponseAsync(req, instanceId);
            }
        }
    }
}
