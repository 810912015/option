using Com.BitsQuan.Option.Core;
using Com.BitsQuan.Option.Imp;
using System;
using Com.BitsQuan.Option.Match.Imp;

namespace Com.BitsQuan.Option.Match
{
    /// <summary>
    /// 爆仓下单器:循环下单,提交撮合,直至数量达到要求或以熔断价格挂出
    /// </summary>
    public class BlastSellor
    {
        PositionType positionType;
        TextLog Log;
        Func<Trader, PositionSummary, int, Tuple<Order, bool>> CreateSellOrder;
        Action<BlasterOperaton> RaiseSell;
        IMatch matcher;

        public BlastSellor(PositionType positionType,
        TextLog Log,
        Func<Trader, PositionSummary, int, Tuple<Order, bool>> CreateSellOrder,
        Action<BlasterOperaton> RaiseSell,
        IMatch matcher)
        {
            this.positionType = positionType;
            this.Log = Log;
            this.CreateSellOrder = CreateSellOrder;
            this.RaiseSell = RaiseSell;
            this.matcher = matcher;
            MaxLoopCount = 5;
        }
        public int MaxLoopCount { get; private set; }

        public void Sell(Trader t, PositionSummary up, int count, int blastId)
        {
            int total = count;
            t.SetSell(up, true); 
            int loopcount = 0;
            while (total > 0)
            {
                //检查持仓
                var pos = t.GetPositionSummary(up.CCode, PositionType.权利仓);
                if (pos == null) break;
                lock (pos.PositionType)
                {
                    if (pos.PositionType != this.positionType.ToString()) break;
                    var closable = t.GetClosableCount(pos);
                    if (closable <= 0) break;

                    if (loopcount++ > MaxLoopCount)
                    {
                        Log.Info(string.Format("平仓循环超过{0}次,结束:{1}3-2平仓操作:{2}-仓{3}-平数{4}", MaxLoopCount, up.CName,
                        this.positionType,
                        up, count));
                        break;
                    }
                    var cs = CreateSellOrder(t,up, total);

                    matcher.Handle(cs.Item1);

                    Log.Info(string.Format("{0}平仓操作:{1}-仓{2}-平数{3}-熔{4}-单{5}", up.CName,
                        this.positionType,
                        up.CName, count, cs.Item2, cs.Item1.ToShortString()));
                    BlasterOperaton bo;

                    bo = new BlasterOperaton
                    {
                    Id = IdService<BlasterOperaton>.Instance.NewId(),
                    BlasterRecordId = blastId,
                    OpOrderId = cs.Item1.Id,
                    Order = cs.Item1,
                    PositionId =0,
                    Result = cs.Item1.State == OrderState.已成交
                    };


                    RaiseSell(bo);

                    if (t.GetMaintainRatio() >= 1) return;//当保证率大于等于1时平仓结束

                    if (cs.Item2)
                    {
                        return;
                    }
                    else
                    {

                        total -= cs.Item1.TotalDoneCount;
                    }
                }

            }
            t.SetSell(up, false);
        }
    }
}