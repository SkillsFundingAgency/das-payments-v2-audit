using AutoMapper;
using SFA.DAS.Payments.EarningEvents.Messages.Events;
using SFA.DAS.Payments.Model.Core.Audit;
using SFA.DAS.Payments.Model.Core.Entities;
using System.Collections.Generic;
using System.Linq;

namespace SFA.DAS.Payments.Audit.Application.Mapping.EarningEvents
{
    public class GslShortCoursesResolver : IValueResolver<GSLShortCourseEarningsEvent, EarningEventModel, List<EarningEventPeriodModel>>
    {
        public List<EarningEventPeriodModel> Resolve(GSLShortCourseEarningsEvent source, EarningEventModel destination, List<EarningEventPeriodModel> destMember,
            ResolutionContext context)
        {
            var periods = destination.Periods ?? new List<EarningEventPeriodModel>();
            periods.AddRange(source.Earnings?
                    .SelectMany(shortCourseEarning => shortCourseEarning.Periods, (shortCourseEarning, period) => new { shortCourseEarning, period })
                    .Select(item => new EarningEventPeriodModel
                    {
                        TransactionType = (TransactionType)item.shortCourseEarning.Type,
                        AcademicYear = source.CollectionPeriod.AcademicYear,
                        CollectionPeriod = source.CollectionPeriod.Period,
                        DeliveryPeriod = item.period.Period,
                        Amount = item.period.Amount,
                        PriceEpisodeIdentifier = item.period.PriceEpisodeIdentifier,
                        SfaContributionPercentage = item.period.SfaContributionPercentage,
                        EarningEventId = source.EventId,
                    }) ?? new List<EarningEventPeriodModel>()
            );
            return periods;
        }
    }
}