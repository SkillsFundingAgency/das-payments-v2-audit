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
