using Com.BitsQuan.Option.Core;
using Com.BitsQuan.Option.Imp;
using Com.BitsQuan.Option.Match.Dto;
using System.Collections.Generic;
using System.ServiceModel;

namespace Com.BitsQuan.Service
{  
    
    [ServiceContract]
    public interface ITradeCallBack
    {
        [OperationContract(IsOneWay = true)]
        void OnLogin(int code, string desc);
        [OperationContract(IsOneWay=true)]
        void OnMsg(string name, string msg, string catagory);
        [OperationContract(IsOneWay = true)]
        void OnPosition(List<PositionSummaryDto> pos,OperationResult result);
        [OperationContract(IsOneWay = true)]
        void OnOrder(OrderDto order, OperationResult result); 
       
        [OperationContract(IsOneWay = true)]
        void ReceiveContract(List<ContractDto> contracts);
     
        [OperationContract(IsOneWay = true)]
        void OnAccount(MyBailAccount bailAccount,Account cacheAccount);
    }
    
}
