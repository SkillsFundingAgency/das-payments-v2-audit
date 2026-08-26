using SFA.DAS.Payments.Model.Core.Audit;
using SFA.DAS.Payments.Application.Infrastructure.Logging;

namespace SFA.DAS.Payments.Audit.ArchiveService.Extensions
{
    // In-memory replacement for durable entity used during isolated-worker POC.
    public static class HandleCurrentJobId
    {
        public const string PeriodEndArchiveEntityName = "CurrentPeriodEndArchiveJobId";

        public static void Add(ArchiveRunInformation info)
        {
            Helpers.StatusHelper.UpdateCurrentJobStatus(info);
        }

        public static void Reset(IPaymentLogger log)
        {
            Helpers.StatusHelper.ClearCurrentStatus(log);
        }

        public static ArchiveRunInformation Get()
        {
            return Helpers.StatusHelper.GetCurrentJobs();
        }
    }
}
