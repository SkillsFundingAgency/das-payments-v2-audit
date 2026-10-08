using AutoMapper;
using FluentAssertions;
using NUnit.Framework;
using SFA.DAS.Payments.Audit.Application.Mapping.EarningEvents;
using SFA.DAS.Payments.Audit.Application.UnitTests.Mapping;
using SFA.DAS.Payments.EarningEvents.Messages.Events;
using SFA.DAS.Payments.Model.Core;
using SFA.DAS.Payments.Model.Core.Audit;
using SFA.DAS.Payments.Model.Core.Entities;
using SFA.DAS.Payments.Model.Core.Incentives;
using System;
using System.Collections.Generic;
using System.Linq;

public class GSLFunctionalSkillEarningsEventTests : PaymentEventMappingTests<GSLFunctionalSkillEarningsEvent, EarningEventModel>
{
    protected override void AddProfile(IMapperConfigurationExpression cfg)
    {
        cfg.AddProfile<EarningEventProfile>();
    }

    protected override GSLFunctionalSkillEarningsEvent CreatePaymentEvent()
    {
        return new GSLFunctionalSkillEarningsEvent
        {
            ContractType = ContractType.Act1,
            ExternalEarningsId = Guid.NewGuid(),
            PriceEpisodes = new List<PriceEpisode>
            {
                new PriceEpisode
                {
                    Identifier = "pe-1",
                    LearningAimSequenceNumber = 112,
                    FundingLineType = "funding line type"
                }
            },
            Earnings = new List<FunctionalSkillEarning>
            {
                new FunctionalSkillEarning
                {
                    Type = FunctionalSkillType.BalancingMathsAndEnglish,
                    Periods = new List<EarningPeriod>
                    {
                        new EarningPeriod
                        {
                            Period = 1,
                            Amount = 100,
                            PriceEpisodeIdentifier = "pe-1"
                        }
                    }.AsReadOnly()
                }
            }.AsReadOnly()
        };
    }

    [Test]
    public void Maps_ContractType()
    {
        var model = Mapper.Map<EarningEventModel>(PaymentEvent);

        model.ContractType.Should().Be((byte)PaymentEvent.ContractType);
    }

    [Test]
    public void Maps_CourseType()
    {
        var model = Mapper.Map<EarningEventModel>(PaymentEvent);

        model.CourseType.Should().Be((byte)CourseType.FunctionalSkill);
    }

    [Test]
    public void Maps_ExternalEarningsId()
    {
        var model = Mapper.Map<EarningEventModel>(PaymentEvent);

        model.ExternalEarningsId.Should().Be(PaymentEvent.ExternalEarningsId);
    }

    [Test]
    public void Maps_Periods()
    {
        var model = Mapper.Map<EarningEventModel>(PaymentEvent);

        model.Periods.Should().HaveCount(
            PaymentEvent.Earnings.SelectMany(x => x.Periods).Count());
    }
}