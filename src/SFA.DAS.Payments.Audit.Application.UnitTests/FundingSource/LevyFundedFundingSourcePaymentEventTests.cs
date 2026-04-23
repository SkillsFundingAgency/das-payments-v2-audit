using NUnit.Framework;
using SFA.DAS.Payments.FundingSource.Messages.Events;
using SFA.DAS.Payments.Model.Core.Audit;
using System;
using FluentAssertions;
using SFA.DAS.Payments.Model.Core.Entities;

namespace SFA.DAS.Payments.Audit.Application.UnitTests.FundingSource
{
    [TestFixture]
    public class LevyFundedFundingSourcePaymentEventTests : FundingSourceMappingTests<LevyFundingSourcePaymentEvent>
    {
        protected override LevyFundingSourcePaymentEvent CreatePaymentEvent()
        {
            return new LevyFundingSourcePaymentEvent
            {
                RequiredPaymentEventId = Guid.NewGuid()
            };
        }

        [TestCase(CourseType.Apprenticeship)]
        [TestCase(CourseType.ShortCourse)]
        public void Maps_CourseType(CourseType courseType)
        {
            PaymentEvent.CourseType = courseType;
            Mapper.Map<FundingSourceEventModel>(PaymentEvent).CourseType.Should().Be(PaymentEvent.CourseType);
        }
    }
}