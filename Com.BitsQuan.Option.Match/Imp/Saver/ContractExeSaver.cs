using Com.BitsQuan.Option.Core;
using System;
using System.Data;

namespace Com.BitsQuan.Option.Match.Imp
{  
    public class ContractExeSaver : BulkSaver
    {
        public ContractExeSaver() : base(System.Configuration.ConfigurationManager.ConnectionStrings["OptionDb"].ToString(), "ContractExecuteRecords",3) { }
        protected override void CreateTable()
        {
            table = new DataTable();
            //        [Id] [int] IDENTITY(1,1) NOT NULL,
            table.Columns.Add("Id", typeof(int));
            //[ContractId] [int] NOT NULL,
            table.Columns.Add("ContractId", typeof(int));
            //[TraderId] [int] NOT NULL,
            table.Columns.Add("TraderId", typeof(int));
            //[PosType] [int] NOT NULL,
            table.Columns.Add("PosType", typeof(int));
            //[Count] [int] NOT NULL,
            table.Columns.Add("Count", typeof(int));
            //[BasePrice] [decimal](18, 2) NOT NULL,
            table.Columns.Add("BasePrice", typeof(decimal));
            //[IsAddTo] [bit] NOT NULL,
            table.Columns.Add("IsAddTo", typeof(bool));
            //[Total] [decimal](18, 2) NOT NULL,
            table.Columns.Add("Total", typeof(decimal));
            
            //[When] [datetime] NOT NULL,
            table.Columns.Add("When", typeof(DateTime));
        }

        protected override void AddToTable(object o)
        {
            var r = o as ContractExecuteRecord;
            if (r == null) return;
            var dr = table.NewRow();
            dr[0] = r.Id;
            dr[1] = r.ContractId;
            dr[2] = r.TraderId;
            dr[3] = r.PosType;
            dr[4] = r.Count;
            dr[5] = r.BasePrice;
            dr[6] = r.IsAddTo;
            dr[7] = r.Total;
           
            dr[8] = r.When;
            table.Rows.Add(dr);
        }

        protected override bool ShouldSave(object o)
        {
            return o is ContractExecuteRecord;
        }
    }
    
}
