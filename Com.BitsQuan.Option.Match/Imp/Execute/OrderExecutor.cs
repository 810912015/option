using Com.BitsQuan.Option.Core;
using Com.BitsQuan.Option.Imp;
using Com.BitsQuan.Option.Match.Imp;
using Com.BitsQuan.Option.Provider;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Timers;


namespace Com.BitsQuan.Option.Match
{

    /// <summary>
    /// 行权执行器
    ///    现金交割
    ///        权利仓:现金账户 +(比特币价格-行权价)*份数
    ///        义务仓:保证金账户 -(比特币价格-行权价)*份数
    ///    完毕后同时清除持仓
    /// </summary>

    public class OrderExecutor : IDisposable
    {
        TextLog log = new TextLog("OrderExecutor.txt");
        static CExeRecHandler handler = new CExeRecHandler();
        public event Action<UserPosition> OnExecuted;
        public static decimal? BtcExeBasePrice = null;
        public event Action<int, decimal> OnContractExecuted;
        MatchService OptionService;
        public PosDataSaver pds { get; private set; }
        Action<Trader, PositionSummary> OnRemovedByExe;
        //行权
        void Execute(PositionSummary up, Trader ts, decimal exeBasePrice, bool isManual = false)
        {
            up.Contract.IsNotInUse = true;
            //if (up.Order == null) return;
            if (!isManual)
                if (up.Contract.ExcuteTime.Date != DateTime.Now.Date) return;
            if (up.Contract.OptionType == OptionType.认购期权)//认购期权:跌了不行权
            {
                if (exeBasePrice > up.Contract.ExcutePrice)//涨了
                {
                    var d = exeBasePrice - up.Contract.ExcutePrice;
                    var t = d * up.Count * up.Contract.CoinCount;
                    if (up.PositionType == "权利仓")//权利方
                    {
                        ts.Account.BailAccount.Collect(t);//收钱 
                        handler.SaveRecord(ts, up.Contract, PositionType.权利仓, up.Count, exeBasePrice, true, t);
                        TraderService.OperateAccount(ts, t, AccountChangeType.行权划入, "系统操作", null, ts.Account.BailAccount.Total);
                    }
                    else//义务方
                    {
                        ts.BailPay(t, ts.GetMarket(), null, AccountChangeType.行权划出);
                        handler.SaveRecord(ts, up.Contract, PositionType.义务仓, up.Count, exeBasePrice, false, t);
                    }
                }
                else
                {
                    if (up.PositionType == "权利仓")//权利方
                    {
                        handler.SaveRecord(ts, up.Contract, PositionType.权利仓, up.Count, exeBasePrice, true, 0);
                    }
                    else//义务方
                    {
                        handler.SaveRecord(ts, up.Contract, PositionType.义务仓, up.Count, exeBasePrice, false, 0);
                    }
                }
            }
            else//认沽期权:涨了不行权
            {
                if (exeBasePrice < up.Contract.ExcutePrice)//跌了
                {
                    var d = up.Contract.ExcutePrice - exeBasePrice;
                    var t = d * up.Count * up.Contract.CoinCount;
                    if (up.PositionType == "权利仓")//权利方
                    {
                        ts.Account.BailAccount.Collect(t);//收钱 
                        handler.SaveRecord(ts, up.Contract, PositionType.权利仓, up.Count, exeBasePrice, true, t);
                        TraderService.OperateAccount(ts, t, AccountChangeType.行权划入, "系统操作", null, ts.Account.BailAccount.Total);
                    }
                    else//义务方
                    {
                        ts.BailPay(t, ts.GetMarket(), null, AccountChangeType.行权划出);
                        handler.SaveRecord(ts, up.Contract, PositionType.义务仓, up.Count, exeBasePrice, false, t);
                    }
                }
                else
                {
                    if (up.PositionType == "权利仓")//权利方
                    {
                        handler.SaveRecord(ts, up.Contract, PositionType.权利仓, up.Count, exeBasePrice, true, 0);
                    }
                    else//义务方
                    {
                        handler.SaveRecord(ts, up.Contract, PositionType.义务仓, up.Count, exeBasePrice, false, 0);
                    }
                }
            }
        }



