
using System.Runtime.Serialization;
namespace Com.BitsQuan.Option.Core
{
    [DataContract]
    /// <summary>
    /// 下单类型:开仓还是平仓
    /// </summary>
    public enum OrderType
    {
        [EnumMember]
        开仓 = 2,//开仓
        [EnumMember]
        平仓 = 1//平仓
    }
}
