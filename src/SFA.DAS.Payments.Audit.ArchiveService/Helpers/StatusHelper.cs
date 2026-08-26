using SFA.DAS.Payments.Application.Infrastructure.Logging;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using SFA.DAS.Payments.Application.Infrastructure.Logging;
using SFA.DAS.Payments.Model.Core.Audit;

namespace SFA.DAS.Payments.Audit.ArchiveService.Helpers
{
    public static class StatusHelper
    {
        public enum ArchiveStatus
        {
            InProgress,
            Queued,
            Completed,
            Failed
        }

        // In-memory placeholder state used during migration to isolated worker.
        private static ArchiveRunInformation CurrentRun = new ArchiveRunInformation();

        public static void UpdateCurrentJobStatus(ArchiveRunInformation runInformation)
        {
            CurrentRun = runInformation;
        }

        public static void ClearCurrentStatus(IPaymentLogger log)
        {
            log.LogInfo("StatusHelper.ClearCurrentStatus: Clearing down previous archive job");
            CurrentRun = new ArchiveRunInformation();
            log.LogInfo($"StatusHelper.ClearCurrentStatus: Current JobId: {CurrentRun.JobId}, JobStatus: {CurrentRun.Status}");
        }

        public static ArchiveRunInformation GetCurrentJobs()
        {
            return CurrentRun ?? new ArchiveRunInformation();
        }

        public static async Task UpdateCurrentJobStatus(IDurableEntityClient entityClient,
            ArchiveRunInformation runInformation)
        {
            if (entityClient != null)
            {
                var entityId = new EntityId("Handle",
                    Extensions.HandleCurrentJobId.PeriodEndArchiveEntityName);
                await entityClient.SignalEntityAsync(entityId, "add", runInformation);
                return;
            }

            UpdateCurrentJobStatus(runInformation);
            await Task.CompletedTask;
        }

        public static async Task ClearCurrentStatus(IDurableEntityClient entityClient, IPaymentLogger log)
        {
            if (entityClient != null)
            {
                log.LogInfo("StatusHelper.ClearCurrentStatus: Clearing down previous archive job");

                var previousRun = await GetCurrentJobs(entityClient);
                if (previousRun != null)
                {
                    log.LogInfo($"StatusHelper.ClearCurrentStatus: Previous JobId: {previousRun.JobId}, JobStatus: {previousRun.Status}");
                }

                var entityId = new EntityId("Handle",
                    Extensions.HandleCurrentJobId.PeriodEndArchiveEntityName);

                await entityClient.SignalEntityAsync(entityId, "add", new ArchiveRunInformation
                {
                    JobId = string.Empty,
                    InstanceId = string.Empty,
                    Status = string.Empty
                });
                var currentRun = await GetCurrentJobs(entityClient);

                log.LogInfo($"StatusHelper.ClearCurrentStatus: Current JobId: {currentRun.JobId}, JobStatus: {currentRun.Status}");
                return;
            }

            ClearCurrentStatus(log);
        }

        public static async Task<ArchiveRunInformation> GetCurrentJobs(IDurableEntityClient entityClient)
        {
            if (entityClient != null)
            {
                var entityId = new EntityId("Handle",
                    Extensions.HandleCurrentJobId.PeriodEndArchiveEntityName);
                var stateResponse = await entityClient.ReadEntityStateAsync<ArchiveRunInformation>(entityId, null, null);
                return stateResponse.EntityExists ? stateResponse.EntityState : new ArchiveRunInformation();
            }

            return GetCurrentJobs();
        }
    }
}
