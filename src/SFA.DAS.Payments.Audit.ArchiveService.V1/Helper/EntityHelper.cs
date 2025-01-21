using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.DurableTask.Client;
using Microsoft.DurableTask.Entities;
using SFA.DAS.Payments.Audit.ArchiveService.V1.EntityTrigger;
using SFA.DAS.Payments.Model.Core.Audit;

namespace SFA.DAS.Payments.Audit.ArchiveService.V1.Helper
{
    public class EntityHelper : IEntityHelper
    {
        public EntityInstanceId GetEntityId()
        {
            return new EntityInstanceId(nameof(EntityDispatcher), EntityDispatcher.PeriodEndArchiveEntityName);
        }

        public async Task<ArchiveRunInformation> GetCurrentJobs(DurableTaskClient client)
        {
            var entityId = GetEntityId();
            var entityResponse = await client.Entities.GetEntityAsync<ArchiveRunInformation>(entityId);
            if (entityResponse == null)
            {
                return new ArchiveRunInformation();
            }
            return entityResponse.State != null ? entityResponse.State : new ArchiveRunInformation();
        }

        public async Task ClearCurrentStatus(DurableTaskClient client)
        {
            var entityId = GetEntityId();
            var currentJob = GetCurrentJobs(client);
            await client.Entities.SignalEntityAsync(entityId, "add", new ArchiveRunInformation {
                JobId = string.Empty,
                InstanceId = string.Empty,
                Status = string.Empty
            });
        }

        public async Task UpdateCurrentJobStatus(DurableTaskClient client, ArchiveRunInformation runInformation)
        {
            var entityId = GetEntityId();
            await client.Entities.SignalEntityAsync(entityId, "add", runInformation);
        }
    }
}
