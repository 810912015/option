using Com.BitsQuan.Option.Core;
using System;
using System.Collections.Generic;
using System.Data;

namespace Com.BitsQuan.Option.Match.Imp
{
    public class OrderSaver : OrderSaverBase
    {
        public OrderSaver()
            : base(System.Configuration.ConfigurationManager.ConnectionStrings["OptionDb"].ToString(), "Orders", 1)
        {
            log = new TextLog("orders.txt");
        }
        TextLog log;

        public override void Save(object o)
        {
            var oo = o as Order;
            log.Info(oo.ToShortString());
            base.Save(o);
        }
        protected override bool ShouldSave(object o)
        {
            var t = o as Order;
            if (t == null) return false;
            //if (t.Detail != null && t.Detail == savedTag) return false;
            //t.Detail = savedTag;
            return true;
        }
        //string savedTag = "saved";
        protected override void AddToTable(object o)
        { 
            base.AddToTable(o);
        }
    }
}
