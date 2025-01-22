using Microsoft.Azure.Functions.Worker;
using Microsoft.DurableTask.Client;
using Microsoft.Extensions.Logging;
using SFA.DAS.Payments.Audit.ArchiveService.V1.Helper;
using SFA.DAS.Payments.Audit.ArchiveService.V1.Models;
using SFA.DAS.Payments.Model.Core.Audit;

namespace SFA.DAS.Payments.Audit.ArchiveService.V1.Activities
{
    public class ArchiveFailActivity
    {
        private readonly IEntityHelper _entityHelper;
        public ArchiveFailActivity(IEntityHelper entityHelper)
        {
            _entityHelper = entityHelper;
        }

        [Function(nameof(ArchiveFailActivity))]
        public async Task StartArchiveFailActivity([ActivityTrigger] PeriodEndArchiveActivityResponse PeriodEndArchiveActivityResponse
            , FunctionContext executionContext
            , [DurableClient] DurableTaskClient client)
        {
            var currentJob = await _entityHelper.GetCurrentJobs(client);

            ILogger logger = executionContext.GetLogger(nameof(ArchiveFailActivity));
            try
            {
                logger.LogInformation($"Starting {nameof(ArchiveFailActivity)} for OrchestrationInstanceId: {PeriodEndArchiveActivityResponse.InstanceId}");

                await _entityHelper.UpdateCurrentJobStatus(client, new ArchiveRunInformation
                {
                    JobId = currentJob.JobId,
                    InstanceId = currentJob.InstanceId,
                    Status = "Failed"
                }, StatusHelper.EntityState.add);

                logger.LogError($"JobId: {currentJob.JobId}. ADF InstanceId: {currentJob.InstanceId} PeriodEndArchiveOrchestrator failed");

            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Error while executing {nameof(ArchiveFailActivity)} function with InstanceId : {PeriodEndArchiveActivityResponse.InstanceId}", ex.Message);

                await _entityHelper.UpdateCurrentJobStatus(client, new ArchiveRunInformation
                {
                    JobId = currentJob.JobId,
                    InstanceId = currentJob.InstanceId,
                    Status = "Failed"
                }, StatusHelper.EntityState.add);
            }
        }
    }
}
