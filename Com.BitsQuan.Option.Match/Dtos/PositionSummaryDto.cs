using Com.BitsQuan.Option.Core;
using System;
using System.Runtime.Serialization;

namespace Com.BitsQuan.Option.Match.Dto
{
    [DataContract]
    [KnownType(typeof(ContractDto))]
    [KnownType(typeof(OrderType))]
    public class PositionSummaryDto
    {
        [DataMember]
        public ContractDto Contract { get; private set; }

        /// 持仓类型
        /// </summary>
        [DataMember]
        public string PositionType { get; private set; }
        /// <summary>
        /// 持仓
        /// </summary>
        [DataMember]
        public int Count { get; private set; }
        /// <summary>
        /// 可平
        /// </summary>
        [DataMember]
        public int ClosableCount { get; private set; }
        /// <summary>
        /// 买入成本价
        /// </summary>
        [DataMember]
        public decimal BuyPrice { get; private set; }
        /// <summary>
        /// 买入成本
        /// </summary>
        [DataMember]
        public decimal BuyTotal { get; private set; }
        /// <summary>
        /// 浮动盈亏
        /// </summary>
        [DataMember]
        public decimal FloatProfit { get; private set; }
        /// <summary>
        /// 平仓盈亏
        /// </summary>
        [DataMember]
        public decimal CloseProfit { get; private set; }
        /// <summary>
        /// 维持保证金
        /// </summary>
        [DataMember]
        public decimal Maintain { get; private set; }
        /// <summary>
        /// 合约市值
        /// </summary>
        [DataMember]
        public decimal TotalValue { get; private set; }
        [DataMember]
        public OrderType OrderType { get; private set; }
        public decimal Commission { get; private set; }

        [DataMember]
        public string Id { get; private set; }

        public PositionSummaryDto(PositionSummary ps, decimal curPrice, int closableCount)
        {

            this.Id = ps.Id;
            this.OrderType = ps.OrderType;

            this.PositionType = ps.PositionType;


            this.BuyPrice = Math.Round(ps.BuyPrice, 2);
            this.BuyTotal = Math.Round(ps.BuyTotal, 2);

            this.Contract = new ContractDto(ps.Contract);

            this.Count = ps.Count;

            this.ClosableCount = closableCount > 0 ? closableCount : 0;
            this.TotalValue = Math.Round(ps.TotalValue, 2);
            this.FloatProfit = Math.Round(ps.FloatProfit, 2);
            this.CloseProfit = Math.Round(ps.CloseProfit, 2);
            this.Maintain = Math.Round(ps.GetMaintain(curPrice), 2);
            this.Commission = 0;
        }
    }
}
