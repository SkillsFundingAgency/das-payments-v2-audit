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
        private readonly IPaymentLogger _logger;

        public ArchiveFailActivity(IPaymentLogger logger)
        {
            _logger = logger;
        }

        [Function(nameof(ArchiveFailActivity))]
        public async Task Run(
            [ActivityTrigger] string messageJson,
            [DurableClient] DurableTaskClient durableClient)
        {
            var message =
                JsonConvert.DeserializeObject<RecordPeriodEndFcsHandOverCompleteJob>(messageJson)
                ?? throw new Exception(
                    $"Error in ArchiveFailActivity. Message is null. Message: {messageJson}");

            var runInformation = await StatusHelper.GetCurrentJobs(durableClient);

            if (string.IsNullOrEmpty(runInformation.JobId))
            {
                runInformation.JobId = message.JobId.ToString();
            }

            runInformation.Status = "Failed";

            _logger.LogError(
                $"JobId: {runInformation.JobId}. " +
                $"ADF InstanceId: {runInformation.InstanceId} " +
                "PeriodEndArchiveOrchestrator failed");

            await StatusHelper.UpdateCurrentJobStatus(
                durableClient,
                runInformation);
        }
    }
}