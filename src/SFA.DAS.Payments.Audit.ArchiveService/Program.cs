using System;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Azure.Functions.Worker.Extensions.DurableTask;

namespace SFA.DAS.Payments.Audit.ArchiveService
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            var host = Host.CreateDefaultBuilder(args)
                .UseServiceProviderFactory(new AutofacServiceProviderFactory())
                .ConfigureFunctionsWorkerDefaults()
                .ConfigureContainer<ContainerBuilder>(builder =>
                {
                    Infrastructure.IoC.ArchiveDependencyRegistration.RegisterModules(builder);
                })
                .Build();

            host.Run();
        }
    }
}
