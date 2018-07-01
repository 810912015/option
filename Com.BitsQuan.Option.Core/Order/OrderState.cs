
namespace Com.BitsQuan.Option.Core
{
    /// <summary>
    /// 委托当前状态:已成交,等待中,已撤销,已行权,部分成交
    /// </summary>
    public enum OrderState { 等待中=1,已撤销=2,已成交=3,
        部分成交=4,已行权=5 }
}
