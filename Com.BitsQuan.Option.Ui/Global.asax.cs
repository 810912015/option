using Com.BitsQuan.Boss.Core;
using Com.BitsQuan.Miscellaneous;
using Com.BitsQuan.Option.Core;
using Com.BitsQuan.Option.Imp;
using Com.BitsQuan.Option.Match;
using Com.BitsQuan.Option.Match.Spot;
using Com.BitsQuan.Option.Ui.Areas.Supervise.Data;
using Com.BitsQuan.Option.Ui.Hubs;
using Com.BitsQuan.Option.Ui.Models;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using Com.BitsQuan.Option.Match.Imp;
using System.Text;

namespace Com.BitsQuan.Option.Ui
{
    /// <summary>
    /// 交易系统监控
    /// 监控内容:
    ///     1.钱监控:每用户的保证金,现金总额之和,加上系统账户的私有总额,公共总额应该恒定;
    ///     2.仓监控:每合约的义务仓和权利仓数量应相等
    /// 运行方式:
    ///     1.每1分钟执行一次,计算结果写日志;
    ///     2.异常发生后通过设置的邮箱发邮件,电话发短信;邮箱和电话多个用逗号隔开;
    ///     3.可通过将邮件和电话设为空使不通知;
    /// </summary>
    public class SvcMonitor
    {
        System.Timers.Timer t;
        TextLog log;
        decimal InitTotal;
        QQExMailSender msender;
        StringBuilder thisLog;
        DateTime? lastSendTime;
        public SvcMonitor()
        {
            lastSendTime = null;
            thisLog = new StringBuilder();
            InitTotal = CalTotal();
            log = new TextLog("svcMonitor.txt");
            msender = new QQExMailSender();
            t = new System.Timers.Timer();
            t.Elapsed += t_Elapsed;
            t.Interval = 60 * 1000;
            t.Start();
        }

        void t_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            try
            {
                t.Stop();

                Execute();
            }
            catch (Exception ex)
            {
                log.Error(ex);
            }
            finally
            {
                log.Info(thisLog.ToString());
                t.Start();
            }
        }
        decimal CalTotal()
        {
            decimal total = 0m;
            thisLog.AppendFormat("系统私有总额{0}-公共总额{1}", SystemAccount.Instance.PrivateSum, SystemAccount.Instance.PublicSum);
            total += SystemAccount.Instance.PublicSum;
            total += SystemAccount.Instance.PrivateSum;
            var svc = MvcApplication.OptionService;
            foreach (var v in svc.Model.Traders)
            {
                total += v.Account.BailAccount.Total;
                total += v.Account.CacheAccount.CnyAccount.Total;
                thisLog.AppendFormat("-{0}-保证金{1}-现金{2}", v.Name, v.Account.BailAccount.Total, v.Account.CacheAccount.CnyAccount.Total);
            }
            thisLog.AppendFormat("-新总额{0}-原总额{1}", total,InitTotal);
            return total;
        }
        List<Tuple<string, int, int>> GetDiffForPos()
        {
            Dictionary<string, List<int>> dic = new Dictionary<string, List<int>>();
              var svc = MvcApplication.OptionService;
              foreach (Trader v in svc.Model.Traders)
              {
                  var ps = v.GetPositionSummaries();
                  foreach (var p in ps)
                  {
                      if (p == null||p.Count <=0) continue;
                      if (!dic.ContainsKey(p.CCode)) dic.Add(p.CCode, new List<int>{0,0});
                      var d = dic[p.CCode];
                      if (p.PositionType == "义务仓")
                      {
                          d[0] += p.Count;
                          
                      }
                      else
                      {
                          d[1] += p.Count;
                      }
                      thisLog.AppendFormat("-{0}{1}{2}{3}个", v.Name, p.CCode, p.PositionType, p.Count);
                  }
              }
              List<Tuple<string, int, int>> l = new List<Tuple<string, int, int>>();
              foreach (var v in dic)
              {
                  thisLog.AppendFormat("-{0}义务仓{1}个权力仓{2}个", v.Key, v.Value[0], v.Value[1]);
                  var delta = Math.Abs(v.Value[0] - v.Value[1]);
                  if (delta>10)
                      l.Add(Tuple.Create<string, int, int>(v.Key, v.Value[0], v.Value[1]));
              }
              return l;
        }
        void Execute()
        {
            thisLog.Clear();
            if (MvcApplication.OptionService == null)
            {
                SendToAdmin("交易对象为空"); return;
            }
            if (MvcApplication.OptionService.IsStopped)
            {
                SendToAdmin("交易系统已停止"); return;
            }
            var tp = CalTotal();
            var delta =Math.Abs(tp - InitTotal);
            if (delta > 100)
            {
                SendToAdmin(string.Format("金额错误:初始{0}-现在{1}-差{2}", InitTotal, tp, delta));
                //发送消息后设置比较基准价为新值,以防重复发送
                InitTotal = tp;
            }
            var posdiff = GetDiffForPos();
            if (posdiff.Count > 0)
            {
                StringBuilder  sb=new StringBuilder ();
                sb.Append("仓位错误:");
                foreach (var v in posdiff)
                {

                    sb.AppendFormat("-{0}-义务仓{1}-权力仓{2}", v.Item1, v.Item2, v.Item3);
                }
                SendToAdmin(sb.ToString());
            }
            
            
        }
        string[] GetAdminPhone()
        {
            var phone = Com.BitsQuan.Miscellaneous.AppSettings.Read<string>("adminPhone", "15921462689");
            thisLog.AppendFormat("-管理员电话:{0}", phone);
            return phone.Split(',');
        }
        string[] GetAdminEmail()
        {
            var phone = Com.BitsQuan.Miscellaneous.AppSettings.Read<string>("adminEmail", "810912015@qq.com");
            thisLog.AppendFormat("-管理员邮箱:{0}", phone);
            return phone.Split(',');
        }
        bool ShouldSend
        {
            get
            {
                if (lastSendTime == null) return true;
                var lt = (DateTime)lastSendTime;
                return DateTime.Now.Subtract(lt).TotalHours > 2;
            }
        }
        void SendToAdmin(string s1)
        {
            var appName = Com.BitsQuan.Miscellaneous.AppSettings.Read<string>("webSite", "");
            var s = string.Format("尊敬的{0}，您好！{1}-{2}【比权网】",DateTime.Now.ToString("yyMMddHHmmss"),s1,appName);
            
            thisLog.AppendFormat("消息:{0},发送:{1}",s,ShouldSend);
            if (!ShouldSend) return;
            lastSendTime = DateTime.Now;
            var ap = GetAdminPhone();
            foreach (var v in ap)
            {
                if (string.IsNullOrEmpty(v)) continue;
                msender.SendMassage2(v, s);
            }
            var ae = GetAdminEmail();
            foreach (var v in ae)
            {
                if (string.IsNullOrEmpty(v)) continue;
                msender.SendTo(v, "系统异常", s);
            }
            
        }
        
    }

    public class MvcApplication : System.Web.HttpApplication
    {
        /// <summary>
        /// 期权交易主程序
        /// </summary>
        public static MatchService OptionService { get; set; }
        static SvcMonitor svcMonitor;
        /// <summary>
        /// 现货交易主程序
        /// </summary>
        public static SpotService SpotService { get; private set; }
        /// <summary>
        /// 上限机器人
        /// </summary>
        static PriceRaiseRobot raiseRobot;
      
        static BlasterSendMsg sm;
        public static InvitorFeeMgr ifm { get; private set; }
        static MvcApplication()
        {
            try
            {
                Singleton<TextLog>.Instance.Info("开始启动期权交易系统");
                OptionService = new MatchService();
                OptionService.Start();

                Singleton<TextLog>.Instance.Info("开始启动虚拟币交易系统");
                SpotService = new Match.Spot.SpotService(OptionService.Model);



                Com.BitsQuan.Option.Ui.Hubs.TradeHub.Init();
                Com.BitsQuan.Option.Ui.Hubs.SpotHub.Init();
                Com.BitsQuan.Option.Core.BtcPrice.Init();

                if (HasRobot())
                {
                    raiseRobot = new PriceRaiseRobot(-1, OptionService.AddOrder, MatchParams.RaiseMax);
                    OptionService.OnAfterMatch += OnAfterMatch;
                }
                sm = new BlasterSendMsg();
                //保证金报警信息提示
                OptionService.Monitor.dutyBlaster.OnBlasting += sm.Notify;
                OptionService.Monitor.rightBlaster.OnBlasting += sm.Notify;
                OptionService.Monitor.OnBailWarning += sm.Warn;
                svcMonitor = new SvcMonitor();
                ifm = new InvitorFeeMgr();
            }
            catch (Exception ex)
            {
                Singleton<TextLog>.Instance.Error(ex);
            }
        }
        
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            GlobalConfiguration.Configure(WebApiConfig.Register);
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
            MvcHandler.DisableMvcResponseHeader = true;
        }
        static bool HasRobot()
        {
            try
            {
                return System.Configuration.ConfigurationManager.AppSettings["hasRobot"] == "1";
            }
            catch (Exception ex)
            {
                Singleton<TextLog>.Instance.Error(ex, "hasrobot");
                return false;
            }
        }

        static void OnAfterMatch(Core.Order obj)
        {
            raiseRobot.EatOrder(obj);
        }

        protected void Application_PreSendRequestHeaders(object sender, EventArgs e)
        {
            // Remove the "Server" HTTP Header from response
            HttpApplication app = sender as HttpApplication;
            if (null != app && null != app.Request && !app.Request.IsLocal &&
                null != app.Context && null != app.Context.Response)
            {
                System.Collections.Specialized.NameValueCollection headers = app.Context.Response.Headers;
                if (null != headers)
                {
                    headers.Remove("Server");
                }
            }
        }
    }

    /// <summary>
    /// 登录失败记录
    /// </summary>
    public class LoginFailRecord
    {
        public int Times { get; set; }
        public DateTime UnlockTime { get; set; }
    }

    public class LoginFailManager
    {
        /// <summary>
        /// 锁定时长
        /// </summary>
        public static TimeSpan LockTimeSpan = TimeSpan.FromMinutes(AppSettings.Read<int>("DefaultAccountLockoutTimeSpan"));
        /// <summary>
        /// 最多尝试次数
        /// </summary>
        public static int MaxFailedAccessAttempts = AppSettings.Read<int>("MaxFailedAccessAttemptsBeforeLockout");
        private static readonly Dictionary<string, LoginFailRecord> _dic = new Dictionary<string, LoginFailRecord>();

        /// <summary>
        /// 登录成功后调用此方法重置记录
        /// </summary>
        /// <param name="userId"></param>
        public static void Reset(string userId)
        {
            _dic.Remove(userId);
        }

        /// <summary>
        /// 登录失败时调用此方法以增加失败计数
        /// </summary>
        /// <param name="userId"></param>
        public static void Fail(string userId)
        {
            lock (_dic)
            {
                if (!_dic.ContainsKey(userId))
                    _dic[userId] = new LoginFailRecord();
                ++_dic[userId].Times;
                if (_dic[userId].Times == MaxFailedAccessAttempts)
                {
                    _dic[userId].UnlockTime = DateTime.Now.Add(LockTimeSpan);
                    _dic[userId].Times = 0;
                }
            }
        }

        /// <summary>
        /// 判断帐号是不是处于锁定状态
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public static bool IsLocked(string userId)
        {
            lock (_dic)
            {
                if (!_dic.ContainsKey(userId))
                    return false;
                else
                    return _dic[userId].UnlockTime > DateTime.Now;
            }
        }

        public static LoginFailRecord Get(string userId)
        {
            return _dic[userId];
        }
    }

    public class BlasterSendMsg
    {
        Dictionary<string, string> tips;
        IMailSender mailSender;
        IMsgSender msgSender;
        public bool IsSendToTestUsers { get; set; }
        public int MaxTestUserId { get; set; }
        void Init(IMailSender mailSender, IMsgSender msgSender, bool isSendToTestUsers = false)
        {
            try
            {
                this.IsSendToTestUsers = ConfigurationManager.AppSettings["IsSendToTestUsers"].ToLower() == "true";
                MaxTestUserId = Convert.ToInt32(ConfigurationManager.AppSettings["MaxTestUserId"]);
            }
            catch (Exception ex)
            {
                Singleton<TextLog>.Instance.Error(ex);
            }
            this.mailSender = mailSender; this.msgSender = msgSender;
            tips = new Dictionary<string, string>();
            tips.Add("txt-爆仓警告", "尊敬的{0}，您好！您的保证金账户保证率已小于1.1，为防止账户发生爆仓，请您及时充值！【比权网】");
            tips.Add("txt-爆仓通知", "尊敬的{0}，您好！您的保证金账户已发生爆仓，请您及时关注！【比权网】");
            tips.Add("mail-爆仓警告", "尊敬的用户{0}，您好！您的保证金账户保证率已小于1.1，为防止账户发生爆仓，请您及时充值！【比权网】");
            tips.Add("mail-爆仓通知", "尊敬的用户{0}，您好！您的保证金账户已发生爆仓，请您及时关注！【比权网】");
        }

        public BlasterSendMsg(bool isSendToTestUsers = false)
        {
            QQExMailSender s = new QQExMailSender();
            Init(s, s, isSendToTestUsers);
        }
        public BlasterSendMsg(IMailSender mailSender, IMsgSender msgSender, bool isSendToTestUsers = false)
        {
            Init(mailSender, msgSender, isSendToTestUsers);
        }

        void Send(Trader trader, string msgType)
        {
            var t = Task.Factory.StartNew(() =>
            {
                try
                {
                    if (!IsSendToTestUsers)
                    {
                        if (trader.Id < MaxTestUserId)
                        {
                            return;
                        }
                    }

                    if (trader == null || trader.Name == null)
                    {
                        Singleton<TextLog>.Instance.Info("爆仓应发送短信和邮件,但交易员或其姓名为空");
                        return;
                    }
                    ApplicationUser user;
                    using (ApplicationDbContext adb = new ApplicationDbContext())
                    {
                        user = adb.Users.Where(a => a.UserName == trader.Name).FirstOrDefault();
                    }
                    if (user == null)
                    {
                        Singleton<TextLog>.Instance.Info("爆仓应发送短信和邮件,但无法找到手机号:" + trader.Name);
                        return;
                    }
                    var txtFormat = tips["txt-" + msgType];
                    var txt = string.Format(txtFormat, trader.Name);
                    msgSender.SendMassage2(user.PhoneNumber.ToString(), txt);

                    var mailFormat = tips["mail-" + msgType];
                    var mail = string.Format(mailFormat, trader.Name);
                    mailSender.SendTo(user.Email, msgType + DateTime.Now.ToString(), mail);
                }
                catch (Exception e)
                {
                    Singleton<TextLog>.Instance.Error(e, "send msg " + msgType, trader.Name);
                }
            });
            t.Wait();
        }
        public void Warn(Trader trader, decimal ratio)
        {
            Send(trader, "爆仓警告");
        }
        public void Notify(BlastRecord br)
        {
            Send(br.Trader, "爆仓通知");
        }
    }
}
