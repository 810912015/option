using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Com.BitsQuan.Option.Core
{
    public enum LogLevel
    {
        FatalError, Error, Wrong, Info
    }
    public class LogImp2Writer
    {
        string subdir;
        string fileName;
        public LogImp2Writer(string subDir, string fileName)
        {
            if (subDir != null)
                this.subdir = subDir;
            else this.subdir = "";
            if (fileName != null)
                this.fileName = fileName;
            else this.fileName = "";
        }
        string GetFilePath()
        {
            var dt=DateTime.Now;
            var cur = string.Format(@"{0}Log\{1}年\{2}月\{3}日\{4}时\{5}{6}",AppDomain.CurrentDomain.BaseDirectory , dt.Year, dt.Month, dt.Day, dt.Hour,
                string.IsNullOrEmpty(subdir)?"":subdir+"\\",fileName);
            return cur;
            //string exePath = AppDomain.CurrentDomain.BaseDirectory  +@"Log\";
            //if (!string.IsNullOrEmpty(subdir)) exePath += subdir + "\\";
            //string mainPath = exePath +DateTime.Now.ToString("yyyyMMddHH") + fileName;
            //return mainPath;
        }
        public void LogToFile(List<string> s)
        {

            try
            {
                string mainPath = GetFilePath();
                FileInfo fi = new FileInfo(mainPath);
                DirectoryInfo di = new DirectoryInfo(fi.DirectoryName);
                if (!di.Exists)
                {
                    di.Create();
                }
                if (!fi.Exists)
                {
                    using (var fs = File.Create(fi.FullName))
                    using (StreamWriter sw = new StreamWriter(fs, Encoding.Unicode))
                    {
                        foreach (var v in s)
                            sw.WriteLine(v);
                    }
                }
                else
                {
                    using (FileStream fs = new FileStream(mainPath, FileMode.Append, FileAccess.Write, FileShare.Write))
                    using (StreamWriter sw = new StreamWriter(fs, Encoding.Unicode))
                    {
                        foreach (var v in s)
                            sw.WriteLine(v);
                    }
                }
            }
            catch (Exception e)
            {
                Singleton<TextLog>.Instance.Error(e);
                throw;
            }

        }
    }
}
