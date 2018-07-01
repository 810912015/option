using Com.BitsQuan.Option.Core;
using System.Collections.Generic;

namespace Com.BitsQuan.Option.Match
{

    public interface IContainer
    {
        bool Add(Order o);
        bool Remove(Order o);
        /// <summary>
        /// 查找委托的可交易对象
        /// </summary>
        /// <param name="o"></param>
        /// <returns></returns>
        List<Order> FindOpposite(Order o);
        List<Order> FindOppositeGFD(Order o);
    }
}
