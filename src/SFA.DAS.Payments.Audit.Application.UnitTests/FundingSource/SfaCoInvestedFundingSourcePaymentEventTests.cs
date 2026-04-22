using NUnit.Framework;
using SFA.DAS.Payments.FundingSource.Messages.Events;
using SFA.DAS.Payments.Model.Core.Audit;
using SFA.DAS.Payments.Model.Core.Entities;
using System;
using FluentAssertions;

namespace SFA.DAS.Payments.Audit.Application.UnitTests.FundingSource
{
    [TestFixture]
    public class SfaCoInvestedFundingSourcePaymentEventTests : FundingSourceMappingTests<SfaCoInvestedFundingSourcePaymentEvent>
    {
        protected override SfaCoInvestedFundingSourcePaymentEvent CreatePaymentEvent()
        {
            return new SfaCoInvestedFundingSourcePaymentEvent
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