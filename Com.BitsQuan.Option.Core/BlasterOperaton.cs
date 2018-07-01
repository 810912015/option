using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Com.BitsQuan.Option.Core
{
    /// <summary>
    /// 用户账户监视消息:如保证率达不到要求等
    /// </summary>
    public class TraderMsg : IEntityWithId
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int Id { get; set; }
        public string Name { get; set; }
        public string Msg { get; set; }
        public DateTime When { get; set; }
        public string MsgType { get; set; }
    }
    /// <summary>
    /// 强制平仓时的下单记录
    /// </summary>
    public class BlasterOperaton:IEntityWithId
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int Id { get; set; } 
        public int PositionId { get; set; }
        [ForeignKey("Order")]
        public int OpOrderId { get; set; }
        public virtual Order Order { get; set; }
        public bool Result { get; set; }
         /// <summary>
         /// 爆仓记录编号
         /// </summary>
        public int BlasterRecordId { get; set; }

        public string ToUserString()
        {
            return string.Format("系统自动平仓记录:平仓编号:{0},委托:{1}", BlasterRecordId, Order.ToShortString());
        }
    }
    public enum BlastType{
        开始强平权利仓=1,开始强平义务仓=2,
        强平权利仓结束 = 3, 强平义务仓结束 = 4
    }
    /// <summary>
    /// 强制平仓记录
    /// </summary>
    public class BlastRecord:IEntityWithId
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int Id { get; set; }
        [ForeignKey("Trader")]
        public int TraderId { get; set; }
        public virtual Trader Trader { get; set; }
        public decimal BailTotal { get; set; }
        public decimal NeededBail { get; set; }
        public BlastType BlastType { get; set; }
        public DateTime StartTime { get; set; }
        public string ToUserString()
        {
            return string.Format("系统自动平仓:{0},{1},需要的保证金:{2},保证金数额:{3}", StartTime, BlastType, NeededBail, BailTotal);
        }
    }
    public enum FuseType
    {
        上涨熔断=1,下跌熔断=2
    }
    /// <summary>
    /// 熔断记录
    /// </summary>
    public class FuseRecord : IEntityWithId
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int Id { get; set; }
        public int ContractId { get; set; }
        public virtual Contract Contract { get; set; }
        public DateTime StartTime { get; set; }
        public FuseType FuseType { get; set; }
        public decimal Price { get; set; }
        public decimal MaxPrice { get; set; }
        public decimal MinPrice { get; set; }
    }
}
