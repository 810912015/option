using Com.BitsQuan.Option.Core;
using Com.BitsQuan.Option.Imp;
using Com.BitsQuan.Option.Match.Imp;
using Com.BitsQuan.Option.Match.Imp.Share;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
namespace Com.BitsQuan.Option.Match
{ 
    /// <summary>
    /// 期权交易服务:主程序
    ///     现包含所有逻辑
    /// </summary>
    public class MatchService : IDisposable
    {
        public OrderMatcher Matcher { get; private set; }
        public OrderExecutor Executor { get; private set; }
        public OrderMonitor Monitor { get; private set; }
        OrderMonitorHandler monitorHandler;
        OrderPreHandler preHandler;
        OrderPostHandler postHandler;
        Market market;
        public IOptionModel Model { get; private set; }
        public KlineSvc ks { get; private set; }
        public DeepDataPool2 DeepPool { get; private set; }
        SysShare sysShare;
        public EntitySvcImp esi { get; private set; }
        static TextLog log = new TextLog("matcher.txt");
        public event Action<string, string> OnUserMsge;
        OrderSvcImp processor;
        public PositionSvcImp PosSvc { get; private set; }
        public void Dispose()
        {
            if (log != null)
            {
                log.Dispose(); log = null;
            }

            ks.Dispose();
            if (Matcher != null)
            {
                Matcher.OnBeforeMatch -= matcher_OnBeforeMatch;
                Matcher.OnFinish -= matcher_OnFinish;
                Matcher.OnPositionChanged -= matcher_OnPositionChanged;
                Matcher.OnPartialFinish -= matcher_OnPartialFinish;
                Matcher.OnDeal -= Matcher_OnDeal;
                Matcher.Dispose(); Matcher = null;
            }
            if (Model != null)
            {
                Model.Dispose(); Model = null;
            }
            if (Executor != null)
            {
                Executor.Dispose(); Executor = null;
            }

        }
        public MatchService()
        {
            preHandler = new OrderPreHandler();
            postHandler = new OrderPostHandler();
            Model = new OptionModel();
            Model.Init();

            esi = new EntitySvcImp(Model);
            market = new Market(Model);

            Matcher = new OrderMatcher(market);
            Matcher.OnAfterMatch += RaiseAfterMatch;

            foreach (var v in Model.LegacyOrders)
            {
                market.Handle(v);
            }

            preHandler.Market = market;
            Monitor = new OrderMonitor(Matcher, market, Model);

            monitorHandler = new OrderMonitorHandler(Monitor, (a, b) => { if (OnUserMsge != null) OnUserMsge(a, b); });


            sysShare = new SysShare(Model.Traders);

            ks = new KlineSvc(this.Model);
            Matcher.OnFinish += matcher_OnFinish;
            Matcher.OnPositionChanged += matcher_OnPositionChanged;
            Matcher.OnPartialFinish += matcher_OnPartialFinish;
            Matcher.OnDeal += Matcher_OnDeal;

         
            foreach (var v in Model.Traders)
            {
                v.ReraiseBailEvent();
                v.InitBailEvent();
            }

            Model.Traders.First().SetMarket(market);

            DeepPool = new DeepDataPool2(Model, Matcher.Container);
            //恢复最近的成交
            foreach (var v in Model.LatestDeals)
            {
                market.Deals.Handle(v);
            }
            Model.LatestDeals.Clear();
            processor = new OrderSvcImp(this.Model, this.preHandler,
                this.postHandler,this.Matcher, log, IsAcceptByFuse);
            ks.Init();
            PosSvc = new PositionSvcImp(this.market);
        }
        public void Start()
        {
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
            //初始化未处理委托
            foreach (var v in Model.LegacyOrders.ToList())
            {
                v.GetSellOpenCountToFreeze();
                Matcher.Handle(v);
            }
            
            Executor = new OrderExecutor(Model.Traders, this, (t, p) =>
            {
               PosSvc.RaisePC(p, t);

            });
            PosSvc.pds = Executor.pds;
            Executor.OnContractExecuted += (a, b) => { Model.RemoveContract(a, b); };
            Monitor.Start();

            
        }
        void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            Singleton<TextLog>.Instance.Info(e.ExceptionObject.ToString());
            Singleton<TextLog>.Instance.Flush();
        }
        void Matcher_OnDeal(Deal obj)
        {
            LogError(() =>
            {
                Model.DbModel.SaveDeal(obj);
                market.HandleDeal(obj);
                ks.ohlcGen.Handle(obj);
            });
        }
        void matcher_OnPartialFinish(Order arg1, int arg2)
        {
            LogError(() =>
            {
                //Model.UpdatePartialOrder(arg1);
                market.Handle(arg1);

                var p = arg1.Trader.GetPositionSummary(arg1.Contract.Code, PositionType.权利仓);

                PosSvc.RaisePC(p, arg1.Trader);
            });
        }
        void matcher_OnPositionChanged(Trader obj, List<UserPosition> ups, bool isAdd)
        {
            LogError(() =>
            {
                PosSvc.UpdatePositionSummary(obj, ups, isAdd);
                Model.UpdateTraderPosition(obj, ups, isAdd);
                market.HandlePosition(ups, isAdd);

            });
        }
        void matcher_OnFinish(Order obj)
        {
            LogError(() =>
            {
                Model.UpdateOrder(obj);
                obj.Trader.Orders().Remove(obj);

                market.Handle(obj);
                var p = obj.Trader.GetPositionSummary(obj.Contract.Code, PositionType.权利仓);

                PosSvc.RaisePC(p, obj.Trader);
            });
        }
        void matcher_OnBeforeMatch(Order obj)
        {
            LogError(() =>
            {
                market.Handle(obj);
            });
        }
        bool IsAcceptByFuse(Order o)
        {
            var m = market.Get(o.Contract.Name);
            if (m == null) return true;
            var fr = m.fuser.ShouldAccept(o);
            return fr;
        }
        public OrderResult AddOrder(int who, int contract, TradeDirectType dir, OrderType orderType,
            OrderPolicy policy, int count, decimal price,
            string userOpId = "")
        {
            return processor.AddOrder(who, contract, dir, orderType, policy, count, price, userOpId);
        }
        /// <summary>
        /// 撮合结束事件
        /// </summary>
        public event Action<Order> OnAfterMatch;
        void RaiseAfterMatch(Order o)
        {
            if (o.State == OrderState.等待中 || o.State == OrderState.部分成交)
            {
                Model.AddOrder(o);
            }

            if (OnAfterMatch != null)
            {
                foreach (var v in OnAfterMatch.GetInvocationList())
                {
                    try
                    {
                        ((Action<Order>)v)(o);
                    }
                    catch (Exception e)
                    {
                        Singleton<TextLog>.Instance.Error(e);
                        continue;
                    }
                }
            }
        }
        bool s = false;
        public bool IsStopped
        {
            get { return s; }
            private set
            {
                if (s == value) return;
                s = value; processor.IsStoped = value;
                esi.IsStoped = value;
            }
        }
        public void Flush()
        {
            if (IsStopped) return;

            IsStopped = true;

            Thread.Sleep(30 * 1000);

            Model.Flush();

            FlushOrder fo = new FlushOrder();
            fo.Flush((sb, nk) =>
            {
                foreach (var v in Matcher.Container.Orders.Values)
                {
                    foreach (var s in v.BuyQueue)
                    {
                        if (s.State == OrderState.部分成交 || s.State == OrderState.等待中)
                            fo.FlushTempOrder(sb, s, nk);
                    }
                    foreach (var b in v.SellQueue)
                    {
                        if (b.State == OrderState.部分成交 || b.State == OrderState.等待中)
                            fo.FlushTempOrder(sb, b, nk);
                    }
                }
            });
            Singleton<TextLog>.Instance.Info("期权交易系统成功停止");
            log.Flush();
        }
      
        public OperationResult RedoOrder(int who, int orderId)
        {
            return processor.RedoOrder(who, orderId);
        }

        public Market MarketBoard
        {
            get { return market; }
        }

        OperationResult HandleError(Func<OperationResult> f)
        {
            try
            {
                if (IsStopped) return new OperationResult { ResultCode = 330, Desc = "系统维护中,请稍后重试" };
                return f();
            }
            catch (Exception e)
            {
                Singleton<TextLog>.Instance.Error(e);
                return new OperationResult(900, "服务器内部错误:" + e.Message);
            }
        }
         
        void LogError(Action a)
        {
            try
            {
                a();
            }
            catch (Exception e)
            {
                Singleton<TextLog>.Instance.Error(e);
            }
        }
    }
}
