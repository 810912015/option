
using System.Runtime.Serialization;
namespace Com.BitsQuan.Option.Core
{
    [DataContract]
    /// <summary>
    /// 交易方向:买还是卖
    /// </summary>
    public enum TradeDirectType { 
        [EnumMember]
        买=1, 
        [EnumMember]
        卖=2 }
}
