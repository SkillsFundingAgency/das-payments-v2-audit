using Autofac;
using SFA.DAS.Payments.Application.Infrastructure.Ioc.Modules;
using SFA.DAS.Payments.Audit.ArchiveService.Helpers;
using ConfigurationModule = SFA.DAS.Payments.Audit.ArchiveService.Infrastructure.IoC.Modules.ConfigurationModule;

namespace SFA.DAS.Payments.Audit.ArchiveService.Infrastructure.IoC
{
    public static class DependencyRegister
    {
        public static void RegisterModules(ContainerBuilder builder)
        {
            builder.RegisterModule<TelemetryModule>();
            builder.RegisterModule<LoggingModule>();
            builder.RegisterModule<ConfigurationModule>();
            builder.RegisterType<TriggerHelper>().As<ITriggerHelper>().SingleInstance();
        }
    }
}