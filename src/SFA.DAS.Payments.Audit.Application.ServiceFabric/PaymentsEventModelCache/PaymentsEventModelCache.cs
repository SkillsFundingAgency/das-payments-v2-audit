﻿using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.ServiceFabric.Data.Collections;
using SFA.DAS.Payments.Audit.Application.PaymentsEventModelCache;
using SFA.DAS.Payments.Audit.Application.ServiceFabric.Infrastructure;
using SFA.DAS.Payments.Audit.Model;
using SFA.DAS.Payments.ServiceFabric.Core;

namespace SFA.DAS.Payments.Audit.Application.ServiceFabric.PaymentsEventModelCache
{
    public class PaymentsEventModelCache<T>: IPaymentsEventModelCache<T> where T: PaymentsEventModel
    {
        private readonly IReliableStateManagerTransactionProvider transactionProvider;
        private readonly IReliableConcurrentQueue<T> queue;

        public PaymentsEventModelCache(IReliableStateManagerProvider reliableStateManagerProvider, IReliableStateManagerTransactionProvider transactionProvider)
        {
            this.transactionProvider = transactionProvider ?? throw new ArgumentNullException(nameof(transactionProvider));
            queue = reliableStateManagerProvider.Current.GetOrAddAsync<IReliableConcurrentQueue<T>>("PaymentsEventModelQueue").Result;
        }

        public async Task AddPayment(T paymentsEventModel)
        {
            await queue.EnqueueAsync(transactionProvider.Current, paymentsEventModel, CancellationToken.None);
        }

        public async Task<List<T>> GetPayments(int batchSize)
        {
            var list = new List<T>();
            for (var i = 0; i < batchSize; i++)
            {
                var ret = await queue.TryDequeueAsync(transactionProvider.Current, default(CancellationToken));
                if (ret.HasValue)
                    list.Add(ret.Value);
                else
                    break; //no more items
            }
            return list;
        }
    }
}