using Com.BitsQuan.Option.Core;
using Com.BitsQuan.Option.Imp;
using Com.BitsQuan.Option.Match.Dto;
using Com.BitsQuan.Option.Match.Imp;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.ServiceModel;
namespace Com.BitsQuan.Option.Match
{
    public interface IMatchService:IDisposable
    {
       OperationResult AddContract(string coinName, DateTime exeTime, decimal exePrice, OptionType optionType, string target, decimal coinCount = 1);
        OrderResult AddOrder(int who, int contract, TradeDirectType dir, OrderType orderType, OrderPolicy policy, int count, decimal price, string userOpId = "");
        OperationResult CreateTrader(string name, bool IsAutoAddBailFromCache, bool IsAutoSellRight);
        DeepDataPool DeepPool { get; }
        OperationResult DisableContract(int contractId);
         
        OrderExecutor Executor { get; }
        void Flush();
        IKlineData GetKlineDataByContractCode(string code);
        List<double> GetLatestKline(string code, OhlcType type);
        Market MarketBoard { get; }
        OrderMatcher Matcher { get; }
        IOptionModel Model { get; }
        OrderMonitor Monitor { get; }
        event Action<Order> OnAfterMatch;
        event Action<string, PositionSummary> OnPositionSummaryChanged;
        event Action<string, string> OnUserMsge;
        OperationResult RedoOrder(int who, int orderId);
        void Start();
        OperationResult UpdateTrader(int traderId, TraderUpdateType type, object value);
    }
    [DataContract]
    [KnownType(typeof(OrderDto))]
    [KnownType(typeof(OperationResult))]
    public class OpOrderDtoResult : OperationResult
    {
        [DataMember]public string UserOpId { get; set; }
        [DataMember]
        public OrderDto Order { get; set; }

    }
    [DataContract]
    [KnownType(typeof(SpotOrderDto))]
    [KnownType(typeof(OperationResult))]
    public class OpSpotDtoResult : OperationResult
    {
        [DataMember]
        public string UserOpId { get; set; }
        [DataMember]
        public SpotOrderDto Order { get; set; }

    }
    
    [DataContract]
    [KnownType(typeof(ContractDto))]  
    [KnownType(typeof(OpType))]
    [KnownType(typeof(TradeTargetType))]
    public class Contract2
    {
        [DataMember]
        public ContractDto Contract { get; set; }
        [DataMember]
        public OpType Operation { get; set; }
        [DataMember]
        public TradeTargetType Type { get; set; }
    }
    [DataContract]
    public enum OrderDoneType
    {
        [EnumMember]
        部分成交 = 1,
        [EnumMember]
        已成交 = 2
    }
    [DataContract]
    public enum OpType
    {
        [EnumMember]
        Added = 1,
        [EnumMember]
        Deleted = 2,
        [EnumMember]
        Updated = 3
    }
    [DataContract]
    public enum TradeTargetType
    {
        [EnumMember]
        BailOption = 1,
        [EnumMember]
        Spot = 2,
        [EnumMember]
        CoveredOption = 3,
        [EnumMember]
        Future = 4
    }
}
