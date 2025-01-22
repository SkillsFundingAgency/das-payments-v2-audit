using Microsoft.DurableTask.Client;
using Microsoft.DurableTask.Entities;
using SFA.DAS.Payments.Model.Core.Audit;
using static SFA.DAS.Payments.Audit.ArchiveService.V1.Helper.StatusHelper;

namespace SFA.DAS.Payments.Audit.ArchiveService.V1.Helper
{
    public interface IEntityHelper
    {
        Task<ArchiveRunInformation> GetCurrentJobs(DurableTaskClient client);
        EntityInstanceId GetEntityId();
        Task ClearCurrentStatus(DurableTaskClient client, EntityState state);
        Task UpdateCurrentJobStatus(DurableTaskClient client, ArchiveRunInformation runInformation, EntityState state);
    }
}