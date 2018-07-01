using System;
using System.Net;
using System.Text;

namespace Com.BitsQuan.Miscellaneous
{
    public class HttpExecutor
    {
        public static string Get(string Url)
        {
            try
            {
                CookieContainer cc = new CookieContainer();
                System.Net.HttpWebRequest wReq = (HttpWebRequest)System.Net.WebRequest.Create(Url);
                wReq.Method = "GET";
                wReq.CookieContainer = cc;
                System.Net.WebResponse wResp = wReq.GetResponse();
                System.IO.Stream respStream = wResp.GetResponseStream();
                string r = "";
                using (System.IO.StreamReader reader = new System.IO.StreamReader(respStream, Encoding.UTF8))
                {
                    r = reader.ReadToEnd();
                }
                return r;
            }
            catch (Exception e) { throw e; }
        }
    }
}
