using System;

namespace Com.BitsQuan.Option.Core
{
    public interface IOrder
    {
        int Id { get; }
        Trader Trader { get; }
        string Sign { get; }
        TradeDirectType Direction { get; }
        decimal Price { get; }
        decimal OrderCount { get; }
        DateTime OrderTime { get; set; }
    }
}
