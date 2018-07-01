using Com.BitsQuan.Option.Core;
using Com.BitsQuan.Option.Imp;
using Com.BitsQuan.Option.Match.Imp;
using System;
using System.Timers;

namespace Com.BitsQuan.Option.Match
{
    public class FuseBoundary
    {
        class TheoryPriceCaculator
        {
            const double E = 2.7182818;

            public static decimal CalcTheoryPrice(
                decimal btcPrice,
                decimal execPrice,
                DateTime execTime,
                ushort optionType)
            {
                var tpl = Caculate(
                    btcPrice,
                    execPrice,
                    1.5,
                    6 / 100.0,
                    (execTime - DateTime.Now).TotalDays / 365.0);
                return optionType == 1 ? tpl.Item1 : tpl.Item2;
            }
            static decimal Ajust(double d)
            {
                try
                {
                    if (double.IsNaN(d)) return 0m;
                    if (d < (double)decimal.MinValue) return decimal.MinValue;
                    if (d > (double)decimal.MaxValue) return decimal.MaxValue;
                    return (decimal)d;
                }
                catch (Exception e)
                {
                    Singleton<TextLog>.Instance.Error(e, "fuse therory");
                    return 0m;
                }
            }
            /// <summary>
            /// 
            /// </summary>
            /// <param name="s0">比特币价格</param>
            /// <param name="k">行权价</param>
            /// <param name="ro">波动率。小数表示：0-1</param>
            /// <param name="r">利率。小数表示：0-1</param>
            /// <param name="t">剩余年数，即天数/365</param>
            /// <returns>Item1是认购期权理论价格，Item2是认沽期权理论价格</returns>
            static Tuple<decimal, decimal> Caculate(decimal s0, decimal k, double ro, double r, double t)
            {
                var d1 = (Math.Log((double)(s0 / k)) + (r + ro * ro / 2) * t) / (ro * Math.Pow(t, 0.5));
                var d2 = (Math.Log((double)(s0 / k)) + (r - ro * ro / 2) * t) / (ro * Math.Pow(t, 0.5));

                var c = (double)s0 * Compute(d1) - (double)k * Compute(d2) / Math.Pow(E, r * t);
                var p = (double)k * Compute(d2 * -1) / Math.Pow(E, r * t) - (double)s0 * Compute(d1 * -1);

                return Tuple.Create(Ajust(c), Ajust(p));

            }


            static double Compute(double v)
            {
                var Z = v;
                var M = 0;
                var SD = 1;
                var Prob = 0.0;
                if (SD < 0)
                {
                    throw new Exception("The standard deviation must be nonnegative.");
                }
                else if (SD == 0)
                {
                    if (Z < M)
                    {
                        Prob = 0;
                    }
                    else
                    {
                        Prob = 1;
                    }
                }
                else
                {
                    Prob = Normalcdf((Z - M) / SD);
                    Prob = Math.Round(10000000 * Prob) / 10000000;
                }
                return Prob;
            }

            static double Normalcdf(double X)
            {   //HASTINGS.  MAX ERROR = .000001
                var T = 1 / (1 + .2316419 * Math.Abs(X));
                var D = .3989423 * Math.Exp(-X * X / 2);
                var Prob = D * T * (.3193815 + T * (-.3565638 + T * (1.781478 + T * (-1.821256 + T * 1.330274))));
                if (X > 0)
                {
                    Prob = 1 - Prob;
                }
                return Prob;
            }
        }
        public FuseExtrem Extrem { get; private set; }
        Contract c;
        public FuseBoundary(Contract c, double span)
        {
            this.c = c;
            Extrem = new FuseExtrem(span);
        }
        decimal Round(decimal d)
        {
            return Math.Round(d, 2);
        }
        decimal TheroyPrice
        {
            get
            {
                var dt = c.ExcuteTime.Subtract(DateTime.Now).TotalDays;
                var dtt = dt / 365d;
                var tp = TheoryPriceCaculator.CalcTheoryPrice(BtcPrice.Current*c.CoinCount, c.ExcutePrice*c.CoinCount, c.ExcuteTime, (ushort)c.OptionType);

                return Round(tp);
            }
        }
        public decimal CalMaxPrice()
        {

            if (Extrem.MinIn5Min < BtcPrice.Current * 0.03m) return TheroyMax;
            return Round(Extrem.MinIn5Min * 1.5m);
        }

        public decimal CalMinPrice()
        {
            if (Extrem.MinIn5Min < BtcPrice.Current * 0.03m) return TheroyMin;
            return Round(Extrem.MaxIn5Min / 2);
        }
        public decimal TheroyMax
        {
            get
            {
                var tp = TheroyPrice;
                if (tp < 50) return tp + 50;
                return tp * 1.5m;
            }
        }
        public decimal TheroyMin
        {
            get
            {
                var tp = TheroyPrice / 2;
                if (tp < 0.01m) return 0.01m;
                return tp;
            }
        }
    }
}
