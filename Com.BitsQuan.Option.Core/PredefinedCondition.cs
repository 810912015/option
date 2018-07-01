using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Com.BitsQuan.Option.Core
{
    public enum PredefinedConditionType
    {
        预埋单_手动发送=1,
        价格条件单=2,
        持仓条件单=3,
        时间条件单=4,
        盘口第一单=5
    }
    public enum PredefinedPriceType
    {
        指定价=1,
        对手价=2,
        排队价=3,
        最新价=4,
        涨跌熔断价=5,
        超价=6
    }
     
    public class PredefinedCondition:IEntityWithId
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int Id { get; set; }
        [ForeignKey("Trader")]
        public int TraderId { get; set; }
        public virtual Trader Trader { get; set; }
        [ForeignKey("Contract")]
        public int ContractId { get; set; }
        public virtual Contract Contract { get; set; }
        public PredefinedConditionType ConditionType { get; set; }
        public TradeDirectType Direction { get; set; }
        public OrderType OrderType { get; set; }
        public PredefinedPriceType PriceType { get; set; }
        public decimal Price { get; set; }
        public int Count { get; set; }

    }

    public class PcManual : PredefinedCondition { }
    public enum PcCompareSign
    {
        大于=1,大于等=2,小于=3,小于等于=4
    }
    public enum PcPriceValidType{
        当时有效=1,启动加载=2,自动加载=3
    }
    public class PcPrice : PredefinedCondition
    {
        public PcCompareSign PriceSign { get; set; }
        public decimal PriceThreshold { get; set; }
        public PcPriceValidType PriceValidType { get; set; }
    }
    public enum PcTimeValidType
    {
        当日有效=1
    }
    public class PcTime : PredefinedCondition
    {
        public DateTime Time { get; set; }
        public PcTimeValidType TimeValidType { get; set; }
    }
    public class PcPositionCount:PredefinedCondition
    {
        public PcCompareSign PositionCountSign { get; set; }
        public int PositionCountThreshold { get; set; }
    }
    public class PcFist : PredefinedCondition { }
}
