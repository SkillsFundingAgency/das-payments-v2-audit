using System.Threading.Tasks;
using Microsoft.DurableTask.Client;
using Microsoft.DurableTask.Entities;
using SFA.DAS.Payments.Application.Infrastructure.Logging;
using SFA.DAS.Payments.Audit.ArchiveService.Extensions;
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

        public static EntityInstanceId GetEntityId()
        {
            return new EntityInstanceId(
                nameof(HandleCurrentJobId),
                HandleCurrentJobId.PeriodEndArchiveEntityName);
        }

        public static async Task UpdateCurrentJobStatus(
            DurableTaskClient client,
            ArchiveRunInformation runInformation)
        {
            await client.Entities.SignalEntityAsync(
                GetEntityId(),
                nameof(HandleCurrentJobId.Add),
                runInformation);
        }

        public static async Task ClearCurrentStatus(
            DurableTaskClient client,
            IPaymentLogger log)
        {
            log.LogInfo(
                "StatusHelper.ClearCurrentStatus: Clearing down previous archive job");

            var previousRun = await GetCurrentJobs(client);

            if (previousRun != null)
            {
                log.LogInfo(
                    $"StatusHelper.ClearCurrentStatus: Previous JobId: {previousRun.JobId}, JobStatus: {previousRun.Status}");
            }

            await client.Entities.SignalEntityAsync(
                GetEntityId(),
                nameof(HandleCurrentJobId.Reset));

            var currentRun = await GetCurrentJobs(client);

            log.LogInfo(
                $"StatusHelper.ClearCurrentStatus: Current JobId: {currentRun.JobId}, JobStatus: {currentRun.Status}");
        }

        public static async Task<ArchiveRunInformation> GetCurrentJobs(
            DurableTaskClient client)
        {
            var entity =
                await client.Entities.GetEntityAsync<ArchiveRunInformation>(
                    GetEntityId());

            return entity?.State ?? new ArchiveRunInformation();
        }
    }
}