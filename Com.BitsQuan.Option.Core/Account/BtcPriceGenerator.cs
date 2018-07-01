using Newtonsoft.Json.Linq;
using System;
using System.Net;
using System.Text;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading;

namespace Com.BitsQuan.Option.Core
{
    public class BtcPriceGenerator
    {
        static Task<string> Get(string Url)
        {
           return Task.Factory.StartNew<string>(()=>{
                try
                {
                    CookieContainer cc = new CookieContainer();
                    System.Net.HttpWebRequest wReq = (HttpWebRequest)System.Net.WebRequest.Create(Url);
                    wReq.CookieContainer = cc;

                    // Get the response instance.
                    System.Net.WebResponse wResp = wReq.GetResponse();
                    System.IO.Stream respStream = wResp.GetResponseStream();
                    string r = "";
                    using (System.IO.StreamReader reader = new System.IO.StreamReader(respStream, Encoding.UTF8))
                    {
                        r = reader.ReadToEnd();
                    }
                    return r;
                }
                catch (Exception e)
                {
                    throw;
                }
            }); 
        }
        public static async Task<decimal> GetHuobi()
        {
            try
            {
                var Url = "https://market.huobi.com/staticmarket/detail_btc.js";
                var r = await Get(Url);
                var start = r.IndexOf('(');
                var end = r.IndexOf(')');
                var str = r.Substring(start + 1, end - start - 1);
                JObject j = JObject.Parse(str);
                var n = (decimal)j.Property("p_new").Value;
                return n;
            }
            catch(Exception ex)
            {
                log.Error(ex, "huobi");
                return -1;
            }
        }
        public static async Task<decimal> GetOkCoin()
        {
            try
            {
                var url = "https://www.okcoin.cn/real/ticker.do";
                var r =await Get(url);
                JObject j = JObject.Parse(r);
                var n = (decimal)j.Property("btcLast").Value;
                return n;
            }
            catch (Exception e)
            {
                log.Error(e, "okoin");
                return -1;
            }
        }

        public static async Task<decimal> GetBtcChina()
        {
            try
            {
                var url = "https://data.btcchina.com/data/ticker?market=all";
                var r = await Get(url);
                JObject j = JObject.Parse(r); 
                var r2 = j.Property("ticker_btccny").Value.Value<decimal>("vwap");
                return r2;
            }
            catch (Exception e){
                log.Error(e, "btcchina");
                return -1;
            }
        }

        public static async Task<decimal> GetCnBtc()
        {
            try
            {
                var url = "https://trans.chbtc.com/line/topall?jsoncallback=jsonp";
                var r =await Get(url);

                var r1 = r.Split('[');
                var r2 = r1[2].Split(']');
                var r3 = r2[0].Split(',');
                decimal d = -1;
                decimal.TryParse(r3[0], out d);
                return d;
            }
            catch (Exception e)
            {
                log.Error(e, "cnbtc");
                return -1;
            }
        }
        static TextLog log = new TextLog("btcPriceGenerator.txt");
        public static async Task<decimal> Our()
        {
            DateTime dt = DateTime.Now;
            var huobi =await GetHuobi();
            var okcoin = await GetOkCoin();
            var btcchina = await GetBtcChina();

            List<decimal> l = new List<decimal>();
            if (huobi != -1) l.Add(huobi);
            if (okcoin != -1) l.Add(okcoin);
            if (btcchina != -1) l.Add(btcchina);
            if (l.Count == 0) return -1;
            var sum = l.Sum();
            var r=Math.Round(sum / (decimal)l.Count,2);

            log.Info(string.Format("huobi:{0},okcoin:{1},btcchina:{2},avg:{3},开始时间:{4},当前时间{5}", huobi, okcoin, btcchina, r, dt.ToString("HH:mm:ss.ffff"), DateTime.Now.ToString("HH:mm:ss.ffff")));

            return r;
        }
    }
}
