using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using NUnit.Framework;
using SFA.DAS.Payments.Audit.Model;
using SFA.DAS.Payments.EarningEvents.Messages.Events;
using SFA.DAS.Payments.Model.Core;
using SFA.DAS.Payments.Model.Core.Audit;
using SFA.DAS.Payments.Model.Core.Entities;
using SFA.DAS.Payments.Model.Core.Incentives;

namespace SFA.DAS.Payments.Audit.Application.UnitTests.Mapping.EarningEvent
{
    public class
        ApprenticeshipContractType1MappingTests : ApprenticeshipContractTypeMappingTests<
        ApprenticeshipContractType1EarningEvent>
    {
        public ApprenticeshipContractType1MappingTests() : base(ContractType.Act1)
        {


        }

        [Test]
        public void Maps_AgreementId()
        {
            PaymentEvent.AgreementId = "AGREEMENTID";

            var model = Mapper.Map<EarningEventModel>(PaymentEvent);
            model.AgreementId.Should().Be(PaymentEvent.AgreementId);
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