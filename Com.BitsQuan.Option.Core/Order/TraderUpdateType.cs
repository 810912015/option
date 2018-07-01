using System.Runtime.Serialization;
namespace Com.BitsQuan.Option.Core
{ 
    [DataContract]
    /// <summary>
    /// 用户更新类型
    /// </summary>
    public enum TraderUpdateType
    {
        [EnumMember]
        充值 = 8,
        [EnumMember]
        提现 = 1,
        [EnumMember]
        设置保证金自动转入 = 3,
        [EnumMember]
        设置自动买平 = 4,
        [EnumMember]
        设置冻结用户 = 5
    }
}
