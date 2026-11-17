using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using NUnit.Framework;
using SFA.DAS.Payments.EarningEvents.Messages.Events;
using SFA.DAS.Payments.Model.Core;
using SFA.DAS.Payments.Model.Core.Audit;
using SFA.DAS.Payments.Model.Core.Entities;
using SFA.DAS.Payments.Model.Core.Incentives;

namespace SFA.DAS.Payments.Audit.Application.UnitTests.Mapping.EarningEvent
{
    public class GSLApprenticeshipEarningsEventMappingTests : ApprenticeshipContractTypeMappingTests<GSLApprenticeshipEarningsEvent>
    {
        public GSLApprenticeshipEarningsEventMappingTests() : base(ContractType.Act1)
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

        [Test]
        public void Maps_Hiring_Payment_Incentive_Earnings_To_Hiring_Payment_Transaction_Types()
        {
            PaymentEvent.IncentiveEarnings = new List<IncentiveEarning>
            {
                new IncentiveEarning
                {
                    Type = IncentiveEarningType.FirstEmployerHiringPayment,
                    Periods = new List<EarningPeriod>
                    {
                        new EarningPeriod { Period = 3, Amount = 500m, PriceEpisodeIdentifier = "PE-1" }
                    }.AsReadOnly()
                },
                new IncentiveEarning
                {
                    Type = IncentiveEarningType.SecondEmployerHiringPayment,
                    Periods = new List<EarningPeriod>
                    {
                        new EarningPeriod { Period = 3, Amount = 250m, PriceEpisodeIdentifier = "PE-1" }
                    }.AsReadOnly()
                }
            };

            var model = Mapper.Map<EarningEventModel>(PaymentEvent);

            var first = model.Periods.Single(x => x.TransactionType == TransactionType.FirstEmployerHiringPayment);
            first.Amount.Should().Be(500m);
            first.DeliveryPeriod.Should().Be(3);
            first.PriceEpisodeIdentifier.Should().Be("PE-1");
            first.EarningEventId.Should().Be(PaymentEvent.EventId);

            var second = model.Periods.Single(x => x.TransactionType == TransactionType.SecondEmployerHiringPayment);
            second.Amount.Should().Be(250m);
            second.DeliveryPeriod.Should().Be(3);
            second.PriceEpisodeIdentifier.Should().Be("PE-1");
            second.EarningEventId.Should().Be(PaymentEvent.EventId);
        }
    }
}
