using Autofac;
using Autofac.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SFA.DAS.Payments.Audit.ArchiveService.Infrastructure.IoC;

var host = new HostBuilder()
    .UseServiceProviderFactory(new AutofacServiceProviderFactory())
    .ConfigureFunctionsWorkerDefaults()
    .ConfigureContainer<ContainerBuilder>(DependencyRegister.RegisterModules)
    .Build();

host.Run();