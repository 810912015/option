using System;
using System.Text;

namespace Com.BitsQuan.Option.Core
{

    public static class ExceptionExtend
    {
        public static string ToDetail(this Exception e)
        {
            if (e == null) return "";
            StringBuilder sb = new StringBuilder();
            sb.AppendFormat("\r\n           Message:{0}", e.Message);
            sb.AppendFormat("\r\n           Source:{0}", e.Source == null ? "" : e.Source);
            sb.AppendFormat("\r\n           StackTrace:{0}", e.StackTrace == null ? "" : e.StackTrace);
            sb.AppendFormat("\r\n           TargetSite:{0}", e.TargetSite == null ? "" : e.TargetSite.ToString());
            if (e.InnerException != null)
                sb.AppendFormat("\r\n           InnerException:{0}", e.InnerException.ToDetail());

            return sb.ToString();
        }
    }
}
