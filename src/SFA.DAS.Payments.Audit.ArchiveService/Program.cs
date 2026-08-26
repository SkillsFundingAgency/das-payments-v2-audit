using Autofac;
using Autofac.Extensions.DependencyInjection;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Hosting;
using SFA.DAS.Payments.Application.Infrastructure.Ioc.Modules;
using ConfigurationModule = SFA.DAS.Payments.Audit.ArchiveService.Infrastructure.IoC.Modules.ConfigurationModule;

var host = new HostBuilder()
    .ConfigureFunctionsWorkerDefaults()
    .UseServiceProviderFactory(new AutofacServiceProviderFactory())
    .ConfigureContainer<ContainerBuilder>(builder =>
    {
        builder.RegisterModule<TelemetryModule>();
        builder.RegisterModule<LoggingModule>();
        builder.RegisterModule<ConfigurationModule>();
    })
    .Build();

host.Run();
