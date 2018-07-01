using Com.BitsQuan.Option.Imp;
using System.Collections.Generic;
using System.ServiceModel;

namespace Com.BitsQuan.Service
{
    [ServiceContract]
    public interface IMarketCallBack
    {
        [OperationContract(IsOneWay = true)]
        void OnLogin(int code, string desc);
        [OperationContract(IsOneWay = true)]
        void OnSubscribe(List<int> contractIds, bool IsSubscribe, OperationResult result);
        [OperationContract(IsOneWay = true)]
        void OnMarket(MarketOrder marketOrder);
    }
}
