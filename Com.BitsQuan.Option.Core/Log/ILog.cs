using System;

namespace Com.BitsQuan.Option.Core
{
    public interface ILog:IDisposable
    {
        void Debug(string logMsg, string desc = "", string who = "");
        void Error(Exception ex, string desc = "", string who = "");
        void Fatal(string logMsg, string desc = "", string who = "");
        void Info(string logMsg, string desc = "", string who = "");
        void Close();
    }
}
