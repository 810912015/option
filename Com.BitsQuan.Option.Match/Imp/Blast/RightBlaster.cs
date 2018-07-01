using Com.BitsQuan.Option.Core;
using Com.BitsQuan.Option.Imp;
using System;
using System.Collections.Generic;
using System.Linq;
using Com.BitsQuan.Option.Match.Imp;

namespace Com.BitsQuan.Option.Match
{

    /// <summary>
    /// 权利仓爆仓:系统强行卖平权利仓--即是2.8权利仓自动卖平
    /// </summary>
    public class RightBlaster : Blaster
    {
        public RightBlaster(IMatch match, Market m, IOptionModel model)
            : base(match, m,model) 
        { }
        /// <summary>
        /// 要平的是权利仓
        /// </summary>
        protected override PositionType positionType
        {
            get { return PositionType.权利仓; }
        }
        /// <summary>
        /// 可能的买家
        /// </summary>
        /// <param name="up"></param>
        /// <returns></returns>
        protected override IEnumerable<Order> GetPossibleMatch(PositionSummary up)
        {
            return matcher.Container.GetByDir(up.CCode, TradeDirectType.买); 
        }
        /// <summary>
        /// 生成卖平的委托
        /// </summary>
        /// <param name="up"></param>
        /// <param name="total"></param>
        /// <returns></returns>
        protected override Tuple<Order,bool> CreateSellOrder(Trader t, PositionSummary up, int total)
        {
            decimal price;
            var pm = GetPossibleMatch(up).FirstOrDefault();
            if (pm == null)
            { 
                var fuse=GetFuser(up);
                price = (decimal)fuse.MinPrice;

                var closableCount = t.GetClosableCount(up.CCode,TradeDirectType.卖);

                var r = new Order
                {
                    Id = IdService<Order>.Instance.NewId(),
                    OrderPolicy = OrderPolicy.限价申报,
                    Price = price,
                    Count =closableCount,// total,
                    ReportCount = closableCount,//total,
                    Contract = up.GetContract(this.model),//.Order.Contract,
                    Direction = TradeDirectType.卖, 
                    OrderType = OrderType.平仓,
                    OrderTime = DateTime.Now,
                    Trader = t,
                    State = OrderState.等待中,
                    IsBySystem = true
                };
                PutInReorder(r, fuse, false,up);
                return Tuple.Create(r,true);
            }
            else
            {  
                price = pm.Price;
                //下单数量是1数量和平仓数量的较小值
                var pcount = pm.Count>=total?total:pm.Count;
                if (pcount > SysPrm.Instance.MonitorParams.MaxCountPerSell)
                    pcount = SysPrm.Instance.MonitorParams.MaxCountPerSell;
                var r= new Order
                {
                    Id = IdService<Order>.Instance.NewId(),
                    OrderPolicy = OrderPolicy.市价IOC,
                    Price = price,
                    Count =pcount,
                    ReportCount =pcount,
                    Contract = up.GetContract (this .model),
                    Direction = TradeDirectType.卖, 
                    OrderType = OrderType.平仓,
                    OrderTime = DateTime.Now,
                    Trader = t,
                    State = OrderState.等待中,
                    IsBySystem = true,
                };
                return Tuple.Create(r, false);
            } 
        }
    }
}
