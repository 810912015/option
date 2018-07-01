using Com.BitsQuan.Option.Core;

namespace Com.BitsQuan.Option.Match
{
    /// <summary>
    /// 一个合约的熔断数据
    /// </summary>
    public interface IContractFuse
    {
        /// <summary>
        /// 是否正在熔断中
        /// </summary>
        bool IsFusing { get; }
        FuseType FuseType { get; }
        /// <summary>
        /// 距离熔断结束时间点的秒数
        /// </summary>
        int RemainInSeconds { get; }
        /// <summary>
        /// 当前上涨熔断价
        /// </summary>
        decimal? MaxPrice { get; }
        /// <summary>
        /// 当前下跌熔断价
        /// </summary>
        decimal? MinPrice { get; }
        /// <summary>
        /// 合约
        /// </summary>
        Contract Contract { get; }
        /// <summary>
        /// 根据熔断确定是否允许委托进行交易
        /// </summary>
        /// <param name="o"></param>
        /// <returns></returns>
        bool ShouldAccept(Order o);
        /// <summary>
        /// 是否允许交易进行:任何时候不能超出价格范围
        ///     返回:0 表示在范围内
        ///          1 表示超出上涨范围
        ///          2 表示超出下跌范围
        /// </summary>
        /// <param name="price"></param>
        /// <returns></returns>
        int ShouldAllowDeal(decimal price);
        /// <summary>
        /// 处理交易以得到熔断区间
        /// </summary>
        /// <param name="d"></param>
        void Handle(Deal d);
    }
}
