using Com.BitsQuan.Option.Core;
using Com.BitsQuan.Option.Imp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Timers;
using Com.BitsQuan.Option.Match.Imp;

namespace Com.BitsQuan.Option.Match
{
    /// <summary>
    /// 重新报价器
    /// 当熔断边界改变时重新报价
    /// </summary>
    public class ReorderItem : IDisposable
    {
        IMatch matcher;
        Order o;
        /// <summary>
        /// 当前委托
        /// </summary>
        public Order order{get;private set;}
        /// <summary>
        /// 持仓编号
        /// </summary>
        public PositionSummary Pos { get; private set; }
         List<int> HisOrderIds { get;  set; }
        /// <summary>
        /// 是否是本次操作的订单:一个重新下单操作可能有多余一个订单
        /// </summary>
        /// <param name="orderId"></param>
        /// <returns></returns>
         public bool IsMyOrder(int orderId)
         {
             return HisOrderIds.Contains(orderId);
         }
       
        public event Action<ReorderItem> OnDone;
        ContractFuse cf;
        bool isHandleMax;
        /// <summary>
        /// 如果超过熔断时间且价格不等于熔断价格,则重新按熔断价格报出
        /// </summary>
        Timer checkTimer;
        DateTime CreateTime;

        public ReorderItem(Order up, ContractFuse cf, bool isHandelMax, PositionSummary pos, IMatch matcher)
        {
            this.matcher = matcher;
            Blaster.Log.Info(string.Format("平仓委托重新报价加入队列:{0}-大{1}-边{2},{3}-单{4}", up.Trader.Name, isHandelMax, cf.MaxPrice, cf.MinPrice, up.ToShortString()));
            this.order = up; this.cf = cf; this.Pos = pos;
            HisOrderIds = new List<int> { up.Id };
            this.isHandleMax = isHandelMax;
            //引发熔断--
            cf.ShouldAccept(up);
            if (isHandelMax)
            {
                cf.OnMaxChanged += cf_OnMaxChanged;
            }
            else
            {
                cf.OnMinChanged += cf_OnMinChanged;
            }
            cf.Excutor.OnFuseOver += Excutor_OnFuseOver;
            CreateTime = DateTime.Now;
            checkTimer = new Timer();
            checkTimer.Interval = ContractFuse.FuseSpanInMin / 2 * 60 * 1000;
            checkTimer.Elapsed += checkTimer_Elapsed;
            checkTimer.Start();
        }

        void checkTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            try
            {
                checkTimer.Stop();

                if (CreateTime.AddMinutes(ContractFuse.FuseSpanInMin) >= DateTime.Now)
                    return;

                var boundary = (isHandleMax ? cf.MaxPrice : cf.MinPrice)??-1;

                if (boundary == -1) return;

                if (order.Price == boundary) return;

                Excutor_OnFuseOver((decimal)cf.MaxPrice, (decimal)cf.MinPrice);
                
            }
            catch (Exception ex)
            {
                Singleton<TextLog>.Instance.Error(ex, "reorderitme check");
            }
            finally
            {
                if (checkTimer != null)
                {
                    checkTimer.Start();
                }
            }
        }

