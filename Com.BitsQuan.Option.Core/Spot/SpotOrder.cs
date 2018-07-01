using Newtonsoft.Json;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Com.BitsQuan.Option.Core.Spot
{
    public class SpotOrder : IEntityWithId,IOrder
    {

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int Id { get; set; }
        [ForeignKey("Trader")]
        public int TraderId { get; set; }
        [JsonIgnore]

        public virtual Trader Trader { get; set; }
        [ForeignKey("Coin")]
        public int CoinId { get; set; }
        public virtual Coin Coin { get; set; }
        public string Sign { get { return Coin.Name; } }
        /// <summary>
        /// 买还是卖
        /// </summary>
        public TradeDirectType Direction { get; set; }
        /// <summary>
        /// 报单量
        /// </summary>
        public decimal ReportCount { get; set; }
        /// <summary>
        /// 单价
        /// </summary>
        public decimal Price { get; set; }
        /// <summary>
        /// 数量:当部分成交时为未成交数量,其他为报单数量
        /// </summary>
        public decimal Count { get; set; }
        public decimal OrderCount { get { return ReportCount; } }
        /// <summary>
        /// 最后一次成交数量
        /// </summary>
        public decimal DoneCount { get; set; }
        public decimal TotalDoneCount { get; set; }
        /// <summary>
        /// 下单时间
        /// </summary>
        public DateTime OrderTime { get; set; }

        /// <summary>
        /// 委托当前状态
        /// </summary>
        public OrderState State { get; set; }
        /// <summary>
        /// 挂单状态
        /// </summary>
        public OrderRequestStatus RequestStatus { get; set; }
        /// <summary>
        /// 最后成交价
        /// </summary>
        public decimal DonePrice { get; set; }
        /// <summary>
        /// 是否是系统产生的委托:用于爆仓
        /// </summary>
        public bool IsBySystem { get; set; }


        /// <summary>
        /// 累计成交金额
        /// </summary>
        /// <returns></returns>
        public decimal TotalDoneSum { get; set; }
        public string Detail { get; set; }

        public OrderPolicy OrderPolicy { get; set; }
        public int GetQueyeType()
        {
            if ((Direction == TradeDirectType.卖))
                return 1;
            else return 2;
        }

        public override string ToString()
        {
            var r = string.Format("现货委托:{0}-{1}-{2}-价{3}-量{4}-成量{5}-{6}-成价{7}-系统单{8}-人{9}",
                Id,
                State,
                 Direction,
                 Price, Count, DoneCount, OrderTime, DonePrice, IsBySystem, Trader.Name
                 );
            return r;
        }


    }

    public class SpotDeal : IEntityWithId,IDeal
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int Id { get; set; }
        [ForeignKey("Coin")]
        public int CoinId { get; set; }
        public int WhatById { get { return CoinId*(-1); } }
        public virtual Coin Coin { get; set; }
        public int MainId { get; set; }
        public int SlaveId { get; set; }
        public decimal Count { get; set; }
        public decimal DealCount { get { return Count; } }
        public decimal Price { get; set; }
        public DateTime When { get; set; }
        public string MainTraderName { get; set; }
        public string SlaveTraderId { get; set; }

        public TradeDirectType MainOrderDir { get; set; }
    }
}
