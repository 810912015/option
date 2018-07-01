using Com.BitsQuan.Option.Core;
using System;
namespace Com.BitsQuan.Option.Match.Imp
{
    public interface IDbModel:IFlush,IDisposable
    {
        void AddContract(Contract c);
        void AddOrder(Order o);
        void AddTrader(Trader t);
        void UpdateOrder(Order o);
        void UpdateTraderPosition(Trader t, System.Collections.Generic.List<UserPosition> ups, bool isAdd);
        void SaveDeal(Deal d);
        void UpdatePartialOrder(Order o);
    }
}
