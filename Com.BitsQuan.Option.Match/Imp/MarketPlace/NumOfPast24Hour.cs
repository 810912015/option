using Com.BitsQuan.Option.Core;
using System.Linq;

namespace Com.BitsQuan.Option.Match
{
    public class NumOfPast24Hour : InPastTimeSpan<int>
    {
        public NumOfPast24Hour() { this.TimeIntervalInMinutes = 24 * 60; }

        public int Total
        {
            get
            {
                if (queue.Count == 0) return 0;
                return queue.Select(a => a.Item2).Sum();
            }
        }
    }

    public class Past24Hour : InPastTimeSpan<decimal>
    {
        public Past24Hour() { this.TimeIntervalInMinutes = 24 * 60; }

        public decimal Total
        {
            get
            {
                if (queue.Count == 0) return 0;
                return queue.Select(a => a.Item2).Sum();
            }
        }
        public decimal HighPriceIn24
        {
            get
            {
                if (queue.Count == 0) return 0;
                return queue.Select(a => a.Item2).Max();
            }
        }
        public decimal LowPriceIn24
        {
            get
            {
                if (queue.Count == 0) return 0;
                return queue.Select(a => a.Item2).Min();
            }
        }

    }
}
