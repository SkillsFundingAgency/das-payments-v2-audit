using Autofac;
using SFA.DAS.Payments.Application.Infrastructure.Ioc.Modules;
using ConfigurationModule = SFA.DAS.Payments.Audit.ArchiveService.Infrastructure.IoC.Modules.ConfigurationModule;

namespace SFA.DAS.Payments.Audit.ArchiveService.Infrastructure.IoC
{
    public static class ArchiveDependencyRegistration
    {
        // Called from Program.cs ConfigureContainer
        public static void RegisterModules(ContainerBuilder builder)
        {
            builder.RegisterModule<TelemetryModule>();
            builder.RegisterModule<LoggingModule>();
            builder.RegisterModule<ConfigurationModule>();
        }
    }
}
