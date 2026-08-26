using System;
using System.Threading.Tasks;
using Microsoft.Azure.Functions.Worker;
using Microsoft.DurableTask.Client;
using Newtonsoft.Json;
using SFA.DAS.Payments.Application.Infrastructure.Logging;
using SFA.DAS.Payments.Audit.ArchiveService.Helpers;
using SFA.DAS.Payments.Monitoring.Jobs.Messages.Commands;

namespace SFA.DAS.Payments.Audit.ArchiveService.Activities
{
    public class ArchiveFailActivity
    {
        private readonly IPaymentLogger logger;

        public ArchiveFailActivity(IPaymentLogger logger)
        {
            this.logger = logger;
        }

        [Function(nameof(ArchiveFailActivity))]
        public async Task Run([ActivityTrigger] string messageJson,
            [DurableClient] DurableTaskClient entityClient)
        {
            var message = JsonConvert.DeserializeObject<RecordPeriodEndFcsHandOverCompleteJob>(messageJson) ??
                          throw new Exception(
                              $"Error in StartPeriodEndArchiveActivity. Message is null. Message: {messageJson}");
            var runInformation = await StatusHelper.GetCurrentJobs(entityClient);
            if(string.IsNullOrEmpty(runInformation.JobId))
            {
                runInformation.JobId = message.JobId.ToString();
            }
            runInformation.Status = "Failed";

            logger.LogError($"JobId: {runInformation.JobId}. ADF InstanceId: {runInformation.InstanceId} PeriodEndArchiveOrchestrator failed");

            await StatusHelper.UpdateCurrentJobStatus(entityClient, runInformation);
        }
    }
}
