using Microsoft.DurableTask.Client;
using Microsoft.DurableTask.Entities;
using SFA.DAS.Payments.Model.Core.Audit;

namespace SFA.DAS.Payments.Audit.ArchiveService.V1.Helper
{
    public interface IEntityHelper
    {
        Task<ArchiveRunInformation> GetCurrentJobs(DurableTaskClient client);
        EntityInstanceId GetEntityId();
        Task ClearCurrentStatus(DurableTaskClient client);
        Task UpdateCurrentJobStatus(DurableTaskClient client, ArchiveRunInformation runInformation);
    }
}