        IEnumerable<Trader> ts;
        Timer exeTimer;
        bool isTimerFirstExe;
        public static int ExecuteTime
        {
            get
            {
                string xingQuanTime = System.Configuration.ConfigurationManager.AppSettings["executeTime"];
                int a = -1;
                int.TryParse(xingQuanTime, out a);
                return a;
            }
        }
        public OrderExecutor(IEnumerable<Trader> traders, MatchService OptionService, Action<Trader, PositionSummary> OnRemovedByExe)
        {
            var a = ExecuteTime;
            if (a > 0 && a < 25)
            {
                this.OnRemovedByExe = OnRemovedByExe;
                pds = new PosDataSaver();
                this.ts = traders;
                this.OptionService = OptionService;
                var cd = DateTime.Now;
                double secs = 0;

                //计算距第一次行权还有多少秒
                if (cd.Hour < a)
                {
                    secs = cd.Date.AddHours(a).Subtract(cd).TotalSeconds;
                }
                else
                {
                    var tomorrow = cd.AddDays(1).Date.AddHours(a);
                    secs = tomorrow.Subtract(cd).TotalSeconds;
                }
                exeTimer = new Timer();
                exeTimer.Interval = secs * 1000;
                //exeTimer.Interval =10000;
                exeTimer.Elapsed += exeTimer_Elapsed;
                isTimerFirstExe = true;
                exeTimer.Start();
            }

        }
        /// <summary>
        /// 手动行权
        /// </summary>
        /// <param name="contractId"></param>
        /// <param name="basePrice"></param>
        public void ManualExcute(int contractId, decimal basePrice, Market m, string name)
        {
           

            log.Info(string.Format("开始手动行权:{0}-{1}", contractId, basePrice));


            List<int> l = new List<int>();
            var oss = OptionService.Model.Traders.Where(a => a.Orders().Count > 0).ToList();
            //先撤单
            foreach (var s in oss)
            {
                var aa = s.Orders().GetLivesByContractId(contractId); 
                foreach (var b in aa)
                {
                    OptionService.Matcher.Redo(b);
                    l.Add(b.Id);
                }
            }
            var traders = ts.ToList();
            //遍历所有交易员
            for (int i = 0; i < traders.Count; i++)
            {
                var upl = traders[i].GetPositionSummaries().Where(a => a.Contract.Id == contractId);
                var has = upl != null && upl.Count() > 0;
                log.Info(string.Format("{0}-{1}-{2}-{3}个-应{4}", traders[i].Name,
                    has ? upl.First().PositionType : "",
                    has ? upl.First().Contract.Id : 0,
                    has ? upl.First().Count : 0, contractId));
                if (upl == null) continue;
                var uppp = upl.ToList();
                //再遍历他的当前合约的持仓汇总
                for (int j = 0; j < upl.Count(); j++)
                {
                    Execute(uppp[j], traders[i], basePrice, true);

                    var q = traders[i].GetPositionSummaries().Where(a => a.Contract.Id == contractId).FirstOrDefault();
                    if (q == null) continue;
                    var v = q;

                    traders[i].RemovePositionSummary(v.CCode);//根据合约代码移除持仓汇总
                    var pd = new PositionSummaryData
                    {
                        OrderType = v.OrderType,
                        Commission = 0,
                        TotalValue = v.TotalValue,
                        Maintain = 0,
                        ClosableCount = traders[i].GetClosableCount(v),
                        FloatProfit = v.FloatProfit,
                        BuyTotal = v.BuyTotal,
                        BuyPrice = v.BuyPrice,
                        Count = v.Count,
                        PositionType = v.PositionType,
                        ContractId = v.Contract.Id,
                        CloseProfit = v.CloseProfit,
                        TraderId = traders[i].Id,
                        When = DateTime.Now
                    };
                    pds.Save(pd);
                    OnRemovedByExe(traders[i], v);


                    var uq = traders[i].Positions.Where(a => a.Order.Contract.Id == contractId);
                    if (uq == null) continue;
                    var uql = uq.ToList();
                    foreach (var u in uql)
                    {
                        traders[i].Positions.Remove(u);
                    }
                }

            }
            m.Board.Remove(name);
            handler.UpdateOrderStateToExecuted(l);
            if (OnContractExecuted != null)
            {
                OnContractExecuted(contractId, basePrice);
            }
            log.Info(string.Format("结束手动行权:{0}-{1}", contractId, basePrice));
        }
        /// <summary>
        /// 自动行权
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void exeTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            try
            {
                exeTimer.Stop();
                if (isTimerFirstExe)
                {
                    isTimerFirstExe = false;
                    exeTimer.Interval = 1000 * 60 * 60 * 24;
                }
                ExecuteAll();
            }
            catch (Exception ex)
            {
                Singleton<TextLog>.Instance.Error(ex, "execute all");
            }
            finally
            {
                BtcExeBasePrice = null;
                exeTimer.Start();
            }
        }
        public void ExecuteAll()
        {
            BtcExeBasePrice = BtcPrice.Current;
            var c = OptionService.Model.Contracts.Where(a => a.IsNotInUse == false && a.ExcuteTime.Date <= DateTime.Now.Date);
            if (c == null) return;
            c = c.ToList();
            foreach (var qq in c)
            {
                try
                {
                    var name = OptionService.Model.Contracts.Where(a => a.IsDel == false && a.Id == qq.Id).FirstOrDefault();
                    OptionService.Executor.ManualExcute(qq.Id, (decimal)BtcExeBasePrice, OptionService.MarketBoard, name.Name);
                    Singleton<TextLog>.Instance.Info(string.Format("自动行权:合约编号{0},行权基准价{1},执行人{2}", qq.Id, (decimal)BtcExeBasePrice, "系统"));
                }
                catch (Exception ex)
                {
                    Singleton<TextLog>.Instance.Error(ex, "execute all");
                }
            }
        }

        public void Dispose()
        {
            if (log != null)
            {
                log.Dispose(); log = null;
            }

            if (exeTimer != null)
            {
                exeTimer.Elapsed -= exeTimer_Elapsed;
                exeTimer.Dispose(); exeTimer = null;
            }
        }
    }
}
