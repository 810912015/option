
using System.Runtime.Serialization;
namespace Com.BitsQuan.Option.Core
{
    [DataContract]
    /// <summary>
    /// 交易策略
    /// </summary>
    public enum OrderPolicy
    {
        [EnumMember]
        /// <summary>
        /// 限价申报:可手工撤单
        /// </summary>
        限价申报 = 1,
        /// <summary>
        /// 市价IOC：按对手方最优报价最大限度成交，不成交部分系统自动撤单。
        /// </summary>
        [EnumMember]
        市价IOC = 2,
        /// <summary>
        /// 市价剩余转限价:市价成交后剩余部分转为限价(已成交价格)
        /// </summary>
        [EnumMember]
        市价剩余转限价 = 3,
        /// <summary>
        /// 限价fok:限价1成交,不成交则撤单
        /// </summary>
        [EnumMember]
        限价FOK = 4,
        /// <summary>
        /// 市价fok:市价1成交,不成交自动撤单
        /// </summary>
        [EnumMember]
        市价FOK = 5
    }
}
