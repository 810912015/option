using Com.BitsQuan.Option.Core;
using System;
using System.Data;

namespace Com.BitsQuan.Option.Match.Imp
{
    public class DealSaver : BulkSaver
    {
        public DealSaver() : base(System.Configuration.ConfigurationManager.ConnectionStrings["OptionDb"].ToString(), "Deals",11) { }

        protected override void CreateTable()
        {
            table = new DataTable();
            //        [Id] [int] NOT NULL,
            table.Columns.Add("Id", typeof(int));
            //[ContractId] [int] NOT NULL,
            table.Columns.Add("ContractId", typeof(int));
            //[MainOrderId] [int] NOT NULL,
            table.Columns.Add("MainOrderId", typeof(int));
            //[MainName] [nvarchar](max) NULL,
             table.Columns.Add("MainName", typeof(string));
            //[SlaveOrderId] [int] NOT NULL,
            table.Columns.Add("SlaveOrderId", typeof(int));
            //[SlaveName] [nvarchar](max) NULL,
             table.Columns.Add("SlaveName", typeof(string));
            //[When] [datetime] NOT NULL,
            table.Columns.Add("When", typeof(DateTime));
            //[IsPartialDeal] [bit] NOT NULL,
            table.Columns.Add("IsPartialDeal", typeof(bool));
            //[Count] [int] NOT NULL,
            table.Columns.Add("Count", typeof(int));
            //[DealType] [int] NOT NULL,
            table.Columns.Add("DealType", typeof(int));
            //[Price] [decimal](18, 2) NOT NULL,
            table.Columns.Add("Price", typeof(decimal));
        }

        protected override void AddToTable(object o)
        {
            var r = o as Deal;
            DataRow dr = table.NewRow();
            dr[0] = r.Id;
            dr[1] = r.ContractId;
            dr[2] = r.MainOrderId;
            dr[3] = r.MainName;
            dr[4] = r.SlaveOrderId;
            dr[5] = r.SlaveName;
            dr[6] = r.When;
            dr[7] = r.IsPartialDeal;
            dr[8] = r.Count;
            dr[9] =(int)r.DealType;
            dr[10] = r.Price;


            table.Rows.Add(dr);
        }

        protected override bool ShouldSave(object o)
        {
            return o is Deal;
        }
    }
}
