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
using SFA.DAS.Payments.EarningEvents.Messages;

namespace SFA.DAS.Payments.Audit.Application.UnitTests.Mapping.EarningEvent
{
    public class GSLApprenticeshipEarningsEventTests : PaymentEventMappingTests<GSLApprenticeshipEarningsEvent, EarningEventModel>
    {
        protected override void AddProfile(IMapperConfigurationExpression cfg)
        {
            cfg.AddProfile<EarningEventProfile>();
        }

        protected override GSLApprenticeshipEarningsEvent CreatePaymentEvent()
        {
            return new GSLApprenticeshipEarningsEvent
            {
                ContractType = ContractType.Act1,
                AgreementId = "agreement-id",
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
                OnProgrammeEarnings = new List<Payments.Model.Core.OnProgramme.OnProgrammeEarning>
                {
                    new Payments.Model.Core.OnProgramme.OnProgrammeEarning
                    {
                        Type = Payments.Model.Core.OnProgramme.OnProgrammeEarningType.Learning,
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
                }
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

            model.CourseType.Should().Be((byte)CourseType.Apprenticeship);
        }

        [Test]
        public void Maps_AgreementId()
        {
            var model = Mapper.Map<EarningEventModel>(PaymentEvent);

            model.AgreementId.Should().Be(PaymentEvent.AgreementId);
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
                PaymentEvent.OnProgrammeEarnings.SelectMany(x => x.Periods).Count());
        }
    }
}