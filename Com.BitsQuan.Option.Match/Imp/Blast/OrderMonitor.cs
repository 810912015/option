using Com.BitsQuan.Option.Core;
using Com.BitsQuan.Option.Match.Imp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Timers;

namespace Com.BitsQuan.Option.Match
{
    /// <summary>
    /// 交易监视类
    ///     目的:根据用户的保证率确定对应的操作
    ///          小于1.1,短信和邮件通知
    ///          小于1时操作依次为:撤单,自动转入保证金,义务仓爆仓,权利仓爆仓
    ///     触发条件:
    ///          1.持仓变化,下单,保证金金额变化引起的保证率变化
    ///          2.期权当前价格变化引起的保证率变化;
    ///          3.定时检查:应对比特币价格变化引起的保证率变化
    ///     保证率一旦小于1则持续监视操作,直至保证率达到要求
    /// </summary>
    public class OrderMonitor : IDisposable
    { 
        /// <summary>
        /// 要监视的交易账户
        /// </summary>
        IEnumerable<Trader> traders; 
        /// <summary>
        /// 爆仓前准备器
        /// </summary>
        public BlasterPrePare prepare { get; private set; }
        /// <summary>
        /// 义务仓爆仓器
        /// </summary>
        public DutyBlaster dutyBlaster { get; private set; }
        /// <summary>
        /// 权利仓爆仓器
        /// </summary>
        public RightBlaster rightBlaster { get; private set; }
        /// <summary>
        /// 市场行情
        /// </summary>
        Market m; 
        /// <summary>
        /// 监视器:一旦开始则持续到达到要求
        /// </summary>
        Watcher watcher;
        /// <summary>
        /// 定时器:5秒钟执行一次,应对比特币变化引起的保证率变化
        /// </summary>
        System.Timers.Timer monitorTimer;
        static TextLog log = new TextLog("traderMonitor.txt");
        
        public OrderMonitor(IMatch matcher, Market m, IOptionModel model)
        {
            this.traders = model.Traders;
            this.m = m;
            prepare = new BlasterPrePare(matcher, m);
            dutyBlaster = new DutyBlaster(matcher, m,model);
            rightBlaster = new RightBlaster(matcher, m,model);
           
            watcher = new Watcher(Monitor,Clear);
            monitorTimer = new System.Timers.Timer();
            monitorTimer.Interval = 5 * 1000;
            monitorTimer.Elapsed += monitorTimer_Elapsed;
            
        }

        public void Start()
        {
            foreach (var v in traders)
            {
                v.OnRatioChanged += v_OnRatioChanged;
            }
            foreach (var v in m.Board.Values)
            {
                v.OnDealPriceChanged += v_OnDealPriceChanged;
            }
            monitorTimer.Start();
        }
        List<Trader> GetTraderExceptRobot()
        {
            return traders.Where(a => a.Id >= 1).ToList();
        }
        void monitorTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            try
            {
                monitorTimer.Stop();
                var tl = GetTraderExceptRobot();

                foreach (var v in tl)
                {
                    var ratio = v.GetMaintainRatio();
                    var shouldMonitor = ratio < SysPrm.Instance.MonitorParams.AlarmMaintainRatio;
                    var isMonitoring = v.IsMonitoring();

                    if(shouldMonitor)
                    {
                        if (!isMonitoring)
                        {
                            log.Info(string.Format("开始监视:人{0}-率{1}-爆{2}-线程{3}",
                                v.Name, ratio, v.IsBlasting(),
                                 Thread.CurrentThread.ManagedThreadId));
                            Monitor(v);
                        }
                        
                    }
                    else
                        {
                            if (isMonitoring)
                            {
                                //log.Info(string.Format("",v.Name,ratio))
                                log.Info(string.Format("结束监视:人{0}-率{1}-爆{2}-线程{3}",
                                    //"爆仓检查定时器:人{0}-监视信号{1}-爆仓信号{2}-线程{3}",
                                       v.Name, ratio, v.IsBlasting(),
                                        Thread.CurrentThread.ManagedThreadId));
                                Clear(v);
                            }
                        }
                }
            }
            catch (Exception ex)
            {
                log.Error(ex);
            }
            finally
            {
                monitorTimer.Start();
            }
        }

