using System.Collections.Generic;

namespace Com.BitsQuan.Option.Core
{
    public interface IKlineData
    {
        string CCode { get; }
        string CName { get; }
        List<List<double>> M5 { get; }
        List<List<double>> M15 { get; }
        List<List<double>> M30 { get; }
        List<List<double>> M60 { get; }
        List<List<double>> M480 { get; }
        List<List<double>> M1440 { get; }
        Ohlc GetLastByType(OhlcType ot);
        void Add(Ohlc o);
    }
}
