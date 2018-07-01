using Com.BitsQuan.Option.Core;
using System;
using System.Data;

namespace Com.BitsQuan.Option.Match.Imp
{  

    public class UserPositionSaver : BulkSaver
    {
        public UserPositionSaver() : base(System.Configuration.ConfigurationManager.ConnectionStrings["OptionDb"].ToString(), "UserPositions",7) { }
        protected override void CreateTable()
        {
            

            table = new DataTable();
    //        [Id] [int] NOT NULL,
            table.Columns.Add("Id", typeof(int));
    //[OrderId] [int] NOT NULL,
            table.Columns.Add("OrderId", typeof(int));
    //[TraderId] [int] NULL,
            table.Columns.Add("TraderId", typeof(int));
            //[Count] [int] NOT NULL,
            table.Columns.Add("Count", typeof(int));
    //[DealTime] [datetime] NOT NULL,
            table.Columns.Add("DealTime", typeof(DateTime));
        }

        protected override void AddToTable(object o)
        {
            var r = o as UserPosition;
            DataRow dr = table.NewRow();
            dr[0] = r.Id;
            dr[1] = r.Order.Id;
            dr[2] = r.Trader.Id;
            dr[3] = r.Count;
            dr[4] = r.DealTime;

            table.Rows.Add(dr);
        }

        protected override bool ShouldSave(object o)
        {
            return o is UserPosition;
        }
    }
}
