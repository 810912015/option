using Com.BitsQuan.Option.Core;
using System;
using System.Timers;

namespace Com.BitsQuan.Option.Match
{

    public class FuseExtrem : ExtremIn5
    {
        //Timer t;
        public FuseExtrem(double intervalInMin)
            : base(intervalInMin)
        {
        }
        /// <summary>
        /// 极值变化事件
        /// 参数:最大值,最小值
        /// </summary>
        public event Action<decimal, decimal> OnExtremChanged;

        decimal PreMax;
        decimal PreMin;

        public override void Put(decimal d)
        {
            if (d >= BtcPrice.Current * 0.03m)
            {
                base.Put(d);
                Cal();
            }

        }
        void Cal()
        {
            var max = this.MaxIn5Min;
            var min = this.MinIn5Min;
            if (max != PreMax || min != PreMin)
            {
                PreMax = max;
                PreMin = min;
                RaiseChanged(max, min);
            }
        }
        void RaiseChanged(decimal max, decimal min)
        {
            if (OnExtremChanged != null)
                OnExtremChanged(max, min);
        }
    }
}
