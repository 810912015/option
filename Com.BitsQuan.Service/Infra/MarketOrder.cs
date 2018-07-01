using Com.BitsQuan.Option.Match.Dto;
using System.Runtime.Serialization;

namespace Com.BitsQuan.Service
{ 
    [DataContract]
    [KnownType(typeof(MarketDto))]
    [KnownType(typeof(OrderDto))]
    public class MarketOrder
    {
        [DataMember]
        public MarketDto Market { get; set; }
        [DataMember]
        public OrderDto Order { get; set; }
    }
}
