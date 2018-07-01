using Com.BitsQuan.Option.Core;
using Com.BitsQuan.Option.Core.Spot;
using System;
using System.Collections.Generic;

namespace Com.BitsQuan.Option.Match.Spot
{

    public abstract class SpotArrange
    {
        protected object matchLock { get; set; }
        protected Func<SpotOrder,bool, List<SpotOrder>> FindPossibleMatch;
        protected SpotOrderContainer Container;
        protected SpotModel model;
        protected Action<SpotOrder, SpotOrder> MakeDeal;
        protected Func<SpotOrder, bool> Redo;

        public SpotArrange(Func<SpotOrder,bool, List<SpotOrder>> FindPossibleMatch,
            SpotOrderContainer Container,
            SpotModel model,
            Action<SpotOrder, SpotOrder> MakeDeal, Func<SpotOrder, bool> Redo)
        {
            this.matchLock = new object();
            this.FindPossibleMatch = FindPossibleMatch;
            this.Container = Container;
            this.model = model;
            this.MakeDeal = MakeDeal;
            this.Redo = Redo;
        }
        
        public abstract void Match(SpotOrder so);
        public virtual void HandleCount(SpotOrder so, List<SpotOrder> p)
        {
            lock (matchLock)
            {
                if (p == null || p.Count == 0)
                {
                    if (so.IsArrangable())
                    {
                        Container.Add(so);
                        model.SpotOrders.Add(so);
                        so.Trader.AddSpotOrder(so);
                    }
                    return;
                }
                foreach (var v in p)
                {
                    if (!v.IsArrangable()) continue;
                    if (v.IsArranging()) continue;
                    v.SetArranging(true);
                    if (v.Count >= so.Count)
                    {
                        MakeDeal(so, v);
                        v.SetArranging(false);
                        break;
                    }
                    else
                    {
                        MakeDeal(so, v);
                        v.SetArranging(false);
                        continue;
                    }

                }
                if (so.IsArrangable())
                {
                   // if()
                    Container.Add(so);
                    model.SpotOrders.Add(so);
                    so.Trader.AddSpotOrder(so);
                }
            }
        }
    }
}
