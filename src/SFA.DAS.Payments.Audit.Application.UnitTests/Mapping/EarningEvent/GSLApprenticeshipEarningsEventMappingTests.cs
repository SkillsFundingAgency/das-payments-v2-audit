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
    public class GSLApprenticeshipEarningsEventMappingTests : ApprenticeshipContractTypeMappingTests<GSLApprenticeshipEarningsEvent>
    {
        public GSLApprenticeshipEarningsEventMappingTests(ContractType contractType) : base(contractType)
        {
        }

        protected override GSLApprenticeshipEarningsEvent CreatePaymentEvent()
        {
            var earningEvent = base.CreatePaymentEvent();
            earningEvent.ContractType = contractType;
            earningEvent.AgreementId = "GSL-AGREEMENT-123";
            earningEvent.SfaContributionPercentage = .95m;
            earningEvent.OnProgrammeEarnings.First().Periods[0].SfaContributionPercentage = .9m;
            return earningEvent;
        }

        protected override void PopulateCommonProperties(GSLApprenticeshipEarningsEvent paymentEvent)
        {
            base.PopulateCommonProperties(paymentEvent);
            paymentEvent.EventId = Guid.NewGuid();
            paymentEvent.LearningAim.LearningType = LearningType.FoundationApprenticeship;
            paymentEvent.LearningAim.CourseCode = "GSL-APP-001";
        }

        [Test]
        public void Maps_AgreementId()
        {
            Mapper.Map<EarningEventModel>(PaymentEvent).AgreementId.Should().Be(PaymentEvent.AgreementId);
        }

        [Test]
        public void Maps_SfaContributionPercentage()
        {
            Mapper.Map<EarningEventModel>(PaymentEvent).SfaContributionPercentage.Should().Be(PaymentEvent.SfaContributionPercentage);
        }

        [Test]
        public void Maps_EventType()
        {
            Mapper.Map<EarningEventModel>(PaymentEvent).EventType.Should().Be(typeof(GSLApprenticeshipEarningsEvent).FullName);
        }

        [Test]
        public void Maps_OnProgramme_Earning_Period()
        {
            var earning = PaymentEvent.OnProgrammeEarnings.Single();
            var expected = earning.Periods.Single();
            var model = Mapper.Map<EarningEventModel>(PaymentEvent);
            var period = model.Periods.Single(x => x.TransactionType == (TransactionType)earning.Type);

            period.Amount.Should().Be(expected.Amount);
            period.CollectionPeriod.Should().Be(PaymentEvent.CollectionPeriod.Period);
            period.AcademicYear.Should().Be(PaymentEvent.CollectionPeriod.AcademicYear);
            period.DeliveryPeriod.Should().Be(expected.Period);
            period.EarningEventId.Should().Be(PaymentEvent.EventId);
            period.PriceEpisodeIdentifier.Should().Be(expected.PriceEpisodeIdentifier);
            period.SfaContributionPercentage.Should().Be(expected.SfaContributionPercentage);
        }

        [Test]
        public void Maps_Incentive_Earning_Period()
        {
            var earning = PaymentEvent.IncentiveEarnings.Single();
            var expected = earning.Periods.Single();
            var model = Mapper.Map<EarningEventModel>(PaymentEvent);
            var period = model.Periods.Single(x => x.TransactionType == (TransactionType)earning.Type);

            period.Amount.Should().Be(expected.Amount);
            period.CollectionPeriod.Should().Be(PaymentEvent.CollectionPeriod.Period);
            period.AcademicYear.Should().Be(PaymentEvent.CollectionPeriod.AcademicYear);
            period.DeliveryPeriod.Should().Be(expected.Period);
            period.EarningEventId.Should().Be(PaymentEvent.EventId);
            period.PriceEpisodeIdentifier.Should().Be(expected.PriceEpisodeIdentifier);
            period.CensusDate.Should().Be(earning.CensusDate);
        }
    }
}
