using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Com.BitsQuan.Option.Core
{
    public interface IDeal
    {
        int WhatById { get;  }
        DateTime When { get;  }
        decimal DealCount { get;  }
        decimal Price { get;  }
    }
    /// <summary>
    /// 一次成交
    /// </summary>
    public class Deal : IEntityWithId,IDeal
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int Id { get; set; }
        /// <summary>
        /// 合约编号
        /// </summary>
        public int ContractId { get; set; }
        public int WhatById { get { return ContractId; } }
        /// <summary>
        /// 主动委托
        /// </summary>
        public int MainOrderId { get; set; }
        public string MainName { get; set; }
        /// <summary>
        /// 被动委托
        /// </summary>
        public int SlaveOrderId { get; set; }
        public string SlaveName { get; set; }
        /// <summary>
        /// 成交时间
        /// </summary>
        public DateTime When { get; set; }
        /// <summary>
        /// 是否是部分成交
        /// </summary>
        public bool IsPartialDeal { get; set; }
        /// <summary>
        /// 成交数量
        /// </summary>
        public int Count { get; set; }
        public decimal DealCount { get { return Count; } }
        /// <summary>
        /// 成交类型
        /// </summary>
        public DealType DealType { get; set; }
        /// <summary>
        /// 成交价格
        /// </summary>
        public decimal Price { get; set; }
    }
}
