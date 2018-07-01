using System;
using System.Text;

namespace Com.BitsQuan.Option.Core
{
    public class LogEntry
    {
        public DateTime When { get; set; }
        public string Who { get; set; }
        public string Description { get; set; }
        public string Content { get; set; }
        public LogLevel Level { get; set; }
        public Exception Exception { get; set; }
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendFormat("{0}|", When.ToString("yyyy-MM-dd HH:mm:ss.ffff"));
            if (!string.IsNullOrEmpty(Who))
                sb.AppendFormat("写入者:{0};", Who);
            if (Level != LogLevel.Info)
                sb.AppendFormat("严重程度:{0};", Level);
            if (!string.IsNullOrEmpty(Description))
                sb.AppendFormat("描述:{0}|", Description);
            sb.Append(Content);
            if (Exception != null)
                sb.AppendFormat("异常{0}", Exception.ToDetail());
            return sb.ToString();
        }
    }
}
