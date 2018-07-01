using System;
namespace Com.BitsQuan.Option.Match
{ 
    public class MatchParams
    {
        public static readonly double FuseSpanInMinutes=5;
        public static readonly decimal RaiseMax = 1000;
        public static readonly decimal FallMax = 0.5m;
        public static readonly int CountPerMinuteLimit = 100;
        public static readonly int CountPerContractLimit = 100;
        public static readonly int FakeCny = 1000000;
        public static readonly int FakeBail = 1000000;
        public static readonly int FakeBtc = 500;
        public static bool UseFake = true;
        public static TimeSpan ShareSpan = new TimeSpan(5, 0, 0);

        static MatchParams()
        {
            var fsi = System.Configuration.ConfigurationManager.AppSettings["fuseSpanInMinutes"].ToString();
            var rm = System.Configuration.ConfigurationManager.AppSettings["raiseMax"].ToString();
            var fm = System.Configuration.ConfigurationManager.AppSettings["fallMax"].ToString();
            var cpm = System.Configuration.ConfigurationManager.AppSettings["CountPerMinuteLimit"].ToString();
        //    var cpcl = System.Configuration.ConfigurationManager.AppSettings["CountPerContractLimit"].ToString();
            var fc = System.Configuration.ConfigurationManager.AppSettings["FakeCny"].ToString();
            var fb = System.Configuration.ConfigurationManager.AppSettings["FakeBail"].ToString();
            var fbtc = System.Configuration.ConfigurationManager.AppSettings["FakeBtc"].ToString();
            var uf = System.Configuration.ConfigurationManager.AppSettings["UseFake"].ToString();

            double.TryParse(fsi, out FuseSpanInMinutes);
            decimal.TryParse(rm, out RaiseMax);
            decimal.TryParse(fm, out FallMax);
            int.TryParse(cpm, out CountPerMinuteLimit);
        //    int.TryParse(cpcl, out CountPerContractLimit);
            int.TryParse(fc, out FakeCny);
            int.TryParse(fb, out FakeBail);
            int.TryParse(fbtc, out FakeBtc);
            bool.TryParse(uf, out UseFake);

            var ss = System.Configuration.ConfigurationManager.AppSettings["ShareSpan"].ToString();
            int hour=5, min=0, sec=0;
            var ssa = ss.Split(',');
            int.TryParse(ssa[0], out hour);
            int.TryParse(ssa[1], out min);
            int.TryParse(ssa[2], out sec);
            ShareSpan = new TimeSpan(hour, min, sec);
            
        }
    }
}
