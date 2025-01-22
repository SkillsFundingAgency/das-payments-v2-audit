using Microsoft.DurableTask.Client;
using Microsoft.DurableTask.Entities;
using Microsoft.Extensions.Logging;
using SFA.DAS.Payments.Audit.ArchiveService.V1.EntityTrigger;
using SFA.DAS.Payments.Model.Core.Audit;
using static SFA.DAS.Payments.Audit.ArchiveService.V1.Helper.StatusHelper;

namespace SFA.DAS.Payments.Audit.ArchiveService.V1.Helper
{
    public class EntityHelper : IEntityHelper
    {
        private readonly ILogger<EntityHelper> _logger;
        public EntityHelper(ILogger<EntityHelper> logger)
        {
            _logger = logger;
        }
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

        public async Task ClearCurrentStatus(DurableTaskClient client, EntityState state)
        {
            _logger.LogInformation("Clearing down previous archive job");
            var entityId = GetEntityId();

            var previousJob = await GetCurrentJobs(client);

            if (previousJob is not null)
            {
                _logger.LogInformation($"ClearCurrentStatus: Previous JobId: {previousJob.JobId}, JobStatus: {previousJob.Status}");
            }

            await client.Entities.SignalEntityAsync(entityId, state.ToString(), new ArchiveRunInformation
            {
                JobId = string.Empty,
                InstanceId = string.Empty,
                Status = string.Empty
            });

            var currentJob = await GetCurrentJobs(client);
            _logger.LogInformation($"ClearCurrentStatus: Previous JobId: {previousJob.JobId}, JobStatus: {previousJob.Status}");

        }

        public async Task UpdateCurrentJobStatus(DurableTaskClient client, ArchiveRunInformation runInformation, EntityState state)
        {
            var entityId = GetEntityId();
            await client.Entities.SignalEntityAsync(entityId, state.ToString(), runInformation);
        }
    }
}
