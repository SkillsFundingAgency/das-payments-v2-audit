﻿using Autofac;
using SFA.DAS.Payments.Application.Infrastructure.Ioc;
using SFA.DAS.Payments.Application.Infrastructure.Logging;
using SFA.DAS.Payments.Application.Infrastructure.Telemetry;
using SFA.DAS.Payments.Application.Messaging;
using SFA.DAS.Payments.Audit.Application.PaymentsEventProcessing.EarningEvent;
using SFA.DAS.Payments.Audit.EarningEventsService.Handlers;
using SFA.DAS.Payments.Core.Configuration;
using SFA.DAS.Payments.Messaging.Serialization;
using SFA.DAS.Payments.Model.Core.Audit;
using SFA.DAS.Payments.ServiceFabric.Core;

namespace SFA.DAS.Payments.Audit.EarningEventsService.Infrastructure.Ioc
{
    public class AuditEarningEventServiceModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.Register(c =>
                {
                    var appConfig = c.Resolve<IApplicationConfiguration>();
                    var configHelper = c.Resolve<IConfigurationHelper>();
                    return new StatelessServiceBusBatchCommunicationListener(configHelper.GetConnectionString("ServiceBusConnectionString"),
                        appConfig.EndpointName,
                        appConfig.FailedMessagesQueue,
                        c.Resolve<IPaymentLogger>(),
                        c.Resolve<IContainerScopeFactory>(),
                        c.Resolve<ITelemetry>(),
                        c.Resolve<IMessageDeserializer>(), 
                        c.Resolve<IApplicationMessageModifier>());
                })
                .As<IStatelessServiceBusBatchCommunicationListener>()
                .SingleInstance();

            builder.RegisterType<EarningEventMessageModifier>()
                .As<IApplicationMessageModifier>();

           builder.RegisterType<EarningEventModelHandler>()
                .As<IHandleMessageBatches<EarningEventModel>>()
                .InstancePerLifetimeScope();
        }
    }
}