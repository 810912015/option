using Com.BitsQuan.Option.Core;
using System;
using System.Collections.Generic;

namespace Com.BitsQuan.Option.Match
{
    /// <summary>
    /// 撮合程序使用的委托容器
    /// </summary>
    public interface IMatcherDataContainer : IContainer
    {
        Dictionary<string, IOrderContainer> Orders { get; }
        List<Order> FindByDirAndPos(string contractCode, TradeDirectType dir, OrderType orderType);
        List<Order> FindByDirAndPos(Order o);
        List<Order> GetByDir(string contractCode, TradeDirectType dir);
        Tuple<decimal, int, decimal, int> Get1PriceAndCount(string code);
        Order GetOrderById(int orderId);
    }
}
