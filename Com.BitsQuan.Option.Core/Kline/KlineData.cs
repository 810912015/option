using System.Collections.Generic;

namespace Com.BitsQuan.Option.Core
{
    /// <summary>
    /// 一个合约的k线图数据
    /// </summary>
    public class KlineData : IKlineData
    {
        string code, name;
        Dictionary<OhlcType, List<Ohlc>> dic;
        public KlineData(string code, string name)
        {
            this.code = code; this.name = name;
            dic = new Dictionary<OhlcType, List<Ohlc>>();
            dic.Add(OhlcType.M1440, new List<Ohlc>());
            dic.Add(OhlcType.M15, new List<Ohlc>());
            dic.Add(OhlcType.M30, new List<Ohlc>());
            dic.Add(OhlcType.M480, new List<Ohlc>());
            dic.Add(OhlcType.M5, new List<Ohlc>());
            dic.Add(OhlcType.M60, new List<Ohlc>());

        }
        public Ohlc GetLastByType(OhlcType ot)
        {
            if (!dic.ContainsKey(ot)) return null;
            var l = dic[ot];
            if (l.Count > 0) return l[l.Count - 1];
            else return null;
        }

        public string CCode
        {
            get { return code; }
        }

        public string CName
        {
            get { return name; }
        }

        public List<List<double>> M5
        {
            get { return dic[OhlcType.M5].List(); }
        }

        public List<List<double>> M15
        {
            get { return dic[OhlcType.M15].List(); }
        }

        public List<List<double>> M30
        {
            get { return dic[OhlcType.M30].List(); }
        }

        public List<List<double>> M60
        {
            get { return dic[OhlcType.M60].List(); }
        }

        public List<List<double>> M480
        {
            get { return dic[OhlcType.M480].List(); }
        }

        public List<List<double>> M1440
        {
            get { return dic[OhlcType.M1440].List(); }
        }



        public void Add(Ohlc o)
        {
            if (dic.ContainsKey(o.OhlcType))
            {
                dic[o.OhlcType].AddAndMax100(o);
            }
            else dic.Add(o.OhlcType, new List<Ohlc> { o });
            
        }


    }

    public class KlineDataDto : IKlineData
    {

        public string CCode { get;private set; }

        public string CName { get; private set; }

        public List<List<double>> M5 { get; private set; }

        public List<List<double>> M15 { get; private set; }

        public List<List<double>> M30 { get; private set; }

        public List<List<double>> M60 { get; private set; }

        public List<List<double>> M480 { get; private set; }

        public List<List<double>> M1440 { get; private set; }

        public Ohlc GetLastByType(OhlcType ot)
        {
            return null;
        }

        public void Add(Ohlc o)
        {
             
        }


        public KlineDataDto(KlineData kd)
        {
            if (kd != null)
            {
                this.CCode = kd.CCode; this.CName = kd.CName; this.M5 = kd.M5;
                this.M15 = kd.M15; this.M30 = kd.M30; this.M60 = kd.M60;
                this.M480 = kd.M480; this.M1440 = kd.M1440;
            }
        }

        public KlineDataDto(string code, string name)
        {
            this.CCode = code; this.CName = name;
            this.M1440 = new List<List<double>>();
            this.M15 = new List<List<double>>();
            this.M30 = new List<List<double>>();
            this.M480 = new List<List<double>>();
            this.M5 = new List<List<double>>();
            this.M60 = new List<List<double>>();
        }
    }
}
