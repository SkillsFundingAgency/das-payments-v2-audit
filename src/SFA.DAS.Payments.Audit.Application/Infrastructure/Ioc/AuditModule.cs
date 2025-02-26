using Autofac;
using SFA.DAS.Payments.Application.Infrastructure.Logging;
using SFA.DAS.Payments.Audit.Application.Data;
using SFA.DAS.Payments.Audit.Application.Infrastructure.Messaging;
using SFA.DAS.Payments.Audit.Application.PaymentsEventProcessing;
using SFA.DAS.Payments.Core.Configuration;

namespace SFA.DAS.Payments.Audit.Application.Infrastructure.Ioc
{
    public class AuditModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterGeneric(typeof(PaymentsEventModelBatchService<>))
                .AsImplementedInterfaces()
                .SingleInstance();

            builder.RegisterGeneric(typeof(PaymentsEventModelBatchProcessor<>))
                .AsImplementedInterfaces()
                .InstancePerLifetimeScope();

            builder.RegisterType<AuditDataContextFactory>()
                .As<IAuditDataContextFactory>()
                .InstancePerLifetimeScope();

            builder.Register(ctx =>
                {
                    var configHelper = ctx.Resolve<IConfigurationHelper>();
                    var dbContext = new AuditDataContext(configHelper.GetConnectionString("PaymentsConnectionString"));
                    return dbContext;
                })
                .As<IAuditDataContext>()
                .InstancePerDependency();

            builder.Register(c =>
            {
                var appConfig = c.Resolve<IApplicationConfiguration>();
                return new ServiceBusManagement(appConfig.ServiceBusConnectionString
                    , appConfig.EndpointName
                    , c.Resolve<IPaymentLogger>());
            })
             .As<IServiceBusManagement>()
             .SingleInstance();
        }
    }
}