        void Excutor_OnFuseOver(decimal arg1, decimal arg2)
        {
            if (order.IsArranging()) return;
            CreateTime = DateTime.Now;
            if (isHandleMax)
            {
                cf_OnMaxChanged(this.order.Contract, arg1);
            }
            else
            {
                cf_OnMinChanged(this.order.Contract, arg2);
            }
        }
        bool IsAlive()
        {
            var notDone = order.State == OrderState.部分成交 || order.State == OrderState.等待中;
            return notDone;
        }
        void cf_OnMinChanged(Contract arg1, decimal? arg2)
        {
            CreateAndReorder(arg1, arg2, false);
        }
        /// <summary>
        /// 重新报价
        /// 1：重新报价
        /// 2：没有重新报价（因为上一单正在撮合等原因）
        /// 3：重新报价而且已成交
        /// </summary>
        /// <param name="newOrder"></param>
        /// <returns></returns>
        int ReOrder(Order newOrder)
        {
            if (matcher.Redo(order))
            {
                this.order = newOrder;
                HisOrderIds.Add(newOrder.Id);
                matcher.Handle(newOrder);
                if (newOrder.State != OrderState.等待中 && newOrder.State != OrderState.部分成交)
                {
                    if (OnDone != null)
                        OnDone(this);
                    return 3;
                }
                return 1;
            }
            return 2;
        }
        /// <summary>
        /// 清除重新报价项
        /// </summary>
        /// <param name="isUpChanged"></param>
        void Clear(bool isUpChanged,string reason)
        {
            Blaster.Log.Info(string.Format("{0}限变动,{1},重新报价结束:{2}-{3}", isUpChanged ? "上" : "下",reason, order.Trader.Name, order.ToShortString()));
            if (isUpChanged)
                cf.OnMaxChanged -= cf_OnMaxChanged;
            else
                cf.OnMinChanged -= cf_OnMinChanged;
            if (OnDone != null)
            {
                OnDone(this);
            }
            this.Dispose();
        }

        void CreateAndReorder(Contract arg1, decimal? arg2, bool isUpChanged)
        {
            if (!IsAlive())
            {
                Clear(isUpChanged,"原单已成交");
                return;
            }
            var ratio = order.Trader.GetMaintainRatio();
            if (ratio >= 1)
            {
                Clear(isUpChanged,"保证率>=1");
                return;
            }
            var pos = order.Trader.GetPositionSummary(arg1.Code, PositionType.权利仓);
            if (pos == null)
            {
                Clear(isUpChanged,"已无持仓");
                return;
            }
            var closable = order.Trader.GetClosableCount(pos);
            if (closable<=0)
            {
                Clear(isUpChanged,"可平<=0"); return;
            }

            if (pos.PositionType == "义务仓")
            {
                if (order.Direction == TradeDirectType.卖)
                {
                    Clear(isUpChanged, "是义务仓但单是卖单");
                    return;
                }
            }
            else
            {
                if (order.Direction == TradeDirectType.买)
                {
                    Clear(isUpChanged, "是权利仓但单是买单");
                    return;
                }
            }
            var neworder = new Order
            {
                Id = IdService<Order>.Instance.NewId(),
                OrderPolicy = OrderPolicy.限价申报,
                Price =isUpChanged? (decimal)cf.MaxPrice:(decimal)cf.MinPrice,
                Count = closable,
                ReportCount = closable,
                Contract = order.Contract,
                Direction =pos.PositionType == "义务仓" ? TradeDirectType.买 : TradeDirectType.卖,
                OrderType = order.OrderType,
                OrderTime = DateTime.Now,
                Trader = order.Trader,
                State = OrderState.等待中,
                IsBySystem = true
            };
           

            var os = order.ToShortString();
            var ps = order.Price.ToString();
            var ro = ReOrder(neworder);
            if (ro != 2)
            {
                Blaster.Log.Info(string.Format("{0}限变化重新报价:{1}-原{2}-现{3}-旧{4}-新{5}", isUpChanged ? "上" : "下", order.Trader.Name,
              ps, neworder.Price, os, neworder.ToShortString()));
            }
            if (ro == 1)
            {
                //引发下一次熔断事件
                cf.ShouldAccept(neworder);
            }

        }
        void cf_OnMaxChanged(Contract arg1, decimal? arg2)
        {
            CreateAndReorder(arg1, arg2, true);
        }
        bool disposed = false;
        public void Dispose()
        {
            if (disposed) return;
            disposed = true;
            order.Trader.SetSell(Pos, false);
            if(matcher!=null)
            matcher = null;
            cf.OnMinChanged -= cf_OnMaxChanged;
            cf.OnMaxChanged -= cf_OnMaxChanged;
            cf.Excutor.OnFuseOver -= Excutor_OnFuseOver;
            if (checkTimer != null)
            {
                checkTimer.Stop();
                checkTimer.Dispose();
                checkTimer = null;
            }
        }
    }
}
