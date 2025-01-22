using DurableTask.Core.Entities;
using Microsoft.DurableTask.Client;
using Microsoft.DurableTask.Entities;
using SFA.DAS.Payments.Audit.ArchiveService.V1.Activities;
using SFA.DAS.Payments.Audit.ArchiveService.V1.EntityTrigger;
using SFA.DAS.Payments.Model.Core.Audit;

namespace SFA.DAS.Payments.Audit.ArchiveService.V1.Helper
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
        public enum EntityState
        {
            add,
            reset,
            get,
            delete
        }
    }
}
