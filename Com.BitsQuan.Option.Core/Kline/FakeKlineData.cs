using System;
using System.Collections.Generic;

namespace Com.BitsQuan.Option.Core
{
    /// <summary>
    /// 代表一个contract的测试蜡烛图数据
    /// </summary>
    public class FakeKlineData : IKlineData
    {
        public string CCode { get; private set; }
        public string CName { get; private set; }

        public DateTime From { get; private set; }
        public DateTime To { get; private set; }
        OhlcContainerForFake m5, m15, m30, m60, m480, m1440;
        public List<List<double>> M5 { get { return m5.List; } }
        public List<List<double>> M15 { get { return m15.List; } }
        public List<List<double>> M30 { get { return m30.List; } }
        public List<List<double>> M60 { get { return m60.List; } }
        public List<List<double>> M480 { get { return m480.List; } }
        public List<List<double>> M1440 { get { return m1440.List; } }

        public FakeKlineData(string code, string name, DateTime from, DateTime to)
        {
            this.CCode = code; this.CName = name; this.From = from; this.To = to;
            m5 = new OhlcContainerForFake(from, to, 5);
            m15 = new OhlcContainerForFake(from, to, 15);
            m30 = new OhlcContainerForFake(from, to, 30);
            m60 = new OhlcContainerForFake(from, to, 60);
            m480 = new OhlcContainerForFake(from, to, 480);
            m1440 = new OhlcContainerForFake(from, to, 1440);
        }
        public void Add(Ohlc o)
        {
            m5.Add(o); m15.Add(o); m30.Add(o); m60.Add(o); m480.Add(o); m1440.Add(o);
        }




        public Ohlc GetLastByType(OhlcType ot)
        {
            return null;
        }
    }
}
