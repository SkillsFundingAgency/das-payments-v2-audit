using Microsoft.DurableTask.Client;
using Microsoft.DurableTask.Entities;
using SFA.DAS.Payments.Model.Core.Audit;
using static SFA.DAS.Payments.Audit.ArchiveService.Helper.StatusHelper;

namespace SFA.DAS.Payments.Audit.ArchiveService.Helper
{
    public interface IEntityHelper
    {
        Task<ArchiveRunInformation> GetCurrentJobs(DurableTaskClient client);
        EntityInstanceId GetEntityId();
        Task ClearCurrentStatus(DurableTaskClient client, EntityState state);
        Task UpdateCurrentJobStatus(DurableTaskClient client, ArchiveRunInformation runInformation, EntityState state);
    }
}