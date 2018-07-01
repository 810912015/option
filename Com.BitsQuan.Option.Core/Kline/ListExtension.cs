using System;
using System.Collections.Generic;
using System.Linq;

namespace Com.BitsQuan.Option.Core
{
    public static class ListExtension
    {
        public static List<List<double>> List(this List<Ohlc> l)
        {
            List<List<double>> r = new List<List<double>>();
            foreach (var v in l) r.Add(v.List);
            return r;
        }
        public static void AddAndMax100(this List<Ohlc> l, Ohlc o)
        {
            try
            {
                lock (l)
                {
                    var q = l.Where(a => a.WhenInDt == o.WhenInDt).FirstOrDefault();
                    if (q != null) return;

                    var i = l.Count - 100;
                    if (i > 0)
                    {
                        l.RemoveRange(0, i);
                    }
                    l.Add(o);
                }
            }
            catch (Exception ex)
            {
                Singleton<TextLog>.Instance.Error(ex, "addandMax100");
            }
        }
    }
}
