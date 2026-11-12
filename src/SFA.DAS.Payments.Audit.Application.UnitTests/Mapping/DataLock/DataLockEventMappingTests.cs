using System;
using System.Collections.Generic;
using AutoMapper;
using FluentAssertions;
using NUnit.Framework;
using SFA.DAS.Payments.Audit.Application.Mapping;
using SFA.DAS.Payments.Audit.Application.Mapping.DataLock;
using SFA.DAS.Payments.Audit.Model;
using SFA.DAS.Payments.DataLocks.Messages.Events;
using SFA.DAS.Payments.Model.Core;
using SFA.DAS.Payments.Model.Core.Audit;

namespace SFA.DAS.Payments.Audit.Application.UnitTests.Mapping.DataLock
{
    public abstract class DataLockEventMappingTests<TSource> : PaymentEventMappingTests<TSource, DataLockEventModel>
        where TSource : DataLockEvent, new()
    {
        protected override void AddProfile(IMapperConfigurationExpression cfg)
        {
            cfg.AddProfile<DataLockProfile>();
        }

        protected override void PopulateCommonProperties(TSource paymentEvent)
        {
            base.PopulateCommonProperties(paymentEvent);
            paymentEvent.EarningEventId = Guid.NewGuid();
            paymentEvent.LearningAim.SequenceNumber = 112;
            paymentEvent.PriceEpisodes = new List<PriceEpisode>(){new PriceEpisode{ LearningAimSequenceNumber = 112, FundingLineType = "funding line type"}};
        }

        [Test]
        public void Maps_EarningEventId()
        {
            Mapper.Map<DataLockEventModel>(PaymentEvent).EarningEventId.Should().Be(PaymentEvent.EarningEventId);
        }

        [Test]
        public void Maps_ExternalEarningsId()
        {
            PaymentEvent.ExternalEarningsId = Guid.NewGuid();

            Mapper.Map<DataLockEventModel>(PaymentEvent).ExternalEarningsId.Should().Be(PaymentEvent.ExternalEarningsId);
        }

        [Test]
        public void Maps_Null_ExternalEarningsId()
        {
            PaymentEvent.ExternalEarningsId = null;

            Mapper.Map<DataLockEventModel>(PaymentEvent).ExternalEarningsId.Should().BeNull();
        }

        [Test]
        public void Maps_PriceEpisodes()
        {
            Mapper.Map<DataLockEventModel>(PaymentEvent).PriceEpisodes.Count.Should().Be(1);
        }
    }
}

