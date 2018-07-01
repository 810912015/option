using Com.BitsQuan.Option.Core;
using Com.BitsQuan.Option.Core.Spot;
using Com.BitsQuan.Option.Imp;
using Com.BitsQuan.Option.Match.Imp;

namespace Com.BitsQuan.Option.Match.Spot
{
    public static class SpotOrderExtension
    {
        public static bool IsPriceValid(this OrderPolicy policy,decimal price)
        {
            if (price == 0)
            {
                if (!(policy == OrderPolicy.市价FOK || policy == OrderPolicy.市价IOC || policy == OrderPolicy.市价剩余转限价))
                    return false;
            }
            return true;
        }
        public static decimal GetTotal(this SpotOrder so)
        {
            return so.Count * so.Price;
        }
        public static bool IsArrangable(this SpotOrder so)
        {
            return so.State == OrderState.等待中 || so.State == OrderState.部分成交;
        }
        public static bool IsDone(this SpotOrder so)
        {
            return so.Count <= 0;
        }
        public static bool IsPartialDone(this SpotOrder so)
        {
            return so.Count > 0 && so.Count < so.ReportCount;
        }

        static BooleanProperty<SpotOrder> arranging = new BooleanProperty<SpotOrder>();
        public static bool IsArranging(this SpotOrder so)
        {
            return arranging.Get(so);
        }
        public static void SetArranging(this SpotOrder so,bool isArranging)
        {
            arranging.Set(so, isArranging);
        }
    }

    public class SoFreeze
    {
        /// <summary>
        /// 冻结单价
        /// </summary>
        public decimal Price { get;private  set; }
        /// <summary>
        /// 数量
        /// </summary>
        public decimal Count { get;private  set; }
        /// <summary>
        /// 是币还是钱
        /// </summary>
        public bool IsCoin { get; private set; }
        /// <summary>
        /// 冻结还是解冻
        /// </summary>
        public bool IsFreeze { get; private set; }

        public SoFreeze(bool isCoin, bool isFreeze, decimal count, decimal price)
        {
            this.IsCoin = isCoin; this.IsFreeze = isFreeze;
            this.Count = count; this.Price = price;
        }

        AccountChangeType ChangeType
        {
            get
            {
                if (IsCoin)
                {
                    if (IsFreeze)
                    {
                        return AccountChangeType.BTC冻结;
                    }
                    else return AccountChangeType.BTC解冻;
                }
                else
                {
                    if (IsFreeze) return AccountChangeType.现金冻结;
                    else return AccountChangeType.现金解冻;
                }
            }
        }

        decimal Ratio
        {
            get
            {
                if (IsCoin) return 1m; else return Price;
            }
        }

        public void Execute(SpotOrder o)
        {
            
            TraderService.OperateAccount(o.Trader, Count*Ratio, ChangeType, o.Trader.Name+o.Id+"-"+o.Price, null);
        }
    }

    public static class SpotOrderFreezeExtension
    {
        static MyProperty<int, SoFreeze> sof = new MyProperty<int, SoFreeze>();

        public static SoFreeze Freeze(this SpotOrder o)
        {
            SoFreeze f;
            if (o.Direction == TradeDirectType.卖)
            {
                f = new SoFreeze(true, true, o.ReportCount, o.Price);
            }
            else
            {
                f = new SoFreeze(false,true,o.ReportCount,o.Price);
            }
            sof.Set(o.Id, f);
            f.Execute(o);
            return f;
        }
        static bool CouldUnfreezeAll(this SpotOrder o)
        {
            if (o.State == OrderState.已行权 || o.State == OrderState.已成交 || o.State == OrderState.已撤销) return true;
            return false;
        }
        
        static decimal GetUnfreezeCount(this SpotOrder o,SoFreeze sof)
        {
            decimal c = o.DoneCount;
            if (o.State == OrderState.已撤销)
            {
                c = o.ReportCount - o.TotalDoneCount;
            }
            return c;
        }
        public static void UnFreeze(this SpotOrder o)
        {
            lock (o.Sign)
            {
                var f = sof.Get(o.Id);
                if (f == null) return;

                var c = o.GetUnfreezeCount(f);
                if (c > 0)
                {
                    SoFreeze sf = new SoFreeze(f.IsCoin,false,c,f.Price);
                    sf.Execute(o);
                }

                if (o.CouldUnfreezeAll())
                {
                    sof.Clear(o.Id);
                }
            }
        }
    }
}
