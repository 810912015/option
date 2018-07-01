using Com.BitsQuan.Option.Core;

namespace Com.BitsQuan.Option.Imp
{
    public static class OrderExtension
    {
        /// <summary>
        /// 是否可持仓:开仓即可持仓
        /// </summary>
        /// <param name="o"></param>
        /// <returns></returns>
        public static bool IsPositionable(this Order o)
        {
            return (o.OrderType == OrderType.开仓);
        }
        /// <summary>
        /// 是否应该计算维持保证金:只有卖开(卖出权利仓)才需要维持保证金 
        /// </summary>
        /// <param name="o"></param>
        /// <returns></returns>
        public static bool IsNeedOccupyMaintainBail(this Order o)
        {
            return o.Direction == TradeDirectType.卖 && o.OrderType==OrderType.开仓;
        }
        /// <summary>
        /// 是否需要释放维持保证金:买平-平义务仓
        /// </summary>
        /// <param name="o"></param>
        /// <returns></returns>
        public static bool IsNeedReleaseMaintainBail(this Order o)
        {
            return o.Direction == TradeDirectType.买 && o.OrderType == OrderType.平仓;
        }
        /// <summary>
        /// 是否需要检查保证金账户的金额且不需要维持保证金
        /// </summary>
        /// <param name="o"></param>
        /// <returns></returns>
        public static bool IsNeedBailExceptMaintainBail(this Order o)
        {
            
            return o.Direction == TradeDirectType.买 && o.OrderType == OrderType.开仓;
        }
        /// <summary>
        /// 需要的现金金额
        /// </summary>
        /// <param name="o"></param>
        /// <returns></returns>
        public static decimal GetNeededCache(this Order o)
        {          
            return o.Price * o.Count;
        }
        /// <summary>
        /// 是否是账户收钱
        /// </summary>
        /// <param name="o"></param>
        /// <returns></returns>
        public static bool IsCollect(this Order o)
        {
            return (o.Direction==TradeDirectType.卖);
        }
        
    }
}
