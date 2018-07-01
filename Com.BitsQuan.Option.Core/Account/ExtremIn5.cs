using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace Com.BitsQuan.Option.Core
{

    public class ExtremIn5 : InPastTimeSpan<decimal>
    {
        public ExtremIn5(double minutes = 5)
            : base()
        {
            this.TimeIntervalInMinutes = minutes;
        }
        /// <summary>
        /// 5分钟内BTC最高价
        /// </summary>
        public decimal MaxIn5Min
        {
            get
            {
                if (queue == null || queue.Count == 0) return 0;
                return queue.Max(a => a.Item2);
            }
        }
        /// <summary>
        /// 5分钟内BTC最低价
        /// </summary>
        public decimal MinIn5Min
        {
            get
            {
                if (queue == null || queue.Count == 0) return 0;
                return queue.Min(a => a.Item2);
            }
        }
    }
}
