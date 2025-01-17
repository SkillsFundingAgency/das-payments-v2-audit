using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SFA.DAS.Payments.Audit.ArchiveService.V1.Configuration
{
    public interface IAppSettingsOptions
    {
        ConnectionStrings ConnectionStrings { get; set; }
        bool IsEncrypted { get; set; }
        Values Values { get; set; }
    }
}
