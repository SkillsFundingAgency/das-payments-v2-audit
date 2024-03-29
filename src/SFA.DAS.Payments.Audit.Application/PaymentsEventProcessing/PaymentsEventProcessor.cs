﻿using System;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using SFA.DAS.Payments.Audit.Application.PaymentsEventModelCache;
using SFA.DAS.Payments.Messages.Common.Events;
using SFA.DAS.Payments.Model.Core.Audit;

namespace SFA.DAS.Payments.Audit.Application.PaymentsEventProcessing
{
    public abstract class PaymentsEventProcessor<TPaymentsEvent, TPaymentsEventModel>
        where TPaymentsEvent : PaymentsEvent
        where TPaymentsEventModel : PaymentsEventModel
    {
        private readonly IPaymentsEventModelCache<TPaymentsEventModel> cache;
        private readonly IMapper mapper;

        protected PaymentsEventProcessor(IPaymentsEventModelCache<TPaymentsEventModel> cache, IMapper mapper)
        {
            this.cache = cache ?? throw new ArgumentNullException(nameof(cache));
            this.mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        public async Task ProcessPaymentsEvent(TPaymentsEvent message, CancellationToken cancellationToken)
        {
            var model = mapper.Map<TPaymentsEventModel>(message);  
            await cache.AddPayment(model, cancellationToken);
        }
    }
}