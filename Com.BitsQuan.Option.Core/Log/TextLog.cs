using System;
using System.Collections.Generic;
using System.Linq;
using System.Timers;

namespace Com.BitsQuan.Option.Core
{
    /// <summary>
    /// 文本文件日志:每一小时写一个文件
    /// </summary>
    public class TextLog : ILog
    {
        string who;
        bool isWriteToConsole;
        LogImp2Writer writer;
        Queue<LogEntry> q;
        Timer t;
        object syncObj;
        public TextLog() : this("GlobalLog.txt") { }
        public TextLog(string fileName, string subDir = "", string who = "", bool isWriteToConsole = true,
            int writeInvervalInSeconds = 1)
        {
            this.writer = new LogImp2Writer(subDir, fileName);
            this.who = who;
            this.isWriteToConsole = isWriteToConsole;
            q = new Queue<LogEntry>();
            syncObj = new object();
            t = new Timer(writeInvervalInSeconds * 1000);
            t.Elapsed += new ElapsedEventHandler(t_Elapsed);
            t.Start();
        }

        void t_Elapsed(object sender, ElapsedEventArgs e)
        {
            try
            {
                t.Stop();
                List<LogEntry> l = q.ToList<LogEntry>();
                lock (syncObj)
                    q.Clear();
                List<string> ls = new List<string>();
                for (int i = 0; i < l.Count; i++)
                {
                    ls.Add(l[i].ToString());
                }
                writer.LogToFile(ls);

            }
            catch (Exception ex)
            {
                lock (syncObj)
                {
                    q.Clear();
                }
                Console.WriteLine(string.Format("{0}|日志写入出错,将清空内存中的日志:{1}", DateTime.Now, ex.Message));
            }
            finally
            {
                t.Start();
            }
        }

        void LogIt(string logMsg, string desc, LogLevel level, Exception e = null, string who = "")
        {
            LogEntry le = new LogEntry
            {
                Who = string.IsNullOrEmpty(who) ? this.who : who,
                Exception = e,
                Level = level,
                Description = desc,
                When = DateTime.Now,
                Content = logMsg
            };
            if (isWriteToConsole)
            {
                var m = le.ToString();
                Console.WriteLine(m);
            }
            lock (syncObj)
                q.Enqueue(le);
        }


        public void Debug(string logMsg, string desc = "", string who = "")
        {
            LogIt(logMsg, desc, LogLevel.Info, null, who);
        }

        public void Error(Exception ex, string desc = "", string who = "")
        {
            LogIt("", desc, LogLevel.Error, ex, who);
        }

        public void Fatal(string logMsg, string desc = "", string who = "")
        {
            LogIt(logMsg, desc, LogLevel.FatalError, null, who);
        }

        public void Info(string logMsg, string desc = "", string who = "")
        {
            LogIt(logMsg, desc, LogLevel.Info, null, who);
        }

        public void Close()
        {
            Flush();
            t.Stop();
            t.Elapsed -= new ElapsedEventHandler(t_Elapsed);
            t.Close();
            t.Dispose();
            q.Clear();
            writer = null;
        }

        public void Debug(string logMsg)
        {
            LogIt(logMsg, "", LogLevel.Info);
        }

        public void Error(Exception ex)
        {
            LogIt("", "", LogLevel.Error, ex);
        }

        public void Fatal(string logMsg)
        {
            LogIt(logMsg, "", LogLevel.FatalError);
        }

        public void Info(string logMsg)
        {
            LogIt(logMsg, "", LogLevel.Info);
        }

        public void Nake(string msg)
        {
            LogIt(msg, "", LogLevel.Info);
        }


        public void Flush()
        {
            t_Elapsed(null, null);
        }

        public void Dispose()
        {
            Close();
        }
    }
}
