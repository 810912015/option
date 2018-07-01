using Com.BitsQuan.Option.Core;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
namespace Com.BitsQuan.Option.Match.Imp
{
    public interface IOptionModel : IInitialbe, IFlush,IDisposable
    {
        IEnumerable<Coin> Coins { get; }
        void AddContract(Contract c);
        void AddOrder(Order o);
        void UpdateOrder(Order o);
        void AddTrader(Trader t);
        IEnumerable<Contract> Contracts { get; }
        Contract CreateContract(string coinName, DateTime exeTime, decimal exePrice,  OptionType optionType, string target,decimal coinCount);
        Order CreateOrder(int who, int contract, TradeDirectType dir, OrderType orderType, OrderPolicy policy, int count, decimal price);
        Trader CreateTrader(string name);
        IEnumerable<Order> LegacyOrders { get; }
        IEnumerable<Trader> Traders { get; }
        void UpdateTraderPosition(Trader t, List<UserPosition> ups, bool isAdd);
       // IEnumerable<UserPosition> UserPositions { get; }
        Coin AddCoin(string name, string contractCode, decimal mainBailRatio, decimal mainBailTimes);
        IDbModel DbModel { get; }
        void RemoveContract(int c,decimal d);
        void UpdatePartialOrder(Order o);
        List<Deal> LatestDeals { get; }
    }
}
