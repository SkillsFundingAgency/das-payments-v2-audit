using System;
using System;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Newtonsoft.Json;
using SFA.DAS.Payments.Audit.ArchiveService.Helpers;
using SFA.DAS.Payments.Model.Core.Audit;
using SFA.DAS.Payments.Monitoring.Jobs.Messages.Commands;

namespace SFA.DAS.Payments.Audit.ArchiveService.Activities
{
    public static class StartPeriodEndArchiveActivity
    {
        [FunctionName(nameof(StartPeriodEndArchiveActivity))]
        public static async Task Run([ActivityTrigger] string messageJson,
            [DurableClient] IDurableEntityClient entityClient)
        {
            var currentRunInfo = await StatusHelper.GetCurrentJobs(entityClient);

            try
            {
                var message = JsonConvert.DeserializeObject<RecordPeriodEndFcsHandOverCompleteJob>(messageJson) ??
                              throw new Exception($"Error in StartPeriodEndArchiveActivity. Message is null. Message: {messageJson}");

                // Simplified: skip DataFactory call in POC — just set run info
                currentRunInfo = new ArchiveRunInformation
                {
                    JobId = message.JobId.ToString(),
                    InstanceId = Guid.NewGuid().ToString(),
                    Status = "Started"
                };
                await StatusHelper.UpdateCurrentJobStatus(entityClient, currentRunInfo);
            }
            catch (Exception)
            {
                currentRunInfo.Status = "Failed";
                await StatusHelper.UpdateCurrentJobStatus(entityClient, currentRunInfo);
                throw;
            }
        }
    }
}
