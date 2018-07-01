using Newtonsoft.Json;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Threading;

namespace Com.BitsQuan.Option.Core
{
    /// <summary>
    /// 委托:交易请求
    /// </summary>
    public class Order : IEntityWithId, IOrder
    {
        [Key]
        public int FakeId { get; set; }
        //[Key]
        //[DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int Id { get; set; }
        [ForeignKey("Trader")]
        public int TraderId { get; set; }
        [JsonIgnore]

        public virtual Trader Trader { get; set; }
        [ForeignKey("Contract")]
        public int ContractId { get; set; }
        /// <summary>
        /// 合约
        /// </summary>
        public virtual Contract Contract { get; set; }
        public string Sign { get { return Contract.Name; } }
        TradeDirectType tt;
        /// <summary>
        /// 买还是卖
        /// </summary>
        public TradeDirectType Direction
        {
            get
            {
                return tt;
            }
            set
            {
                if (value != tt)
                {
                    tt = value;
                }
            }
        }
        /// <summary>
        /// 报单量
        /// </summary>
        public int ReportCount { get; set; }
        /// <summary>
        /// 单价
        /// </summary>
        public decimal Price { get; set; }
        /// <summary>
        /// 数量:当部分成交时为未成交数量,其他为报单数量
        /// </summary>
        public int Count { get; set; }
        public decimal OrderCount { get { return Count; } }
        /// <summary>
        /// 最后一次成交数量
        /// </summary>
        public int DoneCount { get; set; }
        public int TotalDoneCount { get; set; }
        /// <summary>
        /// 下单时间
        /// </summary>
        public DateTime OrderTime { get; set; }

        /// <summary>
        /// 委托当前状态
        /// </summary>
        public OrderState State { get; set; }
        /// <summary>
        /// 开仓还是平仓
        /// </summary>
        public OrderType OrderType { get; set; }
        /// <summary>
        /// 交易策略
        /// </summary>
        public OrderPolicy OrderPolicy { get; set; }
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
        public PositionType PositionType
        {
            get
            {
                if (this.Direction == TradeDirectType.买)
                {
                    if (this.OrderType == OrderType.开仓)
                    {
                        return Core.PositionType.权利仓;
                    }
                    else
                    {
                        return Core.PositionType.义务仓;
                    }
                }
                else
                {
                    if (this.OrderType == OrderType.开仓)
                    {
                        return Core.PositionType.义务仓;
                    }
                    else
                    {
                        return Core.PositionType.权利仓;
                    }
                }
            }
        }

        /// <summary>
        /// 累计成交金额
        /// </summary>
        /// <returns></returns>
        public decimal TotalDoneSum { get; set; }
        public string Detail { get; set; }
        public int GetQueyeType()
        {
            if ((Direction == TradeDirectType.卖))
                return 1;
            else return 2;
        }
        
        public override string ToString()
        {
            return string.Format("委托--编号:{0},{1},方向:{2},价格:{3},委托数量:{4},成交数量:{5},委托时间:{6},当前状态:{7},开平:{8},{9},{10}",
                Id, Contract, Direction,
                Price, Count, DoneCount, OrderTime, State,
                OrderType, Trader.Name, DonePrice);
        }

        public string ToShortString()
        {
            var r = string.Format("委托--{0}-{1},{2}-{3}-{4}{5}-价{6}-量{7}-交{8}-{9}-成价{10}-系统{11}-{12}", Id, Contract.Code, Contract.Name, State,
                Direction, OrderType.ToString().Substring(0, 1), Price, Count, DoneCount, OrderTime, DonePrice, IsBySystem, Trader.Name
                );
            return r;
        }
    }

}
