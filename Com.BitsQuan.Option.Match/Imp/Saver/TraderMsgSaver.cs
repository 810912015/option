using Com.BitsQuan.Option.Core;
using System;
using System.Data;

namespace Com.BitsQuan.Option.Match.Imp
{ 
    public class TraderMsgSaver : BulkSaver
    {
        public TraderMsgSaver() : 
            base(System.Configuration.ConfigurationManager.ConnectionStrings["OptionDb"].ToString(),
            "TraderMsgs",41) { }

        protected override void CreateTable()
        {
            table = new DataTable(); 
           
    //        [Id] [int] NOT NULL,
            table.Columns.Add("Id", typeof(int));
    //[Name] [nvarchar](max) NULL,
            table.Columns.Add("Name", typeof(string));
    //[Msg] [nvarchar](max) NULL,
            table.Columns.Add("Msg", typeof(string));
            //[When] [datetime] NOT NULL,
            table.Columns.Add("When", typeof(DateTime));
            //[MsgType] [nvarchar](max) NULL,
            table.Columns.Add("MsgType", typeof(string));
        }

        protected override void AddToTable(object o)
        {
            var r = o as TraderMsg;
            DataRow dr = table.NewRow();
            dr[0] = r.Id; 
            dr[1] = r.Name; 
            dr[2] = r.Msg; 
            dr[3] = r.When;
            dr[4] = r.MsgType;
            table.Rows.Add(dr);
        }

        protected override bool ShouldSave(object o)
        {
            return o is TraderMsg;
        }
    }
}
