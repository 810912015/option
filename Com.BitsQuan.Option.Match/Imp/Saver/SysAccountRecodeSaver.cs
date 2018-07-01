using Com.BitsQuan.Option.Core;
using System;
using System.Data;

namespace Com.BitsQuan.Option.Match.Imp
{
    public class SysAccountRecodeSaver : BulkSaver
    {
        public SysAccountRecodeSaver() : base(System.Configuration.ConfigurationManager.ConnectionStrings["OptionDb"].ToString(), "SysAccountRecords",37) { }

        protected override void CreateTable()
        {
            table = new DataTable();

            //            [Id] [int] IDENTITY(1,1) NOT NULL,
            table.Columns.Add("Id", typeof(int));
            //[When] [datetime] NOT NULL,
            table.Columns.Add("When", typeof(DateTime));
            //[Delta] [decimal](18, 2) NOT NULL,
            table.Columns.Add("Delta", typeof(decimal));
            //[Sum] [decimal](18, 2) NOT NULL,
            
            //[IsBorrow] [bit] NOT NULL,
            table.Columns.Add("ChangedType", typeof(int));
            table.Columns.Add("TraderSum", typeof(decimal));
            
            table.Columns.Add("PrivateSum", typeof(decimal));
            table.Columns.Add("PublicSum", typeof(decimal));
            //[Order_Id] [int] NULL,
            table.Columns.Add("Order_Id", typeof(int));
            //[Who_Id] [int] NULL,
            table.Columns.Add("Who_Id", typeof(int));
        }

        protected override void AddToTable(object o)
        {
            var r = o as SysAccountRecord;
            DataRow dr = table.NewRow();
            dr[0] = r.Id;
            dr[1] = r.When;
            dr[2] = r.Delta;
            
            
            dr[3] = r.ChangedType;
            dr[4] = r.TraderSum;
            dr[5] = r.PrivateSum;
            dr[6] = r.PublicSum;
            if (r.Order != null)
                dr[7] = r.Order.Id;
            dr[8] = r.Who.Id;


            table.Rows.Add(dr);
        }

        protected override bool ShouldSave(object o)
        {
            return o is SysAccountRecord;
        }
    }
}
