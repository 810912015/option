using Com.BitsQuan.Option.Core;
using System;
using System.Collections.Generic;

namespace Com.BitsQuan.Option.Match
{
    public class Watcher
    {
        Dictionary<Trader, WatchTrader> dic = new Dictionary<Trader, WatchTrader>();
        Action<Trader> act;
        Action<Trader> clearAct;
        object sync;
        public Watcher(Action<Trader> bact, Action<Trader> clearAct)
        {
            this.act = bact;
            this.clearAct = clearAct;
            sync = new object();
        }
        public void Watch(Trader t)
        {
            lock (sync)
            {
                if (!dic.ContainsKey(t))
                {
                    var wt = new WatchTrader(t, act, clearAct); 
                    dic.Add(t, wt);
                    wt.OnStop += wt_OnDisposed;
                }
            }
        }

        void wt_OnDisposed(Trader obj)
        {
            lock (sync)
            {
                if (dic.ContainsKey(obj))
                {
                    var tt = dic[obj];
                    tt.OnStop -= wt_OnDisposed;
                    dic.Remove(obj);
                }
            }
        }
    }
}
