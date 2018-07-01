using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Com.BitsQuan.Option.Core
{ 
    /// <summary>
    /// 账户操作记录
    /// </summary>
    public class AccountTradeRecord:IEntityWithId
    {
        [Key] 
        public int Id { get; set; }
        
       
        /// <summary>
        /// 存入还是取出
        /// </summary>
        public bool IsAddTo { get; set; }
        /// <summary>
        /// 存取金额
        /// </summary>
        public decimal Delta { get; set; }
        /// <summary>
        /// 操作时间
        /// </summary>
        public DateTime When { get; set; }
        /// <summary>
        /// 操作类型
        /// </summary>
        public AccountChangeType OperateType { get; set; }
        /// <summary>
        /// 操作人
        /// </summary>
        public string ByWho { get; set; }
        /// <summary>
        /// 委托描述
        /// </summary>
        public string OrderDesc { get; set; }
        /// <summary>
        /// 总金额
        /// </summary>
        public decimal Current { get; set; }
        /// <summary>
        /// 冻结金额
        /// </summary>

        public decimal Frozen { get; set; }

        public bool IsBail { get; set; }
        [ForeignKey("CoinId")]
        public virtual Coin Coin { get; set; }
        public int CoinId { get; set; }
        [ForeignKey("Who")]
        public virtual int WhoId { get; set; }
        public virtual Trader Who { get; set; }

        public decimal BailSum { get; set; }
        public decimal BailFrozen { get; set; }
        public decimal CnySum { get; set; }
        public decimal CnyFrozen { get; set; }
        public decimal BtcSum { get; set; }
        public decimal BtcFrozen { get; set; }

    }
}