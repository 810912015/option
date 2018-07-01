using Com.BitsQuan.Option.Core;
using System;
using System.ComponentModel;
using System.Configuration;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;

namespace Com.BitsQuan.Miscellaneous
{
    internal class SmsSenderAuthInfo
    {
        public string SmsUserName { get; set; }
        public string SmsPassword { get; set; }
        /// <summary>
        /// 企业代码
        /// </summary>
        public string CorpId { get; set; }
        /// <summary>
        /// 产品编号
        /// </summary>
        public string PrdId { get; set; }
        private SmsSenderAuthInfo() { }
        public static readonly SmsSenderAuthInfo Instance = new SmsSenderAuthInfo
        {
            SmsUserName = AppSettings.Read<string>("SmsUserName"),
            SmsPassword = AppSettings.Read<string>("SmsPassword"),
            CorpId = AppSettings.Read<string>("CorpId"),
            PrdId = AppSettings.Read<string>("PrdId"),
        };
    }
    public class QQExMailSender : Com.BitsQuan.Miscellaneous.IMailSender, Com.BitsQuan.Miscellaneous.IMsgSender
    {
        static SmsSenderAuthInfo SmsAuth = SmsSenderAuthInfo.Instance;

        public WebReference.Service1 service = new WebReference.Service1();
        Object sync = new Object();

        static readonly TextLog log = new TextLog("email.txt");
        public static string FromAddr = Com.BitsQuan.Miscellaneous.AppSettings.Read<string>("serviceMail", "service@bitsquan.com");
        public static string Pwd = Com.BitsQuan.Miscellaneous.AppSettings.Read<string>("serviceMailPwd", "bitsquan2015");
        public static string Smtp = Com.BitsQuan.Miscellaneous.AppSettings.Read<string>("smtpAddr", "smtp.exmail.qq.com");
        static SmtpClient sc;
        public QQExMailSender(string from = "service@bitsquan.com", string pwd = "bitsquan2015")
        {
            InitClient();
        }
        public static void InitClient()
        {
            if (sc != null) sc.Dispose();
            sc = new SmtpClient();
            NetworkCredential nc = new NetworkCredential();
            nc.UserName = FromAddr;
            nc.Password = Pwd;
            sc.UseDefaultCredentials = true;
            sc.EnableSsl = true;
            sc.DeliveryMethod = SmtpDeliveryMethod.Network;
            sc.Credentials = nc;
           
            sc.Host = Smtp;
        }

        void sc_SendCompleted(object sender, AsyncCompletedEventArgs e)
        {
            //throw new NotImplementedException();
        }


        public Task SendToWait(string to, string subject, string content)
        {

            var t = Task.Factory.StartNew(() =>
            {
                try
                {
                    MailMessage mm = new MailMessage();
                    MailAddress Fromma = new MailAddress(FromAddr);
                    MailAddress Toma = new MailAddress(to, null);
                    mm.From = Fromma;
                    //收件人
                    mm.To.Add(to);
                    //邮箱标题
                    mm.Subject = subject;
                    mm.IsBodyHtml = true;
                    //邮件内容
                    mm.Body = content;
                    //内容的编码格式
                    mm.BodyEncoding = System.Text.Encoding.UTF8;
                    mm.DeliveryNotificationOptions = DeliveryNotificationOptions.OnSuccess;
                    mm.CC.Add(Toma);
                    sc.Send(mm);
                    var truncate = content.Length > 100 ? content.Substring(0, 100) : content;
                    log.Info(string.Format("{0}-{1}-{2}", to, subject, truncate));
                }
                catch (Exception e)
                {
                    log.Error(e, string.Format("{0}-{1}-{2}", to, subject, content.Length>100?content.Substring(0,100):content));
                }
            });
            return t;

        }

        public void SendTo(string to, string subject, string content)
        {
            SendToWait(to, subject, content).Wait();
        }

        //发送短信
        public int SendMassage(string phone)
        {
            Random yzm = new Random();
            int yanzheng = yzm.Next(1000, 9999);

            string canshu = phone;//手机号
            string sdst = canshu;
            string smsg = string.Format("您好，您获得的验证码为：{0}，3分钟内有效【比权网】", yanzheng);

            WebReference.CSubmitState state = service.g_Submit(SmsAuth.SmsUserName, SmsAuth.SmsPassword, SmsAuth.CorpId, SmsAuth.PrdId, sdst, smsg);


            log.Info(string.Format("短信:号{0}-文{1}-结果-{2}-{3}-{4}-{5}", phone, smsg, state.MsgID, state.MsgState, state.State, state.Reserve));
            return yanzheng;
        }

        public void SendMassage2(string phone, string str)
        {
            lock (sync)
            {
                string canshu = phone;//手机号
                string sdst = canshu;
                string smsg = string.Format(str);

                WebReference.CSubmitState state = service.g_Submit(SmsAuth.SmsUserName, SmsAuth.SmsPassword, SmsAuth.CorpId, SmsAuth.PrdId, sdst, smsg);
                log.Info(string.Format("短信:号{0}-文{1}-结果-{2}-{3}-{4}-{5}", phone, smsg, state.MsgID, state.MsgState, state.State, state.Reserve));
            }
        }

    }
}
