using Com.BitsQuan.Option.Core.Spot;
using Com.BitsQuan.Option.Match.Imp;
using System;
using System.Data;

namespace Com.BitsQuan.Option.Match.Spot
{
    public class SpotOrderSaver:BulkSaver
    {
        public SpotOrderSaver()
            : base(System.Configuration.ConfigurationManager.ConnectionStrings["OptionDb"].ToString(), "SpotOrders")
        {

        }

        public SpotOrderSaver(string conn,string tableName)
            : base(conn, tableName)
        {

        }
        protected override void CreateTable()
        {
            table = new System.Data.DataTable();
            //[Id] [int] NOT NULL,
            table.Columns.Add("Id", typeof(int));
            table.PrimaryKey = new DataColumn[] { table.Columns[0] };
            //[TraderId] [int] NOT NULL,
            table.Columns.Add("TraderId", typeof(int));
            //[CoinId] [int] NOT NULL,
            table.Columns.Add("CoinId", typeof(int));
            //[Direction] [int] NOT NULL,
            table.Columns.Add("Direction", typeof(int));
            //[ReportCount] [decimal](18, 2) NOT NULL,
            table.Columns.Add("ReportCount", typeof(decimal));
            //[Price] [decimal](18, 2) NOT NULL,
            table.Columns.Add("Price", typeof(decimal));
            //[Count] [decimal](18, 2) NOT NULL,
            table.Columns.Add("Count", typeof(decimal));
            //[DoneCount] [decimal](18, 2) NOT NULL,
            table.Columns.Add("DoneCount", typeof(decimal));
            //[TotalDoneCount] [decimal](18, 2) NOT NULL,
            table.Columns.Add("TotalDoneCount", typeof(decimal));
            //[OrderTime] [datetime] NOT NULL,
            table.Columns.Add("OrderTime", typeof(DateTime));
            //[State] [int] NOT NULL,
            table.Columns.Add("State", typeof(int));
            //[RequestStatus] [int] NOT NULL,
            table.Columns.Add("RequestStatus", typeof(int));
            //[DonePrice] [decimal](18, 2) NOT NULL,
            table.Columns.Add("DonePrice", typeof(decimal));
            //[IsBySystem] [bit] NOT NULL,
            table.Columns.Add("IsBySystem", typeof(bool ));
            //[TotalDoneSum] [decimal](18, 2) NOT NULL,
            table.Columns.Add("TotalDoneSum", typeof(decimal));
            //[Detail] [nvarchar](max) NULL,
            table.Columns.Add("Detail", typeof(string));
            table.Columns.Add("OrderPolicy", typeof(int));
        }

        protected override void AddToTable(object o)
        {
            var so = o as SpotOrder;
            //如果本次表中已有此id,则不再加入;
            var oldRow = table.Rows.Find(so.Id);
            if (oldRow != null) return;
            var dr = table.NewRow();
            dr[0] = so.Id; dr[1] = so.TraderId; dr[2] = so.CoinId; dr[3] = so.Direction; dr[4] = so.ReportCount; dr[5] = so.Price;
            dr[6] = so.Count; dr[7] = so.DoneCount; dr[8] = so.TotalDoneCount; dr[9] = so.OrderTime; dr[10] = so.State;
            dr[11] = so.RequestStatus; dr[12] = so.DonePrice; dr[13] = so.IsBySystem; dr[14] = so.TotalDoneSum; dr[15] = so.Detail;
            dr[16] = so.OrderPolicy;
            table.Rows.Add(dr);
        }

        protected override bool ShouldSave(object o)
        {
            return o is SpotOrder;
        }
    }

    public class TempSpotOrderSaver : SpotOrderSaver
    {
        public TempSpotOrderSaver()
            : base(System.Configuration.ConfigurationManager.ConnectionStrings["OptionDb"].ToString(), "TempSpotOrders")
        {

        }
    }
}
