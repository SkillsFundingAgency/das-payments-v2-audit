using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Grpc.Core;

namespace SFA.DAS.Payments.Audit.ArchiveService.V1.Models
{
    public class PeriodEndArchiveActivityResponse
    {
        public string RunId { get; set; }
        public HttpStatusCode StatusCode { get; set; }

        public string InstanceId { get; set; }
    }
}
