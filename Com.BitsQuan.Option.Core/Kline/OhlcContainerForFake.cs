using System;
using System.Collections.Generic;

namespace Com.BitsQuan.Option.Core
{
    /// <summary>
    /// 为生成测试图表数据而设置的数据容器
    /// </summary>
    public class OhlcContainerForFake
    {
        public DateTime From { get; private set; }
        public DateTime To { get; private set; }
        public int IntervalInMin { get; private set; }
        Dictionary<DateTime, Ohlc> dic;
        List<DateTime> range;
        public OhlcContainerForFake(DateTime from, DateTime to, int intervalInMin)
        {
            this.From = from; this.To = to;
            this.IntervalInMin = intervalInMin;
            dic = new Dictionary<DateTime, Ohlc>();
            range = new List<DateTime>();
            Init();
        }
        void Init()
        {
            var count = (int)To.Subtract(From).TotalMinutes / IntervalInMin + 1;
            for (int i = 0; i < count; i++)
            {
                var dt = From.AddMinutes(IntervalInMin * i);
                dic.Add(dt, new Ohlc(dt, 83d, 159d, 139d, 93d, 0d));
                range.Add(dt);
            }
        }
        public void Add(Ohlc o)
        {
            var dt = o.WhenInDt;
            for (int i = 0; i < range.Count - 1; i++)
            {
                if (dt >= range[i] && dt < range[i + 1])
                {
                    dic[range[i]].Calc(o);
                    break;
                }
            }
        }
        public List<List<double>> List
        {
            get
            {
                var r = new List<List<double>>();

                foreach (var v in dic.Values)
                {
                    r.Add(v.List);
                }
                return r;
            }
        }
    }
}
