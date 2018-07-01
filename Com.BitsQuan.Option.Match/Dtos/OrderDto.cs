using Com.BitsQuan.Option.Core;
using Com.BitsQuan.Option.Core.Spot;
using Com.BitsQuan.Option.Imp;
using Com.BitsQuan.Option.Match;
using Com.BitsQuan.Option.Match.Imp;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Com.BitsQuan.Option.Match.Dto
{
    /// <summary>
    /// 委托
    /// </summary>
    [DataContract]
    [KnownType(typeof(ContractDto))]
    public class OrderDto
    {
        /// <summary>
        /// 编号
        /// </summary>
        [DataMember]
        public int Id { get; set; }
        /// <summary>
        /// 合约
        /// </summary>
        [DataMember]
        public ContractDto Contract { get; set; }
        /// <summary>
        /// 份数
        /// </summary>
        [DataMember]
        public int Count { get; set; }
        /// <summary>
        /// 价格
        /// </summary>
        [DataMember]
        public decimal Price { get; set; }
        /// <summary>
        /// 买卖
        /// </summary>
        [DataMember]
        public string Dir { get; set; }
        /// <summary>
        /// 成交份数
        /// </summary>
        [DataMember]
        public int DoneCount { get; set; }
        /// <summary>
        /// 当前状态
        /// </summary>
        [DataMember]
        public string State { get; set; }
        /// <summary>
        /// 持仓类型
        /// </summary>
        [DataMember]
        public string PositionType { get; set; }
        /// <summary>
        /// 挂单状态
        /// </summary>
        [DataMember]
        public string RequestState { get; set; }
        /// <summary>
        /// 委托金额
        /// </summary>
        [DataMember]
        public decimal RequestTotal { get; set; }
        /// <summary>
        /// 总成交金额
        /// </summary>
        [DataMember]
        public decimal DealTotal { get; set; }
        /// <summary>
        /// 未成交金额
        /// </summary>
        [DataMember]
        public int UndealCount { get; set; }
        /// <summary>
        /// 详细信息
        /// </summary>
        [DataMember]
        public string Detail { get; set; }
        /// <summary>
        /// 策略
        /// </summary>
        [DataMember]
        public string Policy { get; set; }
        /// <summary>
        /// 委托时间
        /// </summary>
        [DataMember]
        public string Time { get; set; }
        /// <summary>
        /// 成交均价
        /// </summary>
        [DataMember]
        public decimal DonePrice { get; set; } 

        public OrderDto(Order o)
        {
            if (o == null) return;
            Id = o.Id;
            Contract = new ContractDto(o.Contract);
            Count = o.ReportCount;
            UndealCount = o.ReportCount - o.TotalDoneCount;
            Price =Math.Round(o.Price,2);
            Dir = o.Direction.ToString() + o.OrderType.ToString().Substring(0, 1);
            DoneCount = o.TotalDoneCount;
            DonePrice =o.TotalDoneCount==0?0: Math.Round(o.TotalDoneSum / o.TotalDoneCount);

            State = o.State.ToString();
            PositionType = o.PositionType.ToString();
            RequestState = o.RequestStatus.ToString();
            RequestTotal =Math.Round( o.Price * o.ReportCount,2);
            DealTotal =Math.Round( o.TotalDoneSum,2);
            
            Detail =string.IsNullOrEmpty(o.Detail)?o.IsBySystem?"爆仓单": State:o.Detail;
            Policy = o.OrderPolicy.ToString();
            Time = o.OrderTime.ToString("yyyy-MM-dd HH:mm:ss"); 
        }

    }
    [DataContract]
    public class MyInfo
    {
        [DataMember]
        public List<OrderDto> Orders { get; set; }
        [DataMember]
        public List<PositionSummaryDto> Positions { get; set; }

        [DataMember]
        public decimal Bail { get; set; }

        public MyInfo(decimal bail, List<OrderDto> orders, List<PositionSummaryDto> positions)
        {
            this.Bail = bail; this.Orders = orders; this.Positions = positions;
        }
        public MyInfo() { Orders = new List<OrderDto>(); Positions = new List<PositionSummaryDto>(); }
    }


    [DataContract]
    public class SpotOrderDto
    {
        [DataMember]
        public int Id { get; set; }
        [DataMember]
        public string Coin { get; set; }
        /// <summary>
        /// 买还是卖
        /// </summary>
        [DataMember]
        public string Direction { get; set; }
        /// <summary>
        /// 报单量
        /// </summary>
        [DataMember]
        public decimal ReportCount { get; set; }
        /// <summary>
        /// 单价
        /// </summary>
        [DataMember]
        public decimal Price { get; set; }
        /// <summary>
        /// 数量:当部分成交时为未成交数量,其他为报单数量
        /// </summary>
        [DataMember]
        public decimal Count { get; set; }
        /// <summary>
        /// 最后一次成交数量
        /// </summary>
        [DataMember]
        public decimal DoneCount { get; set; }
        [DataMember]
        public decimal TotalDoneCount { get; set; }
        /// <summary>
        /// 下单时间
        /// </summary>
        [DataMember]
        public string OrderTime { get; set; }

        /// <summary>
        /// 委托当前状态
        /// </summary>
        [DataMember]
        public string State { get; set; }
        /// <summary>
        /// 挂单状态
        /// </summary>
        [DataMember]
        public string RequestStatus { get; set; }
        /// <summary>
        /// 最后成交价
        /// </summary>
        [DataMember]
        public decimal DonePrice { get; set; }
        [DataMember]
        public decimal Undeal { get; set; }


        /// <summary>
        /// 累计成交金额
        /// </summary>
        /// <returns></returns>
        [DataMember]
        public decimal TotalDoneSum { get; set; }
        public string Detail { get; set; }

        public SpotOrderDto(SpotOrder so)
        {
            if (so != null)
            {
                this.Id = so.Id; this.Price = so.Price; this.Coin = so.Coin.Name; this.Count = so.Count; //this.Detail = so.Detail;
                this.Direction = so.Direction.ToString(); this.DoneCount = so.DoneCount; this.DonePrice = so.DonePrice; this.TotalDoneCount = so.TotalDoneCount;
                this.TotalDoneSum = so.TotalDoneSum; this.OrderTime = so.OrderTime.ToString("yyyy-MM-dd HH:mm:ss"); this.ReportCount = so.ReportCount; 
                this.RequestStatus = so.RequestStatus.ToString();
                this.State = so.State.ToString();
                this.Undeal = so.ReportCount - so.TotalDoneCount;
            }
        }
    }
}