using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SFA.DAS.Payments.Audit.ArchiveService.V1.Configuration;

namespace SFA.DAS.Payments.Audit.ArchiveService.V1.Ioc
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddAppSettingsConfiguration(this IServiceCollection services)
        {
            services.AddSingleton<IAppSettingsOptions>(provider =>
            {
                var configHelper = provider.GetRequiredService<IConfiguration>();

                return new AppSettingsOptions
                {
                    IsEncrypted = configHelper.GetValue<bool>("IsEncrypted"),
                    ConnectionStrings = new ConnectionStrings
                    {
                    },
                    Values = new Values
                    {
                        AzureWebJobsStorage = configHelper.GetValue<string>("AzureWebJobsStorage")
                    }
                };
            });

            return services;
        }
    }
}
