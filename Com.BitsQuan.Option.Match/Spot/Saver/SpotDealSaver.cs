using Com.BitsQuan.Option.Core.Spot;
using Com.BitsQuan.Option.Match.Imp;
using System;

namespace Com.BitsQuan.Option.Match.Spot
{
    public class SpotDealSaver:BulkSaver
    {
        public SpotDealSaver()
            : base(System.Configuration.ConfigurationManager.ConnectionStrings["OptionDb"].ToString(), "SpotDeals")
        {

        }
        protected override void CreateTable()
        {

    //        [Id] [int] NOT NULL,
    //[CoinId] [int] NOT NULL,
    //[MainId] [int] NOT NULL,
    //[SlaveId] [int] NOT NULL,
    //[Count] [decimal](18, 2) NOT NULL,
    //[Price] [decimal](18, 2) NOT NULL,
    //[When] [datetime] NOT NULL,
    //[MainTraderName] [nvarchar](max) NULL,
    //[SlaveTraderId] [nvarchar](max) NULL,

            table = new System.Data.DataTable();
            //[Id] [int] NOT NULL,
            table.Columns.Add("Id", typeof(int));
            //[CoinId] [int] NOT NULL,
            table.Columns.Add("CoinId", typeof(int));
            //[MainId] [int] NOT NULL,
            table.Columns.Add("MainId", typeof(int));
            //[SlaveId] [int] NOT NULL,
            table.Columns.Add("SlaveId", typeof(int));
            //[Count] [decimal](18, 2) NOT NULL,
            table.Columns.Add("Count", typeof(decimal));
            //[Price] [decimal](18, 2) NOT NULL,
            table.Columns.Add("Price", typeof(decimal));
            //[When] [datetime] NOT NULL,
            table.Columns.Add("When", typeof(DateTime));
            //[MainTraderName] [nvarchar](max) NULL,
            table.Columns.Add("MainTraderName", typeof(string));
            //[SlaveTraderId] [nvarchar](max) NULL,
            table.Columns.Add("SlaveTraderId", typeof(string));
            table.Columns.Add("MainOrderDir", typeof(int));
        }

        protected override void AddToTable(object o)
        {
            var sd = o as SpotDeal;
            if (o == null) return;
            var dr = table.NewRow();
            dr[0] = sd.Id; 
            dr[1] = sd.CoinId; 
            dr[2] = sd.MainId; 
            dr[3] = sd.SlaveId;
            dr[4] = sd.Count; 
            dr[5] = sd.Price;  
            dr[6] = sd.When;
            dr[7] = sd.MainTraderName; 
            dr[8] = sd.SlaveTraderId;
            dr[9] = sd.MainOrderDir;
            table.Rows.Add(dr);
        }

        protected override bool ShouldSave(object o)
        {
            return o is SpotDeal;
        }
    }
}
