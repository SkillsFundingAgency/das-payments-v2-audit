using FluentAssertions;
using NUnit.Framework;
using SFA.DAS.Payments.Model.Core.Audit;
using SFA.DAS.Payments.Model.Core.Entities;
using SFA.DAS.Payments.RequiredPayments.Messages.Events;
using System;

namespace SFA.DAS.Payments.Audit.Application.UnitTests.RequiredPayment.Mapping
{
    [TestFixture]
    public class LevyRequiredPaymentMappingTests : RequiredPaymentsMappingTests<CalculatedRequiredLevyAmount>
    {
        protected override CalculatedRequiredLevyAmount CreatePaymentEvent()
        {
            return new CalculatedRequiredLevyAmount
            {
                ContractType = ContractType.Act1,
                ApprenticeshipId = 400L,
                ApprenticeshipPriceEpisodeId = 800L
            };
        }

        [TestCase(CourseType.Apprenticeship, LearningType.Apprenticeship)]
        [TestCase(CourseType.Apprenticeship, LearningType.FoundationApprenticeship)]
        [TestCase(CourseType.ShortCourse, LearningType.ApprenticeshipUnit)]
        public void Maps_CourseType(CourseType courseType, LearningType learningType)
        {
            PaymentEvent.CourseType = courseType;
            Mapper.Map<RequiredPaymentEventModel>(PaymentEvent).CourseType.Should().Be(courseType);
        }
    }
}