using Com.BitsQuan.Option.Core;
using Com.BitsQuan.Option.Match;
using Com.BitsQuan.Option.Match.Imp;
using Com.BitsQuan.Option.Provider;
using System;
using System.Linq;
namespace Com.BitsQuan.Option.Imp
{
    /// <summary>
    /// 下单前处理
    /// </summary>
    public class OrderPreHandler : IDisposable
    {
        public static int CountPerMinuteLimit = MatchParams.CountPerMinuteLimit;
        public static int CountPerContractLimit = MatchParams.CountPerContractLimit;
        

        public OrderPreHandler()
        {
             
        }
        public Market Market { get; set; }
        /// <summary>
        /// 下单前处理:如果账户有足够的钱,则允许下单
        ///     卖开:卖出义务仓,账户收钱,因为卖出的是义务所以需要保证金 Sell Duty
        ///     卖平:买入义务仓,账户付钱,需要计算现金 Buy Duty
        ///     买开:买入权利仓,账户付钱,需要计算现金 Buy Right
        ///     买平:卖出权利仓,账户收钱  Sell Right
        ///     
        ///     如果是卖开(卖出权利),则计算账户保证金是否满足维持保证金要求
        ///     如果是买开(买入权利)或卖平(买入义务),则计算现金是否足够
        ///     
        ///     只能卖持有的权利仓位,且数量要小于等于持有数量
        /// </summary>
        /// <param name="o"></param>
        /// <returns></returns>
        public OperationResult CouldOrder(Order o)
        {
            try
            {
                if (null == o)
                {
                    return new OperationResult(6, "下单时委托不能为空");
                }
                if (o.Trader == null)
                {
                    return new OperationResult(7, "交易用户为空");
                }

                var mr = o.Trader.GetMaintainRatio() < 1;

                if (mr)
                {
                    return new OperationResult(8, "您当前正在爆仓,请等待爆仓完成后再下单");
                }
                if (o.HasSameDirNotSameOrderType())
                {
                    return new OperationResult(5, string.Format("挂单失败:存在同方向的{0}单,请先撤单或等待成交", 
                        o.OrderType == OrderType.开仓 ? "平仓" : "开仓"));
                }
                
                ////只能卖持有的权利仓位,且数量要小于等于持有数量 
                if (!o.IsPositionable())//判断是否为平仓
                {
                    var closableCount = o.Trader.GetClosableCount(o.Contract.Code, o.Direction);
                    if (o.ReportCount > closableCount)
                    {
                        o.RequestStatus = OrderRequestStatus.挂单失败;
                        return new OperationResult(3, string.Format("不能{0}{1}没有的期权:持仓数小于已下单数和本次下单数量之和", o.Direction.ToString(), o.OrderType.ToString()));
                    }

                }
                //卖开计算是否允许下单:保证金含持仓保证金,已下单保证金和当前单所需的保证金
                if (o.IsNeedOccupyMaintainBail())
                {

                    var oldmain = o.Trader.GetMaintain(Market);//持仓保证金及未成交的保证金
                    var newmain = o.GetWaitingMaintain(Market);//当前订单所需的保证金
                    var main = oldmain + newmain;
                    if (main > 0)
                    {
                        var ratio = o.Trader.Account.BailAccount.Sum / main;
                        if (ratio < SysPrm.Instance.MonitorParams.NormalMaintainRatio)
                        {
                            o.RequestStatus = OrderRequestStatus.挂单失败;
                            return new OperationResult(1, "保证金不足");
                        }
                    }
                }
                //买开计算是否允许下单:保证金含持仓保证金,已下单保证金
                if (o.IsNeedBailExceptMaintainBail())
                {
                    var mc = o.Trader.GetMaintain(Market);
                    var tc = o.Price * o.Count;
                    decimal raito = 0;
                    if (mc == 0)
                    {
                        raito = o.Trader.GetMaintainRatio(Market);
                    }
                    else
                    {
                        raito = (o.Trader.Account.BailAccount.Total - tc) / (mc);//持有义务仓的情况下
                    }



                    if (raito < SysPrm.Instance.MonitorParams.NormalMaintainRatio)
                    {
                        o.RequestStatus = OrderRequestStatus.挂单失败;
                        return new OperationResult(1, "保证金不足");
                    }
                }

                if (!o.Trader.CheckLimitPerMin(o))
                {
                    return new OperationResult(3, string.Format("每分钟下单量不能超过{0}份", OrderPreHandler.CountPerMinuteLimit));
                }
                if (!o.Trader.CheckLimitPerContract(o))
                {
                    return new OperationResult(4, string.Format("每个合约未成交委托个数不能超过{0}个", OrderPreHandler.CountPerContractLimit));
                }
                o.RequestStatus = OrderRequestStatus.挂单成功;
                //o.Detail = "挂单成功";
                return OperationResult.SuccessResult;
            }
            catch (Exception ex)
            {
                Singleton<TextLog>.Instance.Error(ex, "couldOrder");
                return new OperationResult(10, "未知错误");
            }
        }

        public void Dispose()
        {
        }
    }
}
