using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.Management.DataFactory;
using Microsoft.Identity.Client;
using Microsoft.Rest;
using SFA.DAS.Payments.Audit.ArchiveService.V1.Configuration;

namespace SFA.DAS.Payments.Audit.ArchiveService.V1.Helper
{
    public class DataFactoryHelper : IDataFactoryHelper
    {
        private readonly IAppSettingsOptions _appSettingsOption;
        public DataFactoryHelper(IAppSettingsOptions appSettingsOption)
        {
            _appSettingsOption = appSettingsOption;
        }
        public async Task<DataFactoryManagementClient> CreateClientAsync()
        {
            var app = ConfidentialClientApplicationBuilder.Create(_appSettingsOption.Values.ApplicationId)
               .WithAuthority(_appSettingsOption.Values.AuthorityUri + _appSettingsOption.Values.TenantId)
               .WithClientSecret(_appSettingsOption.Values.AuthenticationKey)
               .WithLegacyCacheCompatibility(false)
               .WithCacheOptions(CacheOptions.EnableSharedCacheOptions)
               .Build();

            var result = await app.AcquireTokenForClient(
                    new[] { _appSettingsOption.Values.ManagementUri })
                .ExecuteAsync();
            var cred = new TokenCredentials(result.AccessToken);

            return new DataFactoryManagementClient(cred)
            {
                SubscriptionId = _appSettingsOption.Values.SubscriptionId
            };
        }
    }
}
