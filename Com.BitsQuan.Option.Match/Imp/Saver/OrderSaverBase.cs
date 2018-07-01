using Com.BitsQuan.Option.Core;
using System;
using System.Collections.Generic;
using System.Data;

namespace Com.BitsQuan.Option.Match.Imp
{
    public abstract class OrderSaverBase : BulkSaver
    {
        public OrderSaverBase() : base(System.Configuration.ConfigurationManager.ConnectionStrings["OptionDb"].ToString(), "Orders") { }
        protected OrderSaverBase(string connstr, string tableName, int interval) : base(connstr, tableName, interval) { }
        protected override void CreateTable()
        {
            table = new DataTable();
            table.Columns.Add("FakeId", typeof(int));
            //[Id] [int] NOT NULL,
            table.Columns.Add("Id", typeof(int));
            //table.PrimaryKey =new DataColumn[]{ table.Columns[0]};
            //    [TraderId] [int] NOT NULL,
            table.Columns.Add("TraderId", typeof(int));
            //    [ContractId] [int] NOT NULL,
            table.Columns.Add("ContractId", typeof(int));
            //    [Direction] [int] NOT NULL,
            table.Columns.Add("Direction", typeof(int));
            //[ReportCount] [int] NOT NULL,
            table.Columns.Add("ReportCount", typeof(int));
            //    [Price] [decimal](18, 2) NOT NULL,
            table.Columns.Add("Price", typeof(decimal));
            //    [Count] [int] NOT NULL,
            table.Columns.Add("Count", typeof(int));
            //    [DoneCount] [int] NOT NULL,
            table.Columns.Add("DoneCount", typeof(int));
            //[TotalDoneCount] [int] NOT NULL,
            table.Columns.Add("TotalDoneCount", typeof(int));
            //    [OrderTime] [datetime] NOT NULL,
            table.Columns.Add("OrderTime", typeof(DateTime));
            //    [State] [int] NOT NULL,
            table.Columns.Add("State", typeof(int));
            //    [OrderType] [int] NOT NULL,
            table.Columns.Add("OrderType", typeof(int));
            //    [OrderPolicy] [int] NOT NULL,
            table.Columns.Add("OrderPolicy", typeof(int));
            //    [RequestStatus] [int] NOT NULL,
            table.Columns.Add("RequestStatus", typeof(int));
            //[DonePrice] [decimal](18, 2) NOT NULL,
            table.Columns.Add("DonePrice", typeof(decimal));
            //[IsBySystem] [bit] NOT NULL,
            table.Columns.Add("IsBySystem", typeof(bool));
            //[TotalDoneSum] [decimal](18, 2) NOT NULL,
            table.Columns.Add("TotalDoneSum", typeof(decimal));
            table.Columns.Add("Deatil", typeof(string));
            
        }

        protected override void AddToTable(object o)
        {
            var r = o as Order;
            //如果本次表中已有此id,则不再加入;
            //var oldRow = table.Rows.Find(r.Id);
            //if (oldRow != null) return;


            DataRow dr = table.NewRow();
            dr[1] = r.Id;
            dr[2] = r.Trader.Id;
            dr[3] = r.Contract.Id;
            dr[4] = r.Direction;
            dr[5] = r.ReportCount;
            dr[6] = r.Price;
            dr[7] = r.Count;
            dr[8] = r.DoneCount;
            dr[9] = r.TotalDoneCount;
            dr[10] = r.OrderTime;
            dr[11] = r.State;
            dr[12] = r.OrderType;
            dr[13] = r.OrderPolicy;
            dr[14] = r.RequestStatus;
            dr[15] = r.DonePrice;
            dr[16] = r.IsBySystem;
            dr[17] = r.TotalDoneSum;
            dr[18] = r.Detail;

            table.Rows.Add(dr);
        }

        protected override bool ShouldSave(object o)
        {
            return o is Order;
        }
    }
}
