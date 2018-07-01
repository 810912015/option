using Com.BitsQuan.Option.Core;
using System;
using System.Collections.Generic;
using System.Data;

namespace Com.BitsQuan.Option.Match.Imp
{   
    public class TempOrderSaver : OrderSaverBase
    {
       
        LegacyOrders legcy;
        public TempOrderSaver(LegacyOrders legcy ) : base(System.Configuration.ConfigurationManager.ConnectionStrings["OptionDb"].ToString(), "TempOrders",3) {
            
            this.legcy = legcy;
        }
        protected override bool ShouldSave(object o)
        {
            var order = o as Order;
            if (order == null) return false;
            var r = legcy.ShouldSave(order);
            return r;
        }
    }
}
