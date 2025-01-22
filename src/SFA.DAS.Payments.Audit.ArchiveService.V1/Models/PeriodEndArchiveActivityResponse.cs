using System.Net;

namespace SFA.DAS.Payments.Audit.ArchiveService.V1.Models
{
    public class PeriodEndArchiveActivityResponse
    {
        public string RunId { get; set; }
        public HttpStatusCode StatusCode { get; set; }

        public string InstanceId { get; set; }
    }
}
