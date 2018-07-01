
namespace Com.BitsQuan.Option.Match
{
    public class OrderMonitorParam
    {
        public decimal NormalMaintainRatio { get; set; }
        public decimal AlarmMaintainRatio { get; set; }
        public decimal BlastMaintainRatio { get; set; }
        public decimal BlastOnceRatio { get; set; }

        public int MaxCountPerSell { get; set; }

        public bool IsValid
        {
            get { return MaxCountPerSell>0&& BlastOnceRatio>0&&BlastMaintainRatio>=1&& NormalMaintainRatio > AlarmMaintainRatio && AlarmMaintainRatio > BlastMaintainRatio; }
        }

        public OrderMonitorParam(decimal normal = 1.2m, decimal alarm = 1.1m, decimal blast = 1m,decimal one=0.1m,int maxcount=100)
        {
            this.NormalMaintainRatio = normal;
            this.AlarmMaintainRatio = alarm;
            this.BlastMaintainRatio = blast;
            this.BlastOnceRatio = one;
            this.MaxCountPerSell = maxcount;
        }
        public OrderMonitorParam() { }
    }
}
