using Com.BitsQuan.Option.Core;
using System;
using System.Data;

namespace Com.BitsQuan.Option.Match.Imp
{
    public class BlastOperationSaver : BulkSaver
    {
        public BlastOperationSaver() : base(System.Configuration.ConfigurationManager.ConnectionStrings["OptionDb"].ToString(), "BlasterOperatons",2) { }

        protected override void CreateTable()
        {
            table = new DataTable();
            //[Id] [int] NOT NULL,
            table.Columns.Add("Id", typeof(int));
            //[PositionId] [int] NOT NULL,
            table.Columns.Add("PositionId", typeof(int));
            //[OpOrderId] [int] NOT NULL,
            table.Columns.Add("OpOrderId", typeof(int));
            //[Result] [bit] NOT NULL,
            table.Columns.Add("Result", typeof(bool));
            //[BlasterRecordId] [int] NOT NULL,
            table.Columns.Add("BlasterRecordId", typeof(int));
        }

        protected override void AddToTable(object o)
        {
            DataRow dr = table.NewRow();
            var r = o as BlasterOperaton;
            dr[0] = r.Id;
            dr[1] = r.PositionId;
            dr[2] = r.OpOrderId; 
            dr[3] = r.Result;
            dr[4] = r.BlasterRecordId;
            table.Rows.Add(dr);
        }

        protected override bool ShouldSave(object o)
        {
            return o is BlasterOperaton;
        }
    }
}
