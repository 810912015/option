using Com.BitsQuan.Option.Core;
using Com.BitsQuan.Option.Imp;
using Com.BitsQuan.Option.Match.Imp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Com.BitsQuan.Option.Match
{
    public abstract class FlushBase<T>
    {
        public abstract void FlushTempOrder(StringBuilder sb, T o, int nk);
        protected abstract string GKey { get; }
        protected int GetNewGenaration()
        {
            GlobalPrmService gps = new GlobalPrmService();
            gps.Init();
            var k = gps.GetInt(GKey);
            var nk = (k ?? 1) + 1;
            gps.Set(GKey, (nk).ToString());
            gps.Flush();
            return nk;
        }
        public int? GetOldGeneration()
        {
            GlobalPrmService gps = new GlobalPrmService();
            gps.Init();
            var k = gps.GetInt(GKey);
            return k;
        }

        public void Flush(Action<StringBuilder, int> a)
        {
            var nk = GetNewGenaration();
            StringBuilder sb = new StringBuilder();
            a(sb, nk);
            if (sb.Length > 0)
            {
                using (DBServer db = new DBServer())
                {
                    db.ExecNonQuery(sb.ToString());
                }
            }
        }

    }
    public class FlushOrder : FlushBase<Order>
    {

        protected override string GKey
        {
            get { return "latestUndealOrderGeneration"; }
        }
        public override void FlushTempOrder(StringBuilder sb, Order o, int nk)
        {
            string sql = @" update TempOrders set [Count] ={0},DoneCount ={1},TotalDoneCount ={2},[state]={3},DonePrice ={4},TotalDoneSum={5},Detail='{6}'
where Id={7} ;";
            sb.AppendFormat(sql, o.Count, o.DoneCount, o.TotalDoneCount, (int)o.State, o.DonePrice, o.TotalDoneCount, nk, o.Id);
        }
    }

    

    /// <summary>
    /// 撮合程序,同时包含成交后处理
    /// </summary>
    public class OrderMatcher : IMatch
    {
        public IMatcherDataContainer Container { get; private set; }
        Arranger arranger;
        ArrangeChecker arrangeChecker;
        //撮合日志
        TextLog log = new TextLog("orderMatcher.txt");
        public void Dispose()
        {
            if (log != null)
            {
                log.Dispose(); log = null;
            }
            if (Container != null)
            {
                Container.Orders.Clear();
                Container = null;
            }
            if (arrangeChecker != null)
            {
                arrangeChecker.Dispose();
                arrangeChecker = null;
            }

        }
        Market m;
        object matchLock;
        public OrderMatcher(Market m)
        {
            matchLock = new object();
            this.m = m;
            Container = new OrderContainer(this.RaiseOnFinish);
            arranger = new Arranger(Container, SaveDeal, Matched, PartialMatched, Redo, m, PartialMatchedTrue);
            arrangeChecker = new ArrangeChecker(Container, (o) => {
                //因为是合约是从容器中取出来的,所以不需要重新引发开始撮合和撮合结束事件
                DoHandle(o, false);
            }, log);
            arrangeChecker.Start();
        }
        public event Action<Order> OnAfterMatch;
        public void Handle(Order o)
        {
            DoHandle(o, true); 
        }
        void DoHandle(Order o,bool RaiseEvent)
        {
            if(RaiseEvent)
            Raise(OnBeforeMatch, o);
            Match(o);
            //撮合结束,如果没有成交或部分成交,则加入队列等待下一次撮合
            if (o.Price > 0)
            {
                if (o.State == OrderState.等待中 || o.State == OrderState.部分成交)
                {
                    //ioc,fok本身就要撤销的因此不加入容器
                    if (o.OrderPolicy == OrderPolicy.限价申报 || o.OrderPolicy == OrderPolicy.市价剩余转限价)
                    {
                        Container.Add(o);
                    }

                    else
                    {
                        o.Unfreeze();
                        RaiseOnFinish(o);
                    }

                }
                else
                {
                    o.Unfreeze();
                }
                if (RaiseEvent&&OnAfterMatch != null)
                {
                    OnAfterMatch(o);
                }
            }

        }

        /// <summary>
        /// 各项检查均已通过,将要进入撮合:此时应新增委托
        /// </summary>
        public event Action<Order> OnBeforeMatch;
        /// <summary>
        /// 交易结束:成交或者撤销:此时应至修改委托状态,持仓有事件,资金事件在TraderService中
        /// </summary>
        public event Action<Order> OnFinish;
        /// <summary>
        /// 部分成交事件::此时应至修改委托状态,持仓有事件,资金事件在TraderService中
        /// </summary>
        public event Action<Order, int> OnPartialFinish;
        void RaisePartialFinish(Order o, int c)
        {
           // o.Detail = "交易结束";
            if (OnPartialFinish != null)
            {
                OnPartialFinish(o, c);
            }
        }
        void Raise(Action<Order> ent, Order o)
        {
            if (ent != null) ent(o);
        }
        void RaiseOnFinish(Order o)
        {
            Raise(OnFinish, o);
        }
        object redoSync = new object();
        public bool Redo(Order o)
        {
            lock (redoSync)
            {
                if (o == null) return false;
                //如果不是等待中或部分成交,则已经处理完毕,不需要撤销
                if (!o.IsArrangable()) return false;
                //如果正在撮合,不能撤销
                if (o.IsArranging()) return false;
                //撤销同时设置状态
                var r = Container.Remove(o);
                bool success = false;
                if (r)
                {
                    //Unfreeze(o);
                    success = true;
                }
                o.Trader.Orders().Remove(o);
                //有可能已经成交,那就不能再引发完成事件,会倒持重复存储
                if (o.State != OrderState.已撤销 && o.State != OrderState.已成交 && o.State != OrderState.已行权)
                {
                    o.State = OrderState.已撤销;
                    o.Unfreeze();
                    RaiseOnFinish(o);
                }
                else
                {
                    log.Info(string.Format("重复撤销{0}", o.ToString()));
                }

                return success;
            }
        }

        public event Action<Deal> OnDeal;
        /// <summary>
        /// 引发成交事件,并设置委托的成交价格和数量
        /// </summary>
        /// <param name="main"></param>
        /// <param name="slave"></param>
        /// <param name="donePrice"></param>
        void SaveDeal(Order main, Order slave, decimal donePrice)
        {
            main.DonePrice = slave.DonePrice = donePrice;
            var c = (main.Count >= slave.Count) ? slave.Count : main.Count;
            var tdc = c * donePrice;

            main.TotalDoneSum += tdc;
            slave.TotalDoneSum += tdc;

            main.DoneCount = c;
            main.TotalDoneCount += c;

            slave.DoneCount = c;
            slave.TotalDoneCount += c;

            var d = new Deal
            {
                Id = IdService<Deal>.Instance.NewId(),
                MainOrderId = main.Id,
                SlaveOrderId = slave.Id,
                IsPartialDeal = main.Count == slave.Count,
                Count = c,
                DealType = main.OrderType == slave.OrderType ? (main.OrderType == OrderType.开仓 ? DealType.双开 : DealType.双平) : DealType.空换,
                When = DateTime.Now,
                ContractId = main.Contract.Id,
                Price = donePrice,
                MainName = main.Trader.Name,
                SlaveName = slave.Trader.Name
            };
            if (OnDeal != null) OnDeal(d); 

        }

        /// <summary>
        /// 撮合
        /// 若反方向队列中存在合约,价格匹配,
        /// 则数量较小的完全成交,数量较大的减去成交的数量
        /// 否则不操作
        /// </summary>
        /// <param name="o"></param>
        void Match(Order o)
        {
            lock(matchLock)
            arranger.Match(o);
        }
       /// <summary>
        ///  成交处理:
        ///     1.从容器中删除委托;
        ///     2.设置委托状态为已完成;
        ///     3.现金操作;
        ///     4.如果需要执行保证金操作;
        ///     5.更新用户持仓;
        ///     6.引发交易完成事件;
       /// </summary>
       /// <param name="o"></param>
       /// <param name="handlePostion">是否修改持仓:自己和自己成交不修改持仓</param>
        void Matched(Order o,bool handlePostion)
        { 
            Container.Remove(o);
            o.State = OrderState.已成交;
            
            MatchCache(o, o.Count);
            if(handlePostion)
            MatchPosition(o, o.Count);
            RaiseOnFinish(o);
        }
        void PartialMatched(Order o, int c)
        {
            o.State = OrderState.部分成交;
            MatchCache(o, c);
            MatchPosition(o, c);
            RaisePartialFinish(o, c); 
        }
        void PartialMatchedTrue(Order o, int c, bool handlePostion)
        {
            o.State = OrderState.部分成交;
            MatchCache(o, c);
            if (handlePostion)
            MatchPosition(o, c);
            RaisePartialFinish(o, c);
        }
        /// <summary>
        /// 成交后保证金操作:收款,付款,解冻
        /// </summary>
        /// <param name="o"></param>
        /// <param name="count"></param>
        void MatchCache(Order o, int count)
        {
            o.Unfreeze();
           
            if (o.IsCollect())
            {
                bool r = TraderService.OperateAccount(o.Trader, o.DonePrice * o.DoneCount, AccountChangeType.保证金收款, "system", o);
                //if (r) { log.Info(string.Format("保证金收款成功,收款金额:{0}", o.DonePrice * o.DoneCount)); }
            }
            else
            {
                bool r = o.Trader.BailPay(o.DoneCount * o.DonePrice, m, o, AccountChangeType.保证金付款);// TraderService.BailPay(o.Trader, o.DoneCount * o.DonePrice, o);
               // if (r) { log.Info(string.Format("保证金付款成功,付款金额:{0}", o.DonePrice * o.DoneCount)); }
            }

        }
        /// <summary>
        /// 用户持仓变化事件
        ///     参数:用户,仓位,是否是增加持仓
        ///     此时应修改用户的持仓状态
        /// </summary>
        public event Action<Trader, List<UserPosition>, bool> OnPositionChanged;
        void RaiseChanged(Trader c, List<UserPosition> ups, bool IsAdd)
        {
            if (OnPositionChanged != null)
            {
                OnPositionChanged.BeginInvoke(c, ups, IsAdd,null,null);
            }
        }

        /// <summary>
        /// 成交后
        /// 持仓操作
        ///     开仓则增加持仓
        ///     平仓则减少持仓
        /// 保证金操作
        ///     如果是卖保证金,则冻结保证金
        ///     如果卖出去的权利仓自己又买回来了,则保证金解冻
        /// </summary>
        /// <param name="o"></param>
        /// <param name="count"></param>
        void MatchPosition(Order o, int count)
        {
            try
            {
                UserPosition up = new UserPosition //用户当前持仓
                {
                    Id = IdService<UserPosition>.Instance.NewId(),
                    Order = o,
                    Trader = o.Trader,
                    DealTime = DateTime.Now,
                    Count = count
                };
                RaiseChanged(o.Trader, new List<UserPosition> { up },o.IsPositionable()? true:false); 
            }
            catch (Exception e)
            {
                log.Error(e, "持仓更新异常");
            }
        }



    }



}
