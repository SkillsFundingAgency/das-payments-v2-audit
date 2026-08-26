using System;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SFA.DAS.Payments.Audit.ArchiveService.Infrastructure.IoC;

namespace SFA.DAS.Payments.Audit.ArchiveService
{
    internal static class Program
    {
        private static void Main(string[] args)
        {
            var host = Host.CreateDefaultBuilder(args)
                .UseServiceProviderFactory(new AutofacServiceProviderFactory())
                .ConfigureAppConfiguration((context, config) =>
                {
                    // Additional configuration sources can be added here if required
                })
                .ConfigureFunctionsWorkerDefaults(workerApp =>
                {
                    // worker-specific configuration can go here if needed
                })
                .ConfigureContainer<ContainerBuilder>(builder =>
                {
                    // Register the project's Autofac modules
                    ArchiveDependencyRegistration.RegisterModules(builder);
                })
                .ConfigureServices((context, services) =>
                {
                    // Register non-Autofac services (IOptions, small helpers) here if required
                })
                .ConfigureLogging((context, logging) =>
                {
                    logging.AddConsole();
                    // Additional logging providers can be added here
                })
                .Build();

            host.Run();
        }
    }
}