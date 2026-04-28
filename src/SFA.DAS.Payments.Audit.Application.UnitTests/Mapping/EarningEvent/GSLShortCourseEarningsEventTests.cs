using AutoMapper;
using Microsoft.Azure.Amqp.Framing;
using NUnit.Framework;
using SFA.DAS.Payments.Audit.Application.Mapping.EarningEvents;
using SFA.DAS.Payments.EarningEvents.Messages.Events;
using SFA.DAS.Payments.Messages.Common.Events;
using SFA.DAS.Payments.Model.Core;
using SFA.DAS.Payments.Model.Core.Audit;
using SFA.DAS.Payments.Model.Core.Entities;
using SFA.DAS.Payments.Tests.Core.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;

namespace SFA.DAS.Payments.Audit.Application.UnitTests.Mapping.EarningEvent
{
    public class GSLShortCourseEarningsEventTests : PaymentEventMappingTests<GSLShortCourseEarningsEvent, EarningEventModel>
    {
        protected override void AddProfile(IMapperConfigurationExpression cfg)
        {
            cfg.AddProfile<EarningEventProfile>();
        }

        protected override GSLShortCourseEarningsEvent CreatePaymentEvent()
        {
            return new GSLShortCourseEarningsEvent
            {
                Learner = new Learner
                {
                    ReferenceNumber = "LR-12345",
                    Uln = 12345678
                },
                CollectionPeriod = new CollectionPeriodBuilder().WithDate(DateTime.Today).Build(),
                IlrSubmissionDateTime = DateTime.UtcNow,
                JobId = 1234,
                LearningAim = new LearningAim
                {
                    FundingLineType = "funding line type",
                    FrameworkCode = 0,
                    StandardCode = 0,
                    PathwayCode = 0,
                    ProgrammeType = 97,
                    Reference = "ZSC00001",
                    SequenceNumber = 112,
                    CourseCode = "ZSC00001",
                    LearningType = LearningType.ApprenticeshipUnit
                },
                Ukprn = 23456,
                AgeAtStartOfLearning = 17,
                PriceEpisodes = new List<PriceEpisode>
                {
                    new PriceEpisode
                    {
                        Identifier = "pe-1",
                        TotalNegotiatedPrice1 = 10,
                        TotalNegotiatedPrice2 = 11,
                        TotalNegotiatedPrice3 = 12,
                        TotalNegotiatedPrice4 = 13,
                        CompletionAmount = 100,
                        InstalmentAmount = 10,
                        Completed = true,
                        NumberOfInstalments = 10,
                        PlannedEndDate = DateTime.Today,
                        EffectiveTotalNegotiatedPriceStartDate = DateTime.Today.AddMonths(-1),
                        FundingLineType = "funding line type",
                        LearningAimSequenceNumber = 112
                    }
                }
            };
        }


        [Test]
        public void Maps_PriceEpisodes()
        {
            var model = Mapper.Map<EarningEventModel>(PaymentEvent);
            model.PriceEpisodes.Count.Should().Be(PaymentEvent.PriceEpisodes.Count());
        }

        [Test]
        public void Maps_AimSeqNumber()
        {
            PaymentEvent.LearningAim.SequenceNumber = 101;
            var model = Mapper.Map<EarningEventModel>(PaymentEvent);
            model.LearningAimSequenceNumber.Should().Be(101);
        }
        
        [Test]
        public void Maps_AgeAtStartOfLearning()
        {
            PaymentEvent.AgeAtStartOfLearning = 17;
            var model = Mapper.Map<EarningEventModel>(PaymentEvent);
            model.AgeAtStartOfLearning.Should().Be(17);
        }
        
        [Test]
        public void Maps_CourseType()
        {
            var model = Mapper.Map<EarningEventModel>(PaymentEvent);
            model.CourseType.Should().Be((byte)CourseType.ShortCourse);
        }
    }
}