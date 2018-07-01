
using System.Runtime.Serialization;
namespace Com.BitsQuan.Option.Core
{
    [DataContract]
    /// <summary>
    /// 期权类型:认购=1还是认沽=2
    /// </summary>
    public enum OptionType
    {
        /// <summary>
        /// 认购期权
        /// </summary>
        [EnumMember]
        认购期权 = 1,//
        /// <summary>
        /// 认沽期权
        /// </summary>
        [EnumMember]
        认沽期权 = 2//
    }
}
