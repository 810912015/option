using Com.BitsQuan.Option.Core;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Com.BitsQuan.Option.Match
{
    /// <summary>
    /// 一个合约的当前行情
    /// </summary>
    public class MarketItem 
    { 
        /// <summary>
        /// 合约
        /// </summary>
        public Contract Contract { get; private set; }
        /// <summary>
        /// 最新报价
        /// </summary>
        public decimal NewestPrice { get; private set; }
        /// <summary>
        /// 当前队列中卖1数量
        /// </summary>
        public int SellCount { get; private set; }
        /// <summary>
        /// 当前队列中卖1数量
        /// </summary>
        public int BuyCount { get; private set; }
        
        public ContractFuse fuser { get; private set; }

        public MarketItem(Contract c )
        {
            this.Contract = c;
            fuser = new ContractFuse(c); 
        }

        public MarketItem(MarketRecord mr,Contract c)
        {
            this.Contract = c;
            this.fuser = new ContractFuse(c,mr.FuseMax,mr.FuseMin);
            this.NewestPrice = mr.NewestPrice;
            this.NewestDealPrice = mr.NewestDealPrice;
            this.Copis = mr.Copies;
            this.Times = mr.Times;
            this.Total = mr.Total;
            this.OpenTotal = mr.OpenTotal;
            this.SellCount = mr.SellCount;
            this.BuyCount = mr.BuyCount;
             
        }

        public void Handle(Order o)
        {
            this.NewestPrice = o.Price;
             
        }
        /// <summary>
        /// 成交数量
        /// </summary>
        public int Times { get; private set; }
        /// <summary>
        /// 成交份数
        /// </summary>
        public int Copis { get; private set; }
        public event Action<Contract, decimal> OnDealPriceChanged;
        void RaiseDealPriceChanged()
        {
            if (OnDealPriceChanged != null)
            {
                foreach (var v in OnDealPriceChanged.GetInvocationList())
                {
                    try
                    {
                        ((Action<Contract, decimal>)v).BeginInvoke(this.Contract, this.NewestDealPrice, null, null);
                    }
                    catch(Exception ex)
                    {
                        Singleton<TextLog>.Instance.Error(ex, "marketitem deal changed event");
                        continue;
                    }
                }
            }
        }
        decimal ndp;
        /// <summary>
        /// 最新成交价--市价
        /// </summary>
        public decimal NewestDealPrice { get { return ndp; } private set { if (ndp != value) { ndp = value; RaiseDealPriceChanged(); } } }
        /// <summary>
        /// 成交总额
        /// </summary>
        public decimal Total { get; private set; }
        public bool IsAdd { get; private set; }
        /// <summary>
        /// 成交最高价
        /// </summary>

        /// <summary>
        /// 计算市价(成交价),成交总额,交易次数
        /// 和熔断判断
        /// </summary>
        /// <param name="d"></param>
        public void Handle(Deal d)
        {
            if (d.Price >= this.NewestDealPrice)
            {
                this.IsAdd = true;
            }
            else
            {
                this.IsAdd = false;
            }
            this.NewestDealPrice = d.Price;
           
            this.Total += d.Price * d.Count;
            this.Times += 1;
            this.Copis += d.Count;
            this.NewestPrice = d.Price;
            PriceIn24.Put(d.Price);
            fuser.Handle(d);
        }
        /// <summary>
        /// 持仓总数
        /// </summary>
        public decimal PositionTotal { get; private set; }
        NumOfPast24Hour openIn24 = new NumOfPast24Hour();
        NumOfPast24Hour closeIn24 = new NumOfPast24Hour();
        Past24Hour PriceIn24 = new Past24Hour();
        /// <summary>
        /// 24小时开仓数
        /// </summary>
        public int OpenCountIn24 { get { return openIn24.Total/2; } }
        /// <summary>
        /// 24小时平仓数
        /// </summary>
        public int CloseCountIn24 { get { return closeIn24.Total/2; } }
        /// <summary>
        /// 24小时净开仓数
        /// </summary>
        public int PureOpenCountIn24 { get { return (openIn24.Total - closeIn24.Total)/2; } }
        /// <summary>
        /// 总开仓
        /// </summary>
        public int OpenTotal { get; set; }

        public decimal HighPriceIn24 { get { return PriceIn24.HighPriceIn24; } }
        public decimal LowPriceIn24 { get { return PriceIn24.LowPriceIn24; } }
        
        /// <summary>
        /// 计算持仓数:无论是开仓数还是持仓数字均只计算义务仓,权利仓和义务仓一定相等.
        /// </summary>
        /// <param name="ups"></param>
        /// <param name="isAdd"></param>
        public void HandlePosition(List<UserPosition> ups, bool isAdd)
        {

            if (isAdd)
            {
                var q = ups.Select(a => a.Count).Sum();
                openIn24.Put(q); 
            }
            else
            {
                var q = ups.Select(a => a.Count).Sum();
                closeIn24.Put(q); 
            }

        }

        public MarketRecord ToRecord()
        {
            return new MarketRecord
            {
                Low24 = this.LowPriceIn24,
                High24 = this.HighPriceIn24,
                PureOpen24 = this.PureOpenCountIn24,
                Close24 = this.CloseCountIn24,
                Open24 = this.OpenCountIn24,
                OpenTotal = this.OpenTotal,
                Total = this.Total,
                NewestDealPrice = this.NewestDealPrice,
                Copies = this.Copis,
                Times = this.Times,
                FuseMin = fuser.MinPrice ?? 0,
                BuyCount = this.BuyCount,
                ContractId = this.Contract.Id,
                FuseMax = fuser.MaxPrice ?? 0,
                NewestPrice = this.NewestPrice,
                SellCount = this.SellCount
            };
        }
       
    }
}
