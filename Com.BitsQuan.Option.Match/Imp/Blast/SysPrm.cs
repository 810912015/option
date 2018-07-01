using Com.BitsQuan.Option.Core;

namespace Com.BitsQuan.Option.Match
{
    public class SysPrm : Singleton<SysPrm>
    {
        public OrderMonitorParam MonitorParams { get; private set; }

        public SysPrm()
        {
            MonitorParams = new OrderMonitorParam(1.2m,1.1m,1m,0.1m,100);
        }
    }
}
