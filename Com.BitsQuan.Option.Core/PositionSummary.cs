using Com.BitsQuan.Option.Core;
using Com.BitsQuan.Option.Imp;
using System;
using System.Linq;

namespace Com.BitsQuan.Option.Core
{
    /// <summary>
    /// 持仓汇总
    /// </summary>
    public class PositionSummary
    {
        /// <summary>
        /// 合约代码
        /// </summary>
        public string CCode { get; private set; }
        /// <summary>
        /// 合约名称
        /// </summary>
        public string CName { get; private set; }
        Coin Coin;
        /// <summary>
        /// 持仓类型
        /// </summary>
        public string PositionType { get; private set; }

        PositionType ptype;
        
        public decimal GetMaintain(decimal curPrice)
        {
            if (ptype == Com.BitsQuan.Option.Core.PositionType.义务仓)
            {
                var b = this.Contract.GetMaintainForContract(curPrice);
                return b * Count; 
            }
            else
                return 0;
        }
        /// <summary>
        /// 持仓
        /// </summary>
        public int Count { get; private set; }
       

        /// <summary>
        /// 买入成本价
        /// </summary>
        public decimal BuyPrice { get; private set; }
        /// <summary>
        /// 买入成本
        /// </summary>
        public decimal BuyTotal { get; private set; }
        /// <summary>
        /// 浮动盈亏
        /// </summary>
        public decimal FloatProfit { get; private set; }
        /// <summary>
        /// 平仓盈亏
        /// </summary>
        public decimal CloseProfit { get; private set; }

        /// <summary>
        /// 合约市值
        /// </summary>
        public decimal TotalValue { get; private set; }
        public OrderType OrderType { get; private set; }
        
        public string Id { get; private set; }

        public Contract Contract { get; private set; }
        public PositionSummary() { }
        public PositionSummary(UserPosition up, decimal curPrice)
        {
            this.Contract = up.Order.Contract;
            this.OrderType = up.Order.OrderType;
            this.CCode = up.Order.Contract.Code;
            this.CName = up.Order.Contract.Name;
            this.Coin = up.Order.Contract.Coin;
            this.PositionType = up.Order.PositionType.ToString();
            this.ptype = up.Order.PositionType;
            Id = CCode;
            this.Count = up.Count;
            this.BuyPrice = up.Order.DonePrice;
 
            var thisTotal = up.Order.DonePrice * up.Count;
            this.BuyTotal = thisTotal; 
            Calc(curPrice, up.Count);
        }
        public PositionSummary(PositionSummaryData d)
        {
            this.Contract = d.Contract;
            this.OrderType = d.OrderType;
            this.CCode = d.Contract.Code; this.CName = d.Contract.Name; this.Coin = d.Contract.Coin;
            this.ptype = (Core.PositionType)Enum.Parse(typeof(Core.PositionType), d.PositionType, true);
            this.PositionType = d.PositionType;
            Id = d.Contract.Code;
            this.Count = d.Count;
            this.BuyPrice = d.BuyPrice;
            this.BuyTotal = d.BuyTotal;
            this.TotalValue = d.TotalValue;
            this.FloatProfit = d.FloatProfit;
            this.CloseProfit = d.CloseProfit;
        }
        //(2)	买入成本价=所有持仓合约的加权平均成本，计算的是每份持仓的平均成本。
        //(3)	买入成本=买入成本价*持仓数量
        //(4)	合约市值=持仓数量*期权合约卖出价。权利方为正数。义务方为负数。
        //(5)	浮动盈亏计算方式：浮动盈亏=权利仓 
        //期权买方浮动盈亏=合约市值-买入成本
        //期权卖方浮动盈亏=合约市值+买入成本
        //(6)平仓盈亏计算，权利仓平仓时，平仓盈亏=(成交价-成本价)*成交量-手续费。
        //义务仓平仓时，平仓盈亏=(成本价-成交价)*成交量-手续费。手续费=成交价*成交量*手续费率。
        //（7）权利仓，不占用维持保证金。

        //平仓盈亏是某个合约的已平仓产生的盈亏和,当持仓数为0时平仓盈亏为0

