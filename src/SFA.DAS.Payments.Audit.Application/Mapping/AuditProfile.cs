﻿using AutoMapper;
using SFA.DAS.Payments.Audit.Model;
using SFA.DAS.Payments.FundingSource.Messages.Events;
using SFA.DAS.Payments.Messages.Core.Events;
using SFA.DAS.Payments.Model.Core;

namespace SFA.DAS.Payments.Audit.Application.Mapping
{
    public class AuditProfile: Profile
    {
        public AuditProfile()
        {
            CreateMap<IPeriodisedPaymentEvent, PeriodisedModel>()
                .Include<FundingSourcePaymentEvent,FundingSourceEventModel>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.EventId, opt => opt.MapFrom(source => source.EventId))
                .ForMember(dest => dest.AgreementId, opt => opt.Ignore())
                .ForMember(dest => dest.CollectionPeriod, opt => opt.MapFrom(source => source.CollectionPeriod.Period))
                .ForMember(dest => dest.CollectionYear, opt => opt.MapFrom(source => source.CollectionPeriod.GetCollectionYear()))
                .ForMember(dest => dest.Amount, opt => opt.MapFrom(source => source.AmountDue))
                .ForMember(dest => dest.ContractType, opt => opt.Ignore())
                .ForMember(dest => dest.DeliveryPeriod, opt => opt.MapFrom(source => source.DeliveryPeriod.Period))
                .ForMember(dest => dest.EventTime, opt => opt.MapFrom(source => source.EventTime))
                .ForMember(dest => dest.IlrSubmissionDateTime, opt => opt.MapFrom(source => source.IlrSubmissionDateTime))
                .ForMember(dest => dest.JobId, opt => opt.MapFrom(source => source.JobId))
                .ForMember(dest => dest.LearnerReferenceNumber, opt => opt.MapFrom(source => source.Learner.ReferenceNumber))
                .ForMember(dest => dest.LearnerUln, opt => opt.MapFrom(source => source.Learner.Uln))
                .ForMember(dest => dest.LearningAimPathwayCode, opt => opt.MapFrom(source => source.LearningAim.PathwayCode))
                .ForMember(dest => dest.LearningAimFrameworkCode, opt => opt.MapFrom(source => source.LearningAim.FrameworkCode))
                .ForMember(dest => dest.LearningAimFundingLineType, opt => opt.MapFrom(source => source.LearningAim.FundingLineType))
                .ForMember(dest => dest.LearningAimProgrammeType, opt => opt.MapFrom(source => source.LearningAim.ProgrammeType))
                .ForMember(dest => dest.LearningAimReference, opt => opt.MapFrom(source => source.LearningAim.Reference))
                .ForMember(dest => dest.LearningAimStandardCode, opt => opt.MapFrom(source => source.LearningAim.StandardCode))
                ;


        }
    }
}