        void v_OnDealPriceChanged(Contract arg1, decimal arg2)
        {
            var tl = GetTraderExceptRobot();
            foreach (var v in tl)
            {
                if (v.HasPosition(arg1)&& !v.IsMonitoring())
                {
                    log.Info(string.Format("成交价格爆仓:人{0}-监视信号{1}-爆仓信号{2}-成交价格{3}-合约{4}-线程{5}", v.Name, 
                        v.IsMonitoring(), v.IsBlasting(),arg2,arg1.Code, Thread.CurrentThread.ManagedThreadId));
                    Monitor(v);
                }                
            }
        }

        void v_OnRatioChanged(Trader arg1, decimal arg2)
        {

            if (arg1.IsMonitoring() && arg2 >= SysPrm.Instance.MonitorParams.BlastMaintainRatio)
            {
                Clear(arg1); 
            }
            else
                if (arg2 < SysPrm.Instance.MonitorParams .NormalMaintainRatio&& !arg1.IsMonitoring())
            {
                log.Info(string.Format("保证率变化爆仓:人{0}-监视信号{1}-爆仓信号{2}-保证率{3}-线程{4}", arg1.Name, arg1.IsMonitoring(), arg1.IsBlasting(),arg2,
                     Thread.CurrentThread.ManagedThreadId));
                Monitor(arg1);
            }
            

        }
        /// <summary>
        /// 监视结束的清理:撤掉因爆仓引起的挂单
        /// </summary>
        /// <param name="t"></param>
        void Clear(Trader t)
        {
            lock (t.Name)
            {
                //log.Info(string.Format("清理监视{0}-线程{1}", t.Name, Thread.CurrentThread.ManagedThreadId));
                if (t.IsClearing()) return;
                t.ClearAllSelling();
                t.SetClearing(true);
                t.SetMonitoring(false);
                rightBlaster.Clear(t);
                dutyBlaster.Clear(t);
                t.SetClearing(false);
            }
        }


        /// <summary>
        /// 保证金报警事件:当保证率低于1.1时引发
        ///     应当向用户发送邮件和短信提醒
        /// </summary>
        public event Action<Trader, decimal> OnBailWarning;


        void RaiseChanged(Trader trader, decimal ratio)
        {
            foreach (var v in OnBailWarning.GetInvocationList())
            {
                try
                {
                    ((Action<Trader, decimal>)v).BeginInvoke(trader, ratio, null, null); 
                }
                catch (Exception e)
                {
                    Singleton<TextLog>.Instance.Error(e, "OnBailWarning");
                }
            }
        }

        /// <summary>
        /// 监视程序:警示,准备,义务仓爆仓,权利仓爆仓  log.Info(string.Format("保证率:{0},////{1}", ratio,SysPrm.Instance.MonitorParams.AlarmMaintainRatio));
        /// </summary>
        /// <param name="t"></param>
        void Monitor(Trader t)
        {
            lock (t.Name)
            {
                //log.Info(string.Format("监视{0}-线程{1}", t.Name, Thread.CurrentThread.ManagedThreadId));
                if (t.IsBlasting()) return;
                t.SetBlasting(true);
                var ratio = t.GetMaintainRatio(m);
                //<1.1
                if (ratio < SysPrm.Instance.MonitorParams.AlarmMaintainRatio)
                {
                    if (t.ShouldInstantMsg())
                    {
                        RaiseChanged(t, ratio);
                    }
                    t.SetMonitoring(true);//设置监视
                    watcher.Watch(t);
                }
                if (t.GetMaintainRatio(m) < SysPrm.Instance.MonitorParams.BlastMaintainRatio)//<1.0
                {
                    if (prepare.Blast(t))
                    {
                        t.SetBlasting(false);
                        return;
                    }
                   
                    if (dutyBlaster.Blast(t))
                    {
                        t.SetBlasting(false);
                        return;
                    }
                    if (rightBlaster.Blast(t))
                    {
                        t.SetBlasting(false);
                        return;
                    }
                }           
                t.SetBlasting(false);
            }
        }
        

        public void Dispose()
        {
             
        }
    }
}
