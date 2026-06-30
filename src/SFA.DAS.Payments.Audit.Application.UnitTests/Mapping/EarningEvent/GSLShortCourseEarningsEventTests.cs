using AutoMapper;
using NUnit.Framework;
using SFA.DAS.Payments.Audit.Application.Mapping.EarningEvents;
using SFA.DAS.Payments.EarningEvents.Messages.Events;
using SFA.DAS.Payments.Model.Core;
using SFA.DAS.Payments.Model.Core.Audit;
using SFA.DAS.Payments.Model.Core.Entities;
using SFA.DAS.Payments.Tests.Core.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using SFA.DAS.Payments.EarningEvents.Messages;

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
                    FundingLineType = "", // not populated by GSO Earnings Bridge
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
                        FundingLineType = "gso funding line type", // populated by GSO Earnings Bridge
                        LearningAimSequenceNumber = 112
                    }
                },
                Earnings = new List<ShortCourseEarning>()
                {
                    new ShortCourseEarning
                    {
                        Type = ShortCourseEarningType.Milestone1,
                        Periods = new List<EarningPeriod>()
                        {
                            new EarningPeriod
                            {
                                AccountId = 12345,
                                TransferSenderAccountId = 223344,
                                Amount = 1000m,
                                SfaContributionPercentage = 0.95m,
                                PriceEpisodeIdentifier = "PEI",
                                ApprenticeshipId = 11223344,
                                Period = 1,
                                ApprenticeshipEmployerType = ApprenticeshipEmployerType.NonLevy
                            }
                        }
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

        [Test]
        public void Maps_Periods()
        {
            var model = Mapper.Map<EarningEventModel>(PaymentEvent);

            model.Periods.Should().NotBeNull();
            model.Periods.Count.Should().Be(1);
            var period = model.Periods[0];
            var earning = PaymentEvent.Earnings.ToList()[0];
            var earningEventPeriod = earning.Periods.ToList()[0];
            period.TransactionType.Should().Be((TransactionType)earning.Type);
            period.AcademicYear.Should().Be(PaymentEvent.CollectionPeriod.AcademicYear);
            period.CollectionPeriod.Should().Be(PaymentEvent.CollectionPeriod.Period);
            period.DeliveryPeriod.Should().Be(earningEventPeriod.Period);
            period.Amount.Should().Be(earningEventPeriod.Amount);
            period.PriceEpisodeIdentifier.Should().Be(earningEventPeriod.PriceEpisodeIdentifier);
            period.SfaContributionPercentage.Should().Be(earningEventPeriod.SfaContributionPercentage);
            period.EarningEventId.Should().Be(PaymentEvent.EventId);
        }

        [Test]
        public void Maps_Empty_Periods()
        {
            PaymentEvent.Earnings = new List<ShortCourseEarning>
            {
                new ShortCourseEarning
                {
                    Periods = new List<EarningPeriod>()
                }
            };

            var model = Mapper.Map<EarningEventModel>(PaymentEvent);

            model.Periods.Should().NotBeNull();
            model.Periods.Count.Should().Be(0);
        }

        [Test]
        public void Maps_FundingLineType()
        {
            var model = Mapper.Map<EarningEventModel>(PaymentEvent);

            model.LearningAimFundingLineType.Should().Be(PaymentEvent.PriceEpisodes.ToList()[0].FundingLineType);
        }

    }
}