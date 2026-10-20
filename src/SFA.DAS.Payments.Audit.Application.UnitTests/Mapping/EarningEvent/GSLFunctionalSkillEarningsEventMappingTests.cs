using System;
using System.Linq;
using FluentAssertions;
using NUnit.Framework;
using SFA.DAS.Payments.EarningEvents.Messages.Events;
using SFA.DAS.Payments.Model.Core.Audit;
using SFA.DAS.Payments.Model.Core.Entities;

namespace SFA.DAS.Payments.Audit.Application.UnitTests.Mapping.EarningEvent
{
    [TestFixture(ContractType.Act1)]
    [TestFixture(ContractType.Act2)]
    public class GSLFunctionalSkillEarningsEventMappingTests : FunctionalSkillMappingTests<GSLFunctionalSkillEarningsEvent>
    {
        private readonly ContractType contractType;

        public GSLFunctionalSkillEarningsEventMappingTests(ContractType contractType)
        {
            this.contractType = contractType;
        }

        protected override GSLFunctionalSkillEarningsEvent CreatePaymentEvent()
        {
            var earningEvent = base.CreatePaymentEvent();
            earningEvent.ContractType = contractType;
            earningEvent.AgreementId = "GSL-FS-AGREEMENT-456";
            earningEvent.Earnings[0].Periods[0].SfaContributionPercentage = .8m;
            return earningEvent;
        }

        protected override void PopulateCommonProperties(GSLFunctionalSkillEarningsEvent paymentEvent)
        {
            base.PopulateCommonProperties(paymentEvent);
            paymentEvent.EventId = Guid.NewGuid();
            paymentEvent.LearningAim.LearningType = LearningType.MathsAndEnglish;
            paymentEvent.LearningAim.CourseCode = "GSL-MATHS-001";
        }

        [Test]
        public void Maps_AgreementId()
        {
            Mapper.Map<EarningEventModel>(PaymentEvent).AgreementId.Should().Be(PaymentEvent.AgreementId);
        }

        [Test]
        public void Maps_EventType()
        {
            Mapper.Map<EarningEventModel>(PaymentEvent).EventType.Should().Be(typeof(GSLFunctionalSkillEarningsEvent).FullName);
        }

        [Test]
        public void Maps_Earning_Period()
        {
            var earning = PaymentEvent.Earnings.Single();
            var expected = earning.Periods.Single();
            var model = Mapper.Map<EarningEventModel>(PaymentEvent);
            var period = model.Periods.Should().ContainSingle().Which;

            period.TransactionType.Should().Be((TransactionType)earning.Type);
            period.Amount.Should().Be(expected.Amount);
            period.CollectionPeriod.Should().Be(PaymentEvent.CollectionPeriod.Period);
            period.AcademicYear.Should().Be(PaymentEvent.CollectionPeriod.AcademicYear);
            period.DeliveryPeriod.Should().Be(expected.Period);
            period.EarningEventId.Should().Be(PaymentEvent.EventId);
            period.PriceEpisodeIdentifier.Should().Be(expected.PriceEpisodeIdentifier);
            period.SfaContributionPercentage.Should().Be(expected.SfaContributionPercentage);
        }
    }
}