        public void Calc(decimal curPrice, int curCount)
        {
            this.TotalValue = this.ptype == Core.PositionType.权利仓 ? Count * curPrice : Count * curPrice * (-1); 
            this.FloatProfit =  this.ptype == Core.PositionType.权利仓 ?
            TotalValue - BuyTotal : TotalValue + BuyTotal;
        }
      
        /// <summary>
        /// 计算平仓盈亏:计算此持仓的盈亏,然后累加到已有值
        /// </summary>
        /// <param name="up"></param>
         void CalcClosableProfit(UserPosition up, decimal curPrice)
        {
            if (up.Order.PositionType == this.ptype)
            {
              
                var dp = this.BuyPrice - curPrice;
                var count = up.Count;

                var dt =dp * count;
                if (this.ptype == Core.PositionType.义务仓)
                    this.CloseProfit += dt;
                else
                    this.CloseProfit += dt * (-1);
            }

        }
        void AddPosition(UserPosition up, decimal curPrice)
        {
            this.Count += up.Count;
          
            //总金额加上本次成交的金额
            var thisTotal = up.Order.DonePrice * up.Order.DoneCount;
            this.BuyTotal += thisTotal;
            this.BuyPrice =this.Count==0?0: this.BuyTotal / this.Count;
            Calc( curPrice, up.Count);
        }
        
        void SubPosition(UserPosition up, decimal curPrice)
        {
            try
            {
                this.Count -= up.Count;
                //仓位转换:数量为0清空持仓,数量＞0正常计算,小于0转换仓位类型以剩余数量计算
                if (Count == 0)
                {
                    ClearIndex();
                }
                else if (Count < 0)
                {
                    
                    ClearIndex();
                    //只有持仓类型不同才翻转
                    if (up.Order.PositionType != this.ptype)
                    {
                        if (this.ptype == Core.PositionType.义务仓)
                        {
                            this.ptype = Core.PositionType.权利仓;
                        }
                        else
                        {
                            this.ptype = Core.PositionType.义务仓;
                        }
                        this.PositionType = ptype.ToString();
                        this.Count = this.Count * (-1);
                     
                        this.BuyPrice = up.Order.DonePrice;

                        var thisTotal = up.Order.DonePrice * this.Count;
                        this.BuyTotal = thisTotal;
                        Calc(curPrice, this.Count);
                    }
                }
                else
                    if (Count > 0)
                    {
                        //如果10买一个,20买一个,再100卖一个,则成本价为负数
                        //var thisTotal = up.Count * up.Order.DonePrice;
                        //this.BuyTotal -= thisTotal;
                        //this.BuyPrice =this.BuyTotal / this.Count;
                        //修改为当量减少时,只减少成本,单价不变
                        this.BuyTotal -= up.Count * this.BuyPrice;

                        Calc(curPrice, up.Count);
                        CalcClosableProfit(up, curPrice);
                    }
            }
            catch (Exception ex)
            {
                Singleton<TextLog>.Instance.Error(ex, "subposition");
            }

        }
        /// <summary>
        /// 重置各计算指标:当持仓数量为0时
        /// </summary>
        void ClearIndex()
        {
            this.BuyPrice = this.BuyTotal = this.CloseProfit = this.TotalValue = this.FloatProfit = 0;
        }

        public void Update(UserPosition up, bool isAdd, decimal curPrice)
        {
            if (this.Count == 0)
            {
                ClearIndex();
                this.ptype = up.Order.PositionType;
                this.PositionType = ptype.ToString();
                //因为持仓数为0,所以只计算开仓,不再计算平仓
                if (isAdd)
                {
                    AddPosition(up, curPrice);
                }
              
            }
            else
            {
                if (up.Order.PositionType == ptype)
                {
                    if (isAdd) AddPosition(up, curPrice);
                    else SubPosition(up, curPrice);
                }
                else
                {
                    if (isAdd)//只有开仓时才计算:开权利仓=平=义务仓
                    {
                        if (!isAdd) AddPosition(up, curPrice);
                        else SubPosition(up, curPrice);
                    }

                }
            }

        }

        PositionType  GetPositionTypeForClose(TradeDirectType dir)
        {
            return dir == TradeDirectType.买 ? Core.PositionType.义务仓 : Core.PositionType.权利仓;
        }
       
    }


    
}
