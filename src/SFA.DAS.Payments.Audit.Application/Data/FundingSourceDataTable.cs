﻿using System.Collections.Generic;
using System.Data;
using SFA.DAS.Payments.Audit.Model;

namespace SFA.DAS.Payments.Audit.Application.Data
{
    public class FundingSourceDataTable: PeriodisedPaymentsEventModelDataTable<FundingSourceEventModel>
    {
        public FundingSourceDataTable()
        {
            DataTable.Columns.AddRange(new[]
            {
                new DataColumn("RequiredPaymentEventId"),
                new DataColumn("FundingSourceType"),
            });
        }

        protected override DataRow CreateDataRow(FundingSourceEventModel eventModel)
        {
            var dataRow = base.CreateDataRow(eventModel);
            dataRow["RequiredPaymentEventId"] = eventModel.RequiredPaymentEventId;
            dataRow["FundingSourceType"] = (byte)eventModel.FundingSource;
            return dataRow;
        }

        public override string TableName => "FundingSourceEvent";
    }
}