using Com.BitsQuan.Option.Core;
using Com.BitsQuan.Option.Imp;
using System;
using System.Collections.Generic;
using System.Linq;
using Com.BitsQuan.Option.Match.Imp;

namespace Com.BitsQuan.Option.Match
{
    /// <summary>
    /// 撮合比较器基类:同时启用交易策略
    /// </summary>
    public abstract class Arrange
    {
        protected IMatcherDataContainer Container;
        protected Action<Order, Order,decimal> SaveDeal;
        protected Action<Order,bool> Matched;
        protected Action<Order, int> PartialMatched;
        protected Action<Order, int,bool> PartialMatchedTrue;
        protected Func<Order, bool> Redo;
        public Market Market { get; set; }
        /// <summary>
        /// 撮合
        /// 若反方向队列中存在合约,价格匹配,
        /// 则数量较小的完全成交,数量较大的减去成交的数量
        /// 否则不操作
        /// </summary>
        /// <param name="o"></param>
        public abstract void Match(Order o);
        /// <summary>
        /// 根据用户资金判断是否能够成交
        ///     返回
        ///         0:能够成交
        ///         1:主委托保证金不足
        ///         2:从委托保证金不足
        ///         
        /// </summary>
        /// <param name="main"></param>
        /// <param name="slave"></param>
        /// <returns></returns>
        protected int CouldMatchByBail(Order main, Order slave)
        {
            var count = main.Count >= slave.Count ? slave.Count : main.Count;
            var price = slave.Price;
            var total = count * price ;
            int r = 0;
            if (main.Direction == TradeDirectType.买)
            {
                if (main.OrderType == OrderType.开仓)
                {
                    var mr = main.Trader.CouldPay(total, Market);
                    if (!mr)
                    {
                       // main.Detail = "保证金不足,此委托将被撤销";
                        Redo(main);
                        r = 1;
                    }
                }
                
            }
            if (slave.Direction == TradeDirectType.买)
            {
                if (slave.OrderType == OrderType.开仓)
                {
                    var sr = slave.Trader.CouldPay(total, Market);
                    if (!sr)
                    {
                        //slave.Detail = "保证金不足,此委托将被撤销";
                        Redo(slave);
                        r = 2;
                    }
                }
               
            }
            log.Info(string.Format("撮合结果:{0},数量:{1},价格:{2},金额:{3},主:{4},从:{5}", r, count, price, total, main.ToShortString(), slave.ToShortString()));
            return r;
        }
        public static readonly TextLog log = new TextLog("arrange.txt");
        protected abstract void HandleFuse(Order main, Order slave,decimal boundaryPrice);
        /// <summary>
        /// 匹配的数量处理
        /// 返回最后成交价格用于交易策略处理
        /// </summary>
        /// <param name="r"></param>
        /// <param name="o"></param>
        /// <returns>最后的成交价格</returns>
        protected virtual decimal HandleCount(IEnumerable<Order> source,Order o)
        {
            if (source == null||source.Count()==0) return -1;
            var r = source.ToList();
            decimal p = -1;
            foreach (var v in r)
            {
                //如果不是可成交状态,忽略 
                if (!v.IsArrangable()) continue;
                if(v.IsArranging()) continue;
                var fu = Market.Get(o.Contract.Name).fuser;
                var fur = fu.ShouldAllowDeal(v.Price);
                if (fur!=0)
                {
                    HandleFuse(o, v,fur==1?fu.MaxPrice??0:fu.MinPrice??0);
                    log.Info(string.Format("因超出价格范围不能成交:成交价{0},最高价{1},最低价{2},主{0},从{1}", v.Price, fu.MaxPrice ?? 0, fu.MinPrice ?? 0, o.ToShortString(), v.ToShortString()));
                    //o.Detail = "因超出价格范围不能成交,此委托将被撤销";
                    Redo(o); 
                    if (o.State == OrderState.已撤销)
                    {
                        break;
                    }
                    else
                    {
                        continue;
                    }
                }
                if (v.Count == o.Count)
                {
                    var cm=CouldMatchByBail(o, v);
                    if (cm == 0)
                    {
                        v.BeginArrange();
                        SaveDeal(o, v, v.Price);
                        
                            Matched(v, v.Trader != o.Trader);
                            Matched(o, v.Trader != o.Trader); 
                       
                           
                        v.EndArrange();
                        return v.Price; ;
                    }
                    else if (cm == 1) break;
                    else if (cm == 2) continue;
                    
                   
                }
                else
                    if (v.Count > o.Count)
                    {
                        var cm = CouldMatchByBail(o, v);
                        if (cm == 0)
                        {
                            v.BeginArrange();
                            SaveDeal(o, v, v.Price);
                            Matched(o, v.Trader != o.Trader);
                            lock (v)
                            {
                                v.Count -= o.Count;
                            }
                            PartialMatchedTrue(v, o.Count, v.Trader != o.Trader);
                            v.EndArrange();
                            return v.Price;
                        }
                        else if (cm == 1) break;
                        else if (cm == 2) continue;
                    }
                    else
                    {
                        var cm = CouldMatchByBail(o, v);
                        if (cm == 0)
                        {
                            v.BeginArrange();
                            SaveDeal(o, v, v.Price);
                            Matched(v, v.Trader != o.Trader);
                            lock (o)
                            {
                                o.Count -= v.Count;
                            }
                            PartialMatchedTrue(o, v.Count, v.Trader != o.Trader);
                            p = v.Price;
                            v.EndArrange();
                        }
                        else if (cm == 1) break;
                        else if (cm == 2) continue;
                    }
            }
            return p;
        }

    }
     
}
