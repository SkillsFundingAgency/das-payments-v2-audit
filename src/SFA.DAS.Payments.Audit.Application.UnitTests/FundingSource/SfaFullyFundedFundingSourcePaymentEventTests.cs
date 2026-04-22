using NUnit.Framework;
using SFA.DAS.Payments.FundingSource.Messages.Events;
using SFA.DAS.Payments.Model.Core.Audit;
using SFA.DAS.Payments.Model.Core.Entities;
using System;
using FluentAssertions;

namespace SFA.DAS.Payments.Audit.Application.UnitTests.FundingSource
{
    [TestFixture]
    public class SfaFullyFundedFundingSourcePaymentEventTests : FundingSourceMappingTests<SfaFullyFundedFundingSourcePaymentEvent>
    {
        protected override SfaFullyFundedFundingSourcePaymentEvent CreatePaymentEvent()
        {
            return new SfaFullyFundedFundingSourcePaymentEvent
            {
                RequiredPaymentEventId = Guid.NewGuid()
            };
        }

        [TestCase(CourseType.Apprenticeship)]
        [TestCase(CourseType.FunctionalSkill)]
        public void Maps_CourseType(CourseType courseType)
        {
            PaymentEvent.CourseType = courseType;
            Mapper.Map<FundingSourceEventModel>(PaymentEvent).CourseType.Should().Be(PaymentEvent.CourseType);
        }

        [TestCase(LearningType.Apprenticeship)]
        [TestCase(LearningType.FoundationApprenticeship)]
        public void Maps_LearningType(LearningType learningType)
        {
            PaymentEvent.LearningAim.LearningType = learningType;
            Mapper.Map<FundingSourceEventModel>(PaymentEvent).LearningType.Should().Be(learningType);
        }
    }
}