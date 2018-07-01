using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Com.BitsQuan.Option.Match.Imp;
using System.Net;
using System.Text;
using Newtonsoft.Json.Linq;
using System.Linq;

namespace Com.BitsQuan.Option.Test
{
    [TestClass]
    public class OhlcTest
    {
        [TestMethod]
        public void get_date_for_now()
        {
            OhlcMaker om = new OhlcMaker(1, Core.OhlcType.M60);
            var fdt = DateTime.Now.Date.AddHours(8).AddMinutes(43);
            var r = om.GetPreBoundForTime(Core.OhlcType.M15, fdt);
            Assert.AreEqual(30, r.Minute);
        }

        [TestMethod]
        public void btc_price_get()
        { 
            var Url = "https://market.huobi.com/staticmarket/detail_btc.js";
            System.Net.WebRequest wReq = System.Net.WebRequest.Create(Url);
            // Get the response instance.
            System.Net.WebResponse wResp = wReq.GetResponse();
            System.IO.Stream respStream = wResp.GetResponseStream();
            string r = "";
            using (System.IO.StreamReader reader = new System.IO.StreamReader(respStream, Encoding.UTF8))
            {
                r= reader.ReadToEnd();
            }
            var start = r.IndexOf('(');
            var end = r.IndexOf(')');
            var str = r.Substring(start+1, end - start-1);
            JObject j = JObject.Parse(str);
            var n = (decimal)j.Property("p_new").Value;

            Assert.IsTrue(n > 0);
        }
    }
}
