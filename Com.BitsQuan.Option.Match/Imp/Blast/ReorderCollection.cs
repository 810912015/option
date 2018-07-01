using Com.BitsQuan.Option.Core;
using Com.BitsQuan.Option.Imp;
using System;
using System.Collections.Generic;
using System.Linq;
using Com.BitsQuan.Option.Match.Imp;

namespace Com.BitsQuan.Option.Match
{
    public class ReorderCollection
    {
        List<ReorderItem> list;
        IMatch matcher;
        object reOrderSync;
        public ReorderCollection(IMatch match)
        {
            matcher = match;
            list = new List<ReorderItem>();
            reOrderSync = new object();
            matcher.OnFinish += matcher_OnFinish;
        }
        void ClearItem(ReorderItem q)
        {
            matcher.Redo(q.order);
            q.OnDone -= feh_OnDone;
            list.Remove(q);
           // q.Dispose();
        }
        void matcher_OnFinish(Order obj)
        {
            if (obj.State == OrderState.已成交)
            {
                var q = list.Where(a => a.IsMyOrder(obj.Id)).FirstOrDefault();
                if (q != null)
                {

                    lock (reOrderSync)
                    {
                        ClearItem(q);
                    }  
                }
            }
        }

        public void Add(Order up, ContractFuse cf, bool isHandelMax,PositionSummary ps)
        {
            var feh = new ReorderItem(up, cf, isHandelMax,ps,this.matcher);
            feh.OnDone += feh_OnDone;
            list.Add(feh);
        }
        /// <summary>
        /// 爆仓结束后,撤掉一个用户所有因爆仓挂出的单子
        /// </summary>
        /// <param name="t"></param>
        public void RedoAllMyOrder(Trader t)
        {
            var q = list.Where(a => a.order.Trader == t).ToList();
            foreach (var v in q)
            {
                matcher.Redo(v.order);
                ClearItem(v);
            }
        }

        void feh_OnDone(ReorderItem arg)
        {
            ClearItem(arg);
        }
    }
}
