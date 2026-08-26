using System.Threading.Tasks;
using SFA.DAS.Payments.Audit.ArchiveService.Infrastructure.Configuration;

namespace SFA.DAS.Payments.Audit.ArchiveService.Helpers
{
    // Simplified helper used during migration POC. Returns null as DataFactory client is not used by POC activities.
    public static class DataFactoryHelper
    {
        public static Task<object?> CreateClient(IPeriodEndArchiveConfiguration config)
        {
            return Task.FromResult<object?>(null);
        }
    }
}
