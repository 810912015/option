using System.Collections.Generic;
using System.ServiceModel;

namespace Com.BitsQuan.Service
{
    [ServiceContract(SessionMode = SessionMode.Required, CallbackContract = typeof(IMarketCallBack))]
    public interface IMarketSvc
    {
        [OperationContract(IsOneWay = true)]
        void Login(string name, string pwd);
        [OperationContract(IsOneWay = true)]
        void Subscribe(List<int> contractIds, bool IsSubscribe);

    }
}
