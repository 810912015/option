using Com.BitsQuan.Option.Core;
using Com.BitsQuan.Option.Imp;
using Com.BitsQuan.Option.Match;
using Com.BitsQuan.Option.Match.Imp;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Threading;

namespace Com.BitsQuan.Option.Match.Dto
{
    /// <summary>
    /// 合约的市场信息
    /// </summary>
    [DataContract]
    public class MarketDto
    {
        public DateTime TimeStramp { get; set; }
        /// <summary>
        /// 最新成交价
        /// </summary>
        [DataMember]
        public decimal NewestDealPrice { get; set; }
        public bool IsAdd { get; set; }
        /// <summary>
        /// 最新价
        /// </summary>
        [DataMember]
        public decimal Newest { get; set; }
        /// <summary>
        /// 上涨熔断价
        /// </summary>
        [DataMember]
        public decimal Raise { get; set; }
        /// <summary>
        /// 下跌熔断价
        /// </summary>
        [DataMember]
        public decimal Fall { get; set; }
        /// <summary>
        /// 熔断结束秒数
        /// </summary>
        [DataMember]
        public int FuseSeconds { get; set; }
        /// <summary>
        /// 卖1价
        /// </summary>
        [DataMember]
        public decimal Sell1Price { get; set; }
        /// <summary>
        /// 买1价
        /// </summary>
        [DataMember]
        public decimal Buy1Price { get; set; }
        /// <summary>
        /// 卖1份数
        /// </summary>
        [DataMember]
        public decimal Sell1Count { get; set; }
        /// <summary>
        /// 买1份数
        /// </summary>
        [DataMember]
        public decimal Buy1Count { get; set; }
         
        /// <summary>
        /// 成交总额
        /// </summary>
        [DataMember]
        public decimal Total { get; set; }
        [DataMember]
        public string CurExe { get; set; }
        //合约最高价
        [DataMember]
        public decimal HitPrice { get; set; }
        //合约最低价
        [DataMember]
        public decimal LowPrice { get; set; }
        //合约代码
        [DataMember]
        public string Code { get; set; }
        //合约名称
        [DataMember]
        public string Name { get; set; }
        /// <summary>
        /// 持仓总数
        /// </summary>
        [DataMember]
        public decimal PositionTotal { get; set; }
        /// <summary>
        /// 总开仓
        /// </summary>
        [DataMember]
        public int OpenTotal { get; set; }
        /// <summary>
        /// 24小时开仓数
        /// </summary>
        [DataMember]
        public int Open24 { get; set; }
        /// <summary>
        /// 24小时平仓数
        /// </summary>
        [DataMember]
        public int Close24 { get; set; }
        /// <summary>
        /// 24小时净开仓数
        /// </summary>
        [DataMember]
        public int Pure24 { get; set; }
        [DataMember]
        public int Times { get; set; }

        public string MakeCe(DateTime dt)
        {
            var d=dt.Subtract(DateTime.Now);
            var r =d.TotalDays;
            if (r <= 0) return "0天";
            else
            {
                if (r >= 1) return ((int)r) + "天";
                else
                {
                    var h = d.TotalHours;
                    if (h >= 1) return ((int)h) + "小时";
                    else
                    {
                        var m = d.TotalMinutes;
                        if (m >= 1) return ((int)m) + "分钟";
                        else
                        {
                            var s = d.TotalSeconds;
                            return ((int)s) + "秒";
                        }
                    }
                }
            }
        }

        public MarketDto(MarketItem mi,Func<string,Tuple<decimal,int,decimal,int>> get1Prm)
        {
            if (mi == null) return;
            this.TimeStramp = DateTime.Now;
            this.Times = mi.Copis;
            this.NewestDealPrice = mi.NewestDealPrice;
            this.IsAdd = mi.IsAdd;
            this.Code = mi.Contract.Code;
            this.Name = mi.Contract.Name;
            this.Newest = Math.Round(mi.NewestPrice, 2);
            //this.PositionTotal =mi.PositionTotal;

            this.OpenTotal = mi.OpenTotal;
            this.Open24 = mi.OpenCountIn24;
            this.Close24 = mi.CloseCountIn24;
            this.Pure24 = mi.PureOpenCountIn24;
            this.Total = mi.Total;
            this.HitPrice = mi.HighPriceIn24;
            this.LowPrice = mi.LowPriceIn24;
            this.CurExe =MakeCe(mi.Contract.ExcuteTime);// ().ToString();

            var p = get1Prm(mi.Contract.Code);
            this.Buy1Price = p.Item1; this.Buy1Count = p.Item2;
            this.Sell1Price = p.Item3; this.Sell1Count = p.Item4;

           

            this.Fall = mi.fuser.MinPrice == null ? 0.01m : Math.Round((decimal)mi.fuser.MinPrice, 2);
            this.Raise = mi.fuser.MaxPrice == null ? BtcPrice.Current * ContractFuse.MinFuseRatioOfBtcPrice : Math.Round((decimal)mi.fuser.MaxPrice, 2);
            this.FuseSeconds = mi.fuser.RemainInSeconds;
        }
        public MarketDto() {  }
    }
    /// <summary>
    /// 一个用户相关的合约市场信息
    /// </summary>
    public class MyMarket
    {
        public decimal BtcCur { get; set; }
        /// <summary>
        /// 当前合约
        /// </summary>
        public MarketDto Main { get; set; }
        /// <summary>
        /// 相关合约
        /// </summary>
        public List<MarketDto> Related { get; set; }
        public MyMarket()
        {
            BtcCur =Math.Round(BtcPrice.Current,2);
        }
    }
}