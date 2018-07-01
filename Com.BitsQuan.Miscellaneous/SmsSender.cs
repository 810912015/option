using Com.BitsQuan.Option.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Com.BitsQuan.Miscellaneous
{
    public interface ISmsSender
    {
        void Send(string mobileNumber, string content);
    }
    public class SmsSender
    {
        static readonly TextLog log = new TextLog("sms.txt");
        //短信接口用户名：dlbqwl00   密码：aIA51yaZ
        string url;
        string name;
        string pwd;
        public SmsSender(string url = "http://cf.lmobile.cn/submitdata/service.asmx", string name = "dlbqwl00", string pwd = "aIA51yaZ")
        {
            this.url = url; this.name = name; this.pwd = pwd;
        }
        public void Send(List<string> mobiles, string content, string corpid = "1", string proId = "1012818")
        {
            try
            {
                if (mobiles == null || mobiles.Count == 0 || string.IsNullOrEmpty(content)) return;

                //sname:提交用户
                //spwd:提交密码
                //scorpid:企业代码
                //sprdid:产品编号
                //sdst:接收号码，多个以','分割,不可超过100000个号码
                //smsg:短信内容
                ///submitdata/service.asmx/g_Submit?sname=string&spwd=string&scorpid=string&sprdid=string&sdst=string&smsg=string
                ///
                StringBuilder sb = new StringBuilder();
                sb.Append(url);
                sb.AppendFormat("/g_Submit?sname={0}&spwd={1}&scorpid={2}&sprdid={3}", name, pwd, corpid, proId);
                
                StringBuilder sb2 = new StringBuilder();
                foreach (var v in mobiles)
                {
                    sb2.AppendFormat("{0},", v);
                }
                if (sb2.Length > 0) sb2.Remove(sb2.Length - 1, 1);
                sb.AppendFormat("&sdst={0}&smsg={1}", sb2.ToString(), content);

                var Url = sb.ToString();

               // CookieContainer cc = new CookieContainer();
                System.Net.HttpWebRequest wReq = (HttpWebRequest)System.Net.WebRequest.Create(Url);
                //wReq.CookieContainer = cc;

                // Get the response instance.
                System.Net.WebResponse wResp = wReq.GetResponse();
                System.IO.Stream respStream = wResp.GetResponseStream();
                string r = "";
                using (System.IO.StreamReader reader = new System.IO.StreamReader(respStream, Encoding.UTF8))
                {
                    r = reader.ReadToEnd();
                }
                log.Info(string.Format("{0}-{1}", sb.ToString(), r));
            }
            catch (Exception e)
            {
                log.Error(e);
            }
        }

    }


    public class SmsSender1
    {
        public void Test()
        {
            SmsService.Service1SoapClient ssc = new SmsService.Service1SoapClient();
            var r = ssc.g_Submit("dlbqwl00", "aIA51yaZ", "1", "1012818", "15921462689", "hello");
        }
    }
}
