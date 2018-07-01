using Com.BitsQuan.Option.Core;
using System;
using System.Data;

namespace Com.BitsQuan.Option.Match.Imp
{
    public class FuseSaver : BulkSaver
    {
        public FuseSaver() : base(System.Configuration.ConfigurationManager.ConnectionStrings["OptionDb"].ToString(), "FuseRecords",31) { }

        protected override void CreateTable()
        {
            table = new DataTable(); 
    //[Id] [int] NOT NULL,
            table.Columns.Add("Id", typeof(int));
    //[ContractId] [int] NOT NULL,
            table.Columns.Add("ContractId", typeof(int));
    //[StartTime] [datetime] NOT NULL,
            table.Columns.Add("StartTime", typeof(DateTime));
    //[FuseType] [int] NOT NULL,
            table.Columns.Add("FuseType", typeof(int));
    //[Price] [decimal](18, 2) NOT NULL,
            table.Columns.Add("Price", typeof(decimal));
    //        [MaxPrice] [decimal](18, 2) NOT NULL,
            table.Columns.Add("MaxPrice", typeof(decimal));
    //[MinPrice] [decimal](18, 2) NOT NULL,
            table.Columns.Add("MinPrice", typeof(decimal));
        }

        protected override void AddToTable(object o)
        {
            var r = o as FuseRecord;
            var dr = table.NewRow();
            dr[0] = r.Id;
            dr[1] = r.ContractId;
            dr[2] = r.StartTime;
            dr[3] = r.FuseType;
            dr[4] = r.Price;
            dr[5] = r.MaxPrice;
            dr[6] = r.MinPrice;
            table.Rows.Add(dr);
        }

        protected override bool ShouldSave(object o)
        {
            return o is FuseRecord;
        }
    }
}
