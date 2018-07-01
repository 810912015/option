using Com.BitsQuan.Option.Core;
using System;
namespace Com.BitsQuan.Option.Match.Spot
{
    public interface ISpotService
    {
        SpotOrderResult AddOrder(int trader, int coinId, Com.BitsQuan.Option.Core.TradeDirectType dir,
            Com.BitsQuan.Option.Core.OrderPolicy policy, decimal count, decimal price);
        SpotOrderContainer Container { get; }
        Com.BitsQuan.Option.Match.DeepDataPool3 DeepPool { get; }
        Com.BitsQuan.Option.Core.IKlineData GetKlineDataByCoinId(int coinId);
        System.Collections.Generic.List<double> GetLatestKline(Com.BitsQuan.Option.Core.OhlcType type = OhlcType.M5);
        Com.BitsQuan.Option.Core.KlineDataPool KlinePool { get; }
        SpotMarket Market { get; }
        SpotMatch match { get; }
        SpotModel Model { get; }
        bool Redo(int soId);
    }
}
