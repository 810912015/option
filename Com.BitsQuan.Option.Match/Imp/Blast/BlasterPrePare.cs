using Com.BitsQuan.Option.Core;
using Com.BitsQuan.Option.Imp;
using System;
using System.Collections.Generic;
using System.Linq;
using Com.BitsQuan.Option.Match.Imp;
namespace Com.BitsQuan.Option.Match
{        
    /// <summary>
    /// 爆仓前准备
    ///     撤单->转入
    /// </summary>
    public class BlasterPrePare
    {
        IMatch matcher;
        decimal preBlastThreshold;
        Market m;
        public BlasterPrePare(IMatch match,Market m)

        {
            this.m = m;
            preBlastThreshold = 1m;
            this.matcher = match; 

        }
       /// <summary>
        /// 爆仓准备
       /// </summary>
       /// <param name="t">要爆仓的用户</param>
       /// <returns>true表示保证率满足要求,false表示保证率不满足要求</returns>
        public bool Blast(Trader t)
        {
            //撤单
            while (t.GetMaintainRatio(m) < preBlastThreshold)//true保证率>1,false|保证率依旧<1
            {
                var couldRedo = AutoRedo(t);
                if (!couldRedo) break;
            }
             
            //保证金自动转入
            if (t.GetMaintainRatio(m) < preBlastThreshold)
            {
                if (t.IsAutoAddBailFromCache)
                {
                    var delta = t.GetMaintain(m) * 1.1m - t.Account.BailAccount.Sum;

                    if (t.Account.CacheAccount.CnyAccount.Sum <= 100)
                    {
                        delta = t.Account.CacheAccount.CnyAccount.Sum;
                    }
                    var r = TraderService.OperateAddBailFromCache(t, delta, null);
                    Blaster.Log.Info(string.Format("保证金转入:{0}-金额{1}-自动转入{2}-转入结果{3}", t.Name, delta, t.IsAutoAddBailFromCache, r));
                }
            }
            var rrr = t.GetMaintainRatio(m) >= preBlastThreshold;
            return rrr;
        }
        /// <summary>
        /// 保证金自动转入事件:用户名,金额
        /// </summary>
        public event Action<string, decimal> OnBailAutoCollected;
        void RaiseCollected(string tn, decimal delta)
        {
            if (OnBailAutoCollected != null)
            {
                foreach (var v in OnBailAutoCollected.GetInvocationList())
                {
                    try
                    {
                        ((Action<string, decimal>)v).BeginInvoke(tn, delta, null, null);
                    }
                    catch (Exception e)
                    {
                        Singleton<TextLog>.Instance.Error(e);
                    }
                }
            }
        }
        /// <summary>
        /// 撤单开始事件:用户名,委托描述
        /// </summary>
        public event Action<string, string> OnRedoing;
        /// <summary>
        /// 撤单结束事件:用户名,委托描述
        /// </summary>
        public event Action<string, string> OnRedoed;

        void RaiseRedo(string tn, string od, Action<string, string> a)
        {
            if (a != null)
            {
                foreach (var v in a.GetInvocationList())
                {
                    try
                    {
                        ((Action<string, string>)v).BeginInvoke(tn, od, null, null);
                    }
                    catch (Exception e)
                    {
                        Singleton<TextLog>.Instance.Error(e);
                    }
                }
            }
        }

        /// <summary>
        /// 爆仓前准备中的撤单操作
        /// 是否还有可以撤的单子
        /// </summary>
        /// <param name="t"></param>
        bool AutoRedo(Trader t)
        {
            if (t == null || t.Orders() == null || t.Orders().Count == 0) return false;

            var pos = t.Orders().GetByDirAndOrderType(TradeDirectType.卖,OrderType.开仓)
                .OrderBy(a => a.Contract.ExcuteTime)
                .ThenBy(a => a.Contract.OptionType)
                .ThenBy(a => a.Contract.ExcutePrice);

            var pos2 = t.Orders().GetByDirAndOrderType(TradeDirectType.买, OrderType.开仓)
                .OrderBy(a => a.Contract.ExcuteTime)
                .ThenBy(a => a.Contract.OptionType)
                .ThenBy(a => a.Contract.ExcutePrice);

            var pos3 = t.Orders().GetByDirAndOrderType(TradeDirectType.卖 , OrderType.平仓)
                .OrderBy(a => a.Contract.ExcuteTime)
                .ThenBy(a => a.Contract.OptionType)
                .ThenBy(a => a.Contract.ExcutePrice);

            var pos4 = t.Orders().GetByDirAndOrderType(TradeDirectType.买,OrderType.平仓)
                .OrderBy(a => a.Contract.ExcuteTime)
                .ThenBy(a => a.Contract.OptionType)
                .ThenBy(a => a.Contract.ExcutePrice);
            List<Order> orders = new List<Order>();
            if (pos != null) orders.AddRange(pos);
            if (pos2 != null) orders.AddRange(pos2);
            if (pos3 != null) orders.AddRange(pos4);
            if (pos3 != null) orders.AddRange(pos4);
            if (orders.Count == 0) return false;
            var r = false;
            foreach (var v in orders)
            {
                var ts = v.ToShortString();
                RaiseRedo(t.Name,ts, OnRedoing);
                matcher.Redo(v);
                t.Orders().Remove(v);
                Blaster.Log.Info(string.Format("撤单:{0}-单{1}-率{2}", t.Name, v.ToShortString(), t.GetMaintainRatio(m)));
                RaiseRedo(t.Name, ts, OnRedoed);
                if (t.GetMaintainRatio(m) >= 1)
                {
                    r = true;
                    break;
                }
            } 
            return r;
             
        }
        
    }
}
