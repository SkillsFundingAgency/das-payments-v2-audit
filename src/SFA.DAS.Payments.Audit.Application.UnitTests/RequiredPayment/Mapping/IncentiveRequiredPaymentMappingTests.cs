using System;
using FluentAssertions;
using NUnit.Framework;
using SFA.DAS.Payments.Model.Core.Audit;
using SFA.DAS.Payments.Model.Core.Entities;
using SFA.DAS.Payments.Model.Core.Incentives;
using SFA.DAS.Payments.RequiredPayments.Messages.Events;

namespace SFA.DAS.Payments.Audit.Application.UnitTests.RequiredPayment.Mapping
{
    [TestFixture]
    public class IncentiveRequiredPaymentMappingTests: RequiredPaymentsMappingTests<CalculatedRequiredIncentiveAmount>
    {
        protected override CalculatedRequiredIncentiveAmount CreatePaymentEvent()
        {
            return new CalculatedRequiredIncentiveAmount
            {
                ContractType = ContractType.Act2,
                
            };
        }

        [TestCaseSource(nameof(GetIncentiveTypes))]
        public void Maps_IncentiveTypes(IncentivePaymentType incentiveType)
        {
            PaymentEvent.Type = incentiveType;
            var model = Mapper.Map<RequiredPaymentEventModel>(PaymentEvent);
            model.TransactionType.Should().Be((TransactionType)PaymentEvent.Type);
        }

        public static Array GetIncentiveTypes()
        {
            return GetEnumValues<IncentivePaymentType>();
        }

        [TestCaseSource(nameof(GetContractTypes))]
        public void Maps_ContractType(ContractType contractType)
        {
            PaymentEvent.ContractType = contractType;
            Mapper.Map<RequiredPaymentEventModel>(PaymentEvent).ContractType.Should().Be(PaymentEvent.ContractType);
        }

        [Test]
        public void Maps_SfaContributionPercentage()
        {
            Mapper.Map<RequiredPaymentEventModel>(PaymentEvent).SfaContributionPercentage.Should().Be(1);
        }

        //TODO: Short courses can also contain incentive earnings, The CalculatedRequiredIncentiveAmount type should also have a type of course
        [TestCase(IncentivePaymentType.First16To18EmployerIncentive)]
        [TestCase(IncentivePaymentType.First16To18ProviderIncentive)]
        [TestCase(IncentivePaymentType.Second16To18EmployerIncentive)]
        [TestCase(IncentivePaymentType.Second16To18ProviderIncentive)]
        [TestCase(IncentivePaymentType.OnProgramme16To18FrameworkUplift)]
        [TestCase(IncentivePaymentType.Completion16To18FrameworkUplift)]
        [TestCase(IncentivePaymentType.Balancing16To18FrameworkUplift)]
        [TestCase(IncentivePaymentType.FirstDisadvantagePayment)]
        [TestCase(IncentivePaymentType.SecondDisadvantagePayment)]
        [TestCase(IncentivePaymentType.OnProgrammeMathsAndEnglish)]
        [TestCase(IncentivePaymentType.BalancingMathsAndEnglish)]
        [TestCase(IncentivePaymentType.LearningSupport)]
        [TestCase(IncentivePaymentType.CareLeaverApprenticePayment)]
        public void Maps_CourseType_Apprenticeship_Incentives(IncentivePaymentType paymentType)
        {
            PaymentEvent.Type = paymentType;
            Mapper.Map<RequiredPaymentEventModel>(PaymentEvent).CourseType.Should().Be(CourseType.Apprenticeship);
        }

        //        [TestCase(CourseType.Apprenticeship)]
        //        [TestCase(CourseType.FunctionalSkill)]
        [TestCase(IncentivePaymentType.OnProgrammeMathsAndEnglish)]
        [TestCase(IncentivePaymentType.BalancingMathsAndEnglish)]
        public void Maps_CourseType_For_Functional_Skills(IncentivePaymentType paymentType)
        {
            PaymentEvent.Type = paymentType;
            Mapper.Map<RequiredPaymentEventModel>(PaymentEvent).CourseType.Should().Be(CourseType.FunctionalSkill);
        }
    }
}