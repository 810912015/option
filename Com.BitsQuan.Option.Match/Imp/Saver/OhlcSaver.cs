using Com.BitsQuan.Option.Core;
using System;
using System.Data;

namespace Com.BitsQuan.Option.Match.Imp
{
    public class OhlcSaver : BulkSaver
    {
        public OhlcSaver() : this(System.Configuration.ConfigurationManager.ConnectionStrings["OptionDb"].ToString(), "Ohlcs") { }
        public OhlcSaver(string connstr, string tableName) : base(connstr, tableName) { }
         
        protected override void CreateTable()
        {
            table = new DataTable();
            //        [Id] [int] NOT NULL,
            table.Columns.Add("Id", typeof(int));
            //[ContractId] [int] NOT NULL,
            table.Columns.Add("WhatId", typeof(int));
            //[OhlcType]
            table.Columns.Add("OhlcType", typeof(int));
            
            //[When] [float] NOT NULL,
            table.Columns.Add("When", typeof(float));
            //[WhenInDt] [datetime] NOT NULL,
            table.Columns.Add("WhenInDt", typeof(DateTime));
            //[Open] [float] NOT NULL,
            table.Columns.Add("Open", typeof(float));
            //[High] [float] NOT NULL,
            table.Columns.Add("High", typeof(float));
            //[Low] [float] NOT NULL,
            table.Columns.Add("Low", typeof(float));
            //[Close] [float] NOT NULL,
            table.Columns.Add("Close", typeof(float));
            //[Volume] [float] NOT NULL,
            table.Columns.Add("Volume", typeof(float));
        }

        protected override void AddToTable(object o)
        {
            var r = o as Ohlc;
            r.Id = Com.BitsQuan.Option.Imp.IdService<Ohlc>.Instance.NewId();
            DataRow dr = table.NewRow();
            dr[0] = r.Id;
            dr[1] = r.WhatId;
            dr[2] = r.OhlcType;
            dr[3] = r.When;
            dr[4] = r.WhenInDt;//.SlaveOrderId;
            dr[5] = r.Open;//.When;
            dr[6] = r.High;//.IsPartialDeal;
            dr[7] = r.Low;//.Count;
            dr[8] = r.Close;//.DealType;
            dr[9] = r.Volume;//
            table.Rows.Add(dr);
        }

        protected override bool ShouldSave(object o)
        {
            return o is Ohlc;
        }
    }
}
