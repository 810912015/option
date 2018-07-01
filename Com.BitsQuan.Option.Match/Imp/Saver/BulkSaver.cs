using Com.BitsQuan.Option.Core;
using System;
using System.Data;
using System.Data.SqlClient;
using System.Text;
using System.Threading;

namespace Com.BitsQuan.Option.Match.Imp
{

    /// <summary>
    /// 批量存储类型--内存表,定时批量写入数据库
    /// </summary>
    public abstract class BulkSaver:IDisposable
    {
        protected string connStr { get; set; }
        object tableLock;
        protected DataTable table { get; set; }
        protected string tableName { get; set; }
        int maxRowCount { get; set; }

        System.Timers.Timer t;
        /// <summary>
        /// 批量存储类
        /// </summary>
        /// <param name="connStr">连接字符串</param>
        /// <param name="tableName">要存入的数据库表名称</param>
        /// <param name="saveIntervalInSec">保存间隔(单位秒,默认1)</param>
        public BulkSaver(string connStr, string tableName, int saveIntervalInSec = 1, int maxRowCount=3000)
        {
            if (string.IsNullOrEmpty(connStr) || string.IsNullOrEmpty(tableName))
                throw new ArgumentException("连接字串或表名称不能为空或空白");
            this.connStr = connStr; this.tableName = tableName;
            tableLock = new object();
            CreateTable();
            this.maxRowCount = maxRowCount;
            t = new System.Timers.Timer(saveIntervalInSec * 1000);
            t.Elapsed += new System.Timers.ElapsedEventHandler(t_Elapsed);
            t.Start();
        }
        /// <summary>
        /// 用于结束时:将内存表中的数据提交至数据库
        /// </summary>
        public void Flush()
        {
            t_Elapsed(null, null);
        }
        public virtual void BeforeBulk() { }
        static Semaphore sem = new Semaphore(2, 2);
        void t_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            try
            {
                t.Stop();
                if (table.Rows.Count <= 0) return;
                sem.WaitOne();
                    if (table.Rows.Count > 0)
                    {
                        BeforeBulk();
                        DataTable dt;
                        lock (tableLock)
                        {
                            dt = table.Copy();
                            if (dt.Rows.Count > 0)
                                table.Clear();
                        }

                        BulkCopy(dt);
                    }
                sem.Release();
            }
            catch (Exception ex) 
            { 
                Singleton<TextLog>.Instance.Error(ex, GetTableName());
            }
            finally
            {
                
                t.Start();
            }
        }
        protected virtual string GetTableName()
        {
            return tableName;
        }
        int ParseKey(string s)
        {
            try
            {
                var start = s.IndexOf('(');
                var end = s.IndexOf(')');
                var tar = s.Substring(start + 1, end - start - 1);
                int k = -1;
                int.TryParse(tar, out k);
                return k;
            }
            catch { return -1; }
        }
        void BulkCopy(DataTable dt)
        {
            try
            {
                if (dt.Rows.Count > 0)
                {
                    using (SqlBulkCopy sbc = new SqlBulkCopy(connStr, SqlBulkCopyOptions.TableLock))
                    {
                        sbc.BatchSize = dt.Rows.Count;
                        sbc.DestinationTableName = GetTableName();
                        
                        sbc.WriteToServer(dt);
                    }
                }
            }
            catch (Exception e)
            {
                //if (GetTableName().Contains("Orders"))
                //{
                //    var k = ParseKey(e.Message);
                //    if (k > -1)
                //    {
                //        var v = dt.Rows.Find(k);
                //        Singleton<TextLog>.Instance.Info(string.Format("抛弃{0}冲突id值{1}", GetTableName(), k));
                //        dt.Rows.Remove(v);
                //        if (dt.Rows.Count > 0)
                //        {
                //            lock (tableLock)
                //            {
                //                foreach (DataRow row in dt.Rows)
                //                {
                //                    table.Rows.Add(row.ItemArray);
                //                }
                //            }
                           
                //            dt.Dispose();
                //            return;
                //        }
                //    }
                //}


                StringBuilder sb = new StringBuilder();
                sb.Append("丢弃的数据:");
                foreach (DataRow v in dt.Rows)
                {
                    foreach (var c in v.ItemArray)
                        sb.AppendFormat("{0}-", c == null ? "" : c.ToString());
                }
                Singleton<TextLog>.Instance.Error(e,sb.ToString(), GetTableName());
            }
            finally
            {
                dt.Clear();
                dt.Dispose();
            }
            
        }
        /// <summary>
        /// 构造内存表
        /// </summary>
        protected abstract void CreateTable();
        /// <summary>
        /// 将对象保存至内存表
        /// </summary>
        /// <param name="o">要保存的对象</param>
        protected  abstract void AddToTable(object o);
        /// <summary>
        /// 是否应该保存
        /// </summary>
        /// <param name="o">要保存的对象</param>
        /// <returns></returns>
        protected abstract bool ShouldSave(object o);
        /// <summary>
        /// 保存
        /// </summary>
        /// <param name="o">要保存的对象</param>
        public virtual void Save(object o)
        {
            try
            {
                if (ShouldSave(o))
                {
                    lock (tableLock)
                    {
                        while (table.Rows.Count > 3000)
                        {
                            table.Rows.RemoveAt(0);
                        }
                        AddToTable(o);
                    }
                    t.Start();
                }
            }
            catch(Exception e) {
                Singleton<TextLog>.Instance.Error(e, "bulk saver", GetTableName());
                throw; }
        }


        public void Dispose()
        {
            Flush();
            if (t != null)
            {
                t.Elapsed -= t_Elapsed;
                t.Dispose();
                t = null;
            }
            
        }
    }
}
