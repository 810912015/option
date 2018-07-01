using Com.BitsQuan.Option.Core;
using Com.BitsQuan.Option.Imp;
using System;
using System.Collections.Generic;
using System.Linq;
using Com.BitsQuan.Option.Match.Imp;

namespace Com.BitsQuan.Option.Match
{
    /// <summary>
    /// 义务仓爆仓:系统强行卖平义务仓--即是2.9爆仓
    /// </summary>
    public class DutyBlaster : Blaster
    {
        public DutyBlaster(IMatch match, Market m,IOptionModel model) : base(match,m,model) {  }
        /// <summary>
        /// 要卖平的是义务仓
        /// </summary>
        protected override PositionType positionType
        {
            get { return PositionType.义务仓; }
        }
        /// <summary>
        /// 找出义务仓的可能成交对象
        /// </summary>
        /// <param name="up"></param>
        /// <returns></returns>
        protected override IEnumerable<Order> GetPossibleMatch(PositionSummary up)
        {
            return matcher.Container.GetByDir(up.CCode, TradeDirectType.卖);
        }
        /// <summary>
        /// 生成买平操作的委托
        /// </summary>
        /// <param name="up"></param>
        /// <param name="total"></param>
        /// <returns></returns>
        protected override Tuple<Order, bool> CreateSellOrder(Trader t, PositionSummary up, int total)
        {
            decimal price;
            var fuse=GetFuser(up);
            var pm = GetPossibleMatch(up)
                .Where(a=>a.Price<(decimal)fuse.MaxPrice&&a.Price>(decimal)fuse.MinPrice)
                .OrderBy(a=>a.Price)
                .FirstOrDefault();
            if (pm == null)
            {
                price =(decimal) fuse.MaxPrice;

                var closableCount =t.GetClosableCount(up.CCode,TradeDirectType.买);

                var r= new Order
                {
                    Id = IdService<Order>.Instance.NewId(),
                    OrderPolicy = OrderPolicy.限价申报,
                    Price = price,
                    Count =closableCount,
                    ReportCount = closableCount,
                    Contract = up.GetContract(this.model),
                    Direction = TradeDirectType.买, 
                    OrderType = OrderType.平仓,
                    OrderTime = DateTime.Now,
                    Trader = t,
                    State = OrderState.等待中,
                    IsBySystem = true
                };
                PutInReorder(r, fuse, true,up);
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
                    ReportCount = pcount,
                    Contract = up.GetContract(this.model),
                    Direction = TradeDirectType.买, 
                    OrderType = OrderType.平仓,
                    OrderTime = DateTime.Now,
                    Trader = t,
                    State = OrderState.等待中,
                    IsBySystem = true
                };
                return Tuple.Create(r, false);
            }
            
        }
    }

}
