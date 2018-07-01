using Com.BitsQuan.Option.Core;
using System;

namespace Com.BitsQuan.Option.Imp
{
    public class InvitorFeeTrans
    {
        public Trader Trader { get; private set; }
        public decimal Ratio { get; private set; }
        public DateTime? LastTransferTime { get; private set; }
        public Action<decimal> UpdateSumAction { get; private set; }

        public InvitorFeeTrans(Trader t, decimal ratio, DateTime? lastTime, Action<decimal> updateAction)
        {
            this.Trader = t; this.Ratio = ratio; this.LastTransferTime = lastTime; this.UpdateSumAction = updateAction;
        }
    }
}
