using Com.BitsQuan.Option.Core;
using Com.BitsQuan.Option.Imp;
using Com.BitsQuan.Option.Match.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Com.BitsQuan.Option.Match.Imp
{
    public static class ClosableCountExtension
    {
        public static int GetClosableCount(this Trader t, string code,TradeDirectType dir)
        {
            try
            {
                var p = t.GetPositionSummary(code, PositionType.权利仓);
                if (p == null) return 0;

                if (p.PositionType == "权利仓" && dir == TradeDirectType.买) return 0;
                if (p.PositionType == "义务仓" && dir == TradeDirectType.卖) return 0;


                if (t.Orders() == null || t.Orders().Count == 0) return p.Count;
                var tl = t.Orders();
                if (tl == null) return p.Count;
                var c = tl.GetCloseCount(code, dir);
              
                var r = p.Count - c;
                return r;
            }
            catch (Exception e)
            {
                Singleton<TextLog>.Instance.Error(e, "getclosablecountextension");
                return 0;
            }
        }

        public static int GetClosableCount(this Trader t, PositionSummary ps)
        {
            return t.GetClosableCount(ps.CCode, ps.PositionType == "权利仓" ? TradeDirectType.卖 : TradeDirectType.买);
        }
    }
    public static class TraderPosExtension
    {
        static Dictionary<int, PositionSummaryCollection> dic = new Dictionary<int, PositionSummaryCollection>();
        static object dicSync = new object();

        public static void GetPositionTotal(this MarketDto  c)
        {
            var q = dic.Values.Select(a => a.All.Where(b =>b.PositionType =="义务仓"&& b.CCode == c.Code));
            if (q == null) { c.PositionTotal = 0; return; }
            int count = 0;
            q = q.ToList();
            foreach (var v in q)
            {
                if (q == null || q.Count() == 0) continue;
                foreach (var vv in v)
                {
                    count += vv.Count;
                }
            }

            c.PositionTotal = count;
        }
        public static void Calc(this Trader t,UserPosition up,bool isAdd,  decimal curPrice)
        {
            if (!dic.ContainsKey(t.Id))
            {
                lock (dicSync)
                {
                    if (!dic.ContainsKey(t.Id)) dic.Add(t.Id, new PositionSummaryCollection());
                }
            }
            dic[t.Id].Calc(up, isAdd,  curPrice==0?up.Order.Price:curPrice);
        }
        public static void InitPosition(this Trader t, PositionSummaryData d)
        {
            if (!dic.ContainsKey(t.Id))
            {
                lock (dicSync)
                {
                    if (!dic.ContainsKey(t.Id)) dic.Add(t.Id, new PositionSummaryCollection());
                }
            }
            dic[t.Id].Add(d);
        }
        public static PositionSummary GetPositionSummary(this Trader t, string contractCode, PositionType type)
        {
            if (!dic.ContainsKey(t.Id)) return null;
            return dic[t.Id].Get(contractCode, type);
        }
        public static PositionSummaryCollection GetPsCollection(this Trader t)
        {
            if (!dic.ContainsKey(t.Id)) return null;
            return dic[t.Id];
        }
        
        public static List<PositionSummary> GetPositionSummaries(this Trader t)
        {
            if (!dic.ContainsKey(t.Id)) return new List<PositionSummary>();
            return dic[t.Id].All.OrderByDescending(a=>a.Id).ToList();
        }

        public static void RemovePositionSummary(this Trader t, string code)
        {
            if (!dic.ContainsKey(t.Id)) return;
            var r= dic[t.Id].RemoveByCode(code);
            return;

        }
        public static List<PositionSummary> GetPosByType(this Trader t,PositionType pt)
        {
            if (!dic.ContainsKey(t.Id)||dic[t.Id]==null) return new List<PositionSummary>();
            return dic[t.Id].GetByType(pt);
        }
        public static bool HasPosition(this Trader t,Contract c)
        {
            if (!dic.ContainsKey(t.Id)) return false;
            var ps = dic[t.Id];
            if (ps == null) return false;
            return ps.Contains(c);
        }

        /// <summary>
        /// 获取保证金:含持仓保证金和已下单但未成交保证金
        /// </summary>
        /// <param name="t"></param>
        /// <param name="m"></param>
        /// <returns></returns>
        
        public static decimal GetMaintain(this Trader t,Market m)
        {
            decimal pm = 0;
            if (!dic.ContainsKey(t.Id)) pm = 0;
            else
            {
                if (dic[t.Id] == null) pm = 0;
                else
                pm= dic[t.Id].GetTotalMaintain(m);
            }
            decimal om = 0;
            var ol = t.Orders().GetLives();
            foreach (var v in ol)
            {
                if (v == null) continue;
                
                om += v.GetWaitingMaintain(m);
            }
            return om + pm;
        }
        public static decimal GetWaitingMaintainDispiteState(this Order o, Market m)
        {
            if (o == null) return 0;
            if (!(o.Direction == TradeDirectType.卖 && o.OrderType == OrderType.开仓)) return 0;
            var mp = o.Contract.GetMaintainForContract(m.GetNewestPrice(o.Contract.Name));// o.Contract.Coin.GetPrice());
            return mp;
        }
        public static decimal GetWaitingMaintain(this Order o, Market m)
        {
            if (o == null) return 0;
            if (!(o.Direction == TradeDirectType.卖 && o.OrderType == OrderType.开仓)) return 0;
            if (o.State != OrderState.等待中 && o.State != OrderState.部分成交) return 0;
            return o.GetWaitingMaintainDispiteState(m);
        }
        /// <summary>
        /// 获取用户保证金账户快照
        /// </summary>
        /// <param name="t"></param>
        /// <param name="m"></param>
        /// <returns></returns>
        public static MyBailAccount SnapshotBail(this Trader t, Market m)
        {
            var total = t.Account.BailAccount.Sum;
            var maintain = t.GetMaintain(m);
            var raio = maintain == 0 ? -1 : t.Account.BailAccount.Sum / maintain;
            var usable = total - maintain * SysPrm.Instance.MonitorParams.NormalMaintainRatio;// 1.2m;
            MyBailAccount mba = new MyBailAccount(total, maintain, usable, raio,t.Account.BailAccount.Frozen);
            return mba;
        }

        public static MyBailAccount SnapshotBail(this Trader t)
        {
            return SnapshotBail(t, t.GetMarket());
        }
        /// <summary>
        /// 保证金账户金额是否大于要付的金额和手续费,每次付钱都要付手续费
        /// </summary>
        /// <param name="ba"></param>
        /// <param name="delta"></param>
        /// <returns></returns>
        public static bool CouldPay(this Trader t, decimal delta,Market m)
        {
            var maintain = GetMaintain(t, m);
            var usable = t.Account.BailAccount.Sum - maintain * SysPrm.Instance.MonitorParams.NormalMaintainRatio;//;
            var r = usable >= delta * (1 + TraderService.CommissionRatio);
            if (!r)
            {
                TraderService.RaiseOnPayWhenInsufficent(t, delta);
            }
            return r;
        }
         /// <summary>
         /// 如果是平仓,直接支付;否则检查保证金是否足够
         /// </summary>
         /// <param name="t"></param>
         /// <param name="delta"></param>
         /// <param name="m"></param>
         /// <param name="o"></param>
         /// <returns></returns>
        public static bool BailPay(this Trader t, decimal delta, Market m, Order o,AccountChangeType type)
        {
            lock (t.Account.BailAccount)
            {
                SystemAccount.Instance.Log.Info(string.Format("支付(可能借款):{0}-线程{1}-委托{2}", DateTime.Now.ToString("HH:mm:ss.fff"),
                    Thread.CurrentThread.ManagedThreadId,o==null?"":o.Id.ToString()));
                if (o != null && o.OrderType == OrderType.平仓)
                {
                    //系统借款记录处理
                    if (t.Account.BailAccount.Sum < delta)
                    {
                        var needed = delta* (TraderService.CommissionRatio + 1)-t.Account.BailAccount.Sum;
                        var borrowResult = SystemAccount.Instance.Borrow(needed, o);
                    }
                    var r = t.Account.BailAccount.Sub(delta);
                    TraderService.OperateAccount(t, delta, type, "system", o, t.Account.BailAccount.Total);
                    return r;
                }
                else
                {
                    var maintain = GetMaintain(t, m);
                    var remain = t.Account.BailAccount.Sum - maintain;
                    if (t.Account.BailAccount.Sum < delta || remain < delta)
                    {
                        //执行保证金自动转入
                        var d1 = delta - t.Account.BailAccount.Sum;
                        var d2 = delta - remain;
                        var d = d1 >= d2 ? d1 : d2;
                        var tr = TraderService.OperateAddBailFromCache(t, d, o);
                        if (!tr)
                            return false;
                    }
                    var r = t.Account.BailAccount.Sub(delta);//划转保证金
                    TraderService.OperateAccount(t, delta, type, "system", o, t.Account.BailAccount.Total);
                    return r;
                }
            }
        }

        /// <summary>
        /// 计算账户保证率
        /// </summary>
        /// <param name="t"></param>
        /// <returns></returns>
        public static decimal GetMaintainRatio(this Trader t,Market m)
        {
            var maintainBail = t.GetMaintain(m);//获得维持保证金
            if (maintainBail <= 0) return int.MaxValue;
            var r =Math.Round(t.Account.BailAccount.Sum / maintainBail,3);
            return r;
        }
        public static decimal GetMaintainRatio(this Trader t)
        {
            return t.GetMaintainRatio(t.GetMarket());
        }
        /// <summary>
        /// 计算卖出后的保证金
        ///     按熔断价卖出了但没有成交,
        ///     计算如果卖出后的保证率
        /// </summary>
        /// <param name="t"></param>
        /// <param name="m"></param>
        /// <param name="releaseDelta"></param>
        /// <returns></returns>
        public static decimal GetMaintainRatioSubIt(this Trader t, Market m,decimal releaseDelta)
        {
            var maintainBail = t.GetMaintain(m);
            if (maintainBail <= 0) return int.MaxValue;
            
            var r =Math.Round((t.Account.BailAccount.Sum-releaseDelta) / maintainBail,3);
            return r;
        }
    }
}
