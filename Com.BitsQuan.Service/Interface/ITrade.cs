using Com.BitsQuan.Option.Core;
using System.Collections.Generic;
using System.ServiceModel;

namespace Com.BitsQuan.Service
{

    [ServiceContract(SessionMode = SessionMode.Required, CallbackContract = typeof(ITradeCallBack))]
    public interface ITrade
    {
        [OperationContract(IsOneWay = true)]
        void Login(string name, string pwd);
        [OperationContract(IsOneWay = true)]
        void QueryContract();
        [OperationContract(IsOneWay = true)]
        void QueryOrder(int orderId);
        [OperationContract(IsOneWay = true)]
        void QueryPosition(List<string> contractCodes);

        [OperationContract(IsOneWay = true)]
        void AddOrder(int contract, TradeDirectType dir, OrderType orderType,
            OrderPolicy policy, decimal count, decimal price,
            string userOpId = "");

        [OperationContract(IsOneWay = true)]
        void RedoOrder(int orderId);
    }
}
