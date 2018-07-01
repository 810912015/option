using Com.BitsQuan.Option.Core;
using Com.BitsQuan.Option.Match.Imp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Timers;

namespace Com.BitsQuan.Option.Match
{
    public class AccountUpdator
    {
        List<Account> al;
        object loc;
        Timer t;
        string f;
        int intervalInseconds;
        public AccountUpdator(int invervalInSeconds = 30)
        {
            al = new List<Account>();
            loc = new object();
            t = new Timer();
            this.intervalInseconds = invervalInSeconds;
            t.Interval = intervalInseconds * 1000;
            t.Elapsed += t_Elapsed;
            f = "update Accounts set Sum={0},Frozen={1} where Id={2} ";
            Account.OnTotalChanged += Update;
            t.Start();
        }
        public void Update(Account a)
        {
            if (al.Contains(a)) return;
            lock (loc)
            {
                if (!al.Contains(a))
                    al.Add(a);
            }
        }
        void Execute()
        {
            if (al.Count == 0) return;
            List<Account> tl;
            lock (loc)
            {
                tl = al.ToList();
                al.Clear();
            }
            if (tl == null || tl.Count == 0) return;
            StringBuilder sb = new StringBuilder();
            foreach (var v in tl)
            {
                sb.AppendFormat(f, v.Sum, v.Frozen, v.Id);
            }
            using (DBServer db = new DBServer())
            {
                db.ExecNonQuery(sb.ToString());
            }
        }
        void t_Elapsed(object sender, ElapsedEventArgs e)
        {
            try
            {
                t.Stop();
                Execute();
            }
            catch (Exception ex)
            {
                Singleton<TextLog>.Instance.Error(ex, "accountupdator");
            }
            finally
            {
                t.Start();
            }
        }
    }
}
