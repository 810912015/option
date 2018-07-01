using Com.BitsQuan.Option.Core;
using System.Collections.Generic;

namespace Com.BitsQuan.Option.Match
{
    public interface IOrderContainer : IContainer
    {
        /// <summary>
        /// 根据买卖开平查找对象
        /// </summary>
        /// <param name="dir">卖还是买</param>
        /// <param name="pos">开还是平</param>
        /// <returns></returns>
        List<Order> FindByDirAndPos(TradeDirectType dir, OrderType orderType); 
        /// <summary>
        /// 队列1数量:卖开,卖平队列
        /// </summary>
        int SellOrderCount { get; }
        /// <summary>
        /// 队列2数量:买开,买平队列
        /// </summary>
        int BuyOrderCount { get; }
        List<Order> SellQueue { get; }
        List<Order> BuyQueue { get; }
        //int MyCloseOrderCount(Trader t, TradeDirectType dir);
       
        decimal Buy1Price { get; }
        decimal Sell1Price { get; }
        
        int Buy1Count { get; }
        int Sell1Count { get; }
    }
}
