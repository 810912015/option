using Newtonsoft.Json;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Com.BitsQuan.Option.Core
{
    /// <summary>
    /// 用户当前持仓,同时是用户持仓记录
    /// </summary>
    [Table("UserPositions")]
    public class UserPosition:IEntityWithId
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int Id { get; set; }
        [ForeignKey("Order")]
        public int OrderId { get; set; }
        public virtual Order Order { get; set; }
        [ForeignKey("Trader")]
        public int? TraderId { get; set; }
        [JsonIgnore]
        public virtual Trader Trader { get; set; }
        public int Count { get; set; }

        /// <summary>
        /// 成交时间
        /// </summary>
        public DateTime DealTime { get; set; }

        public override string ToString()
        {
            return string.Format("-持仓{0}-单号{1}-人号-{2}-量{3}-{4}-合约{5}", Id, OrderId,TraderId??0,Count,DealTime,Order==null?"":Order.Contract.Code);
        }
    }

    [Table("PositionSummaryDatas")]
    public class PositionSummaryData : IEntityWithId
    {
        [Key] 
        public int Id { get; set; }
        [ForeignKey("Contract")]
        public int ContractId { get;  set; }

        /// 持仓类型
        /// </summary>
        public string PositionType { get;  set; }
        /// <summary>
        /// 持仓
        /// </summary>
        public int Count { get;  set; }
        /// <summary>
        /// 可平
        /// </summary>
        public int ClosableCount { get;  set; }
        /// <summary>
        /// 买入成本价
        /// </summary>
        public decimal BuyPrice { get;  set; }
        /// <summary>
        /// 买入成本
        /// </summary>
        public decimal BuyTotal { get;  set; }
        /// <summary>
        /// 浮动盈亏
        /// </summary>
        public decimal FloatProfit { get;  set; }
        /// <summary>
        /// 平仓盈亏
        /// </summary>
        public decimal CloseProfit { get;  set; }
        /// <summary>
        /// 维持保证金
        /// </summary>
        public decimal Maintain { get;  set; }
        /// <summary>
        /// 合约市值
        /// </summary>
        public decimal TotalValue { get;  set; }
        public OrderType OrderType { get;  set; }
        public decimal Commission { get;  set; }

        [ForeignKey("Trader")]
        public int TraderId { get; set; }
        public DateTime When { get; set; }
        public virtual Trader Trader { get; set; }
        public virtual Contract Contract { get; set; }
 
    }
}
