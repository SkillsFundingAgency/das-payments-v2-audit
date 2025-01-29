using Microsoft.Azure.Functions.Worker;
using Microsoft.DurableTask.Client;
using Microsoft.Extensions.Logging;
using SFA.DAS.Payments.Audit.ArchiveService.Helper;
using SFA.DAS.Payments.Audit.ArchiveService.Models;
using SFA.DAS.Payments.Model.Core.Audit;

namespace SFA.DAS.Payments.Audit.ArchiveService.Activities
{
    public class ArchiveFailActivity
    {
        private readonly IEntityHelper _entityHelper;
        private ILogger<ArchiveFailActivity> _logger;
        public ArchiveFailActivity(IEntityHelper entityHelper, ILogger<ArchiveFailActivity> logger)
        {
            _entityHelper = entityHelper;
            _logger = logger;
        }

        [Function(nameof(ArchiveFailActivity))]
        public async Task StartArchiveFailActivity([ActivityTrigger] PeriodEndArchiveActivityResponse PeriodEndArchiveActivityResponse
            , FunctionContext executionContext
            , [DurableClient] DurableTaskClient client)
        {
            var currentJob = await _entityHelper.GetCurrentJobs(client);

            _logger = executionContext.GetLogger<ArchiveFailActivity>();
            try
            {
                _logger.LogInformation($"Starting {nameof(ArchiveFailActivity)} for OrchestrationInstanceId: {PeriodEndArchiveActivityResponse.InstanceId}");

                await _entityHelper.UpdateCurrentJobStatus(client, new ArchiveRunInformation
                {
                    JobId = currentJob.JobId,
                    InstanceId = currentJob.InstanceId,
                    Status = "Failed"
                }, StatusHelper.EntityState.add);

                _logger.LogError($"JobId: {currentJob.JobId}. ADF InstanceId: {currentJob.InstanceId} PeriodEndArchiveOrchestrator failed");

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error while executing {nameof(ArchiveFailActivity)} function with InstanceId : {PeriodEndArchiveActivityResponse.InstanceId}", ex.Message);

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
