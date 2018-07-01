using Com.BitsQuan.Option.Core.Spot;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Com.BitsQuan.Option.Match.Spot
{
    /// <summary>
    /// 市价FOK：市价申报后立即全部成交，否则系统自动撤单。
    /// 市价FOK = 5
    /// </summary>
    public class SpotMarketFok : SpotArrange
    {
        public SpotMarketFok(Func<SpotOrder,bool, List<SpotOrder>> FindPossibleMatch,
            SpotOrderContainer Container,
            SpotModel model,
            Action<SpotOrder, SpotOrder> MakeDeal, Func<SpotOrder, bool> Redo)
            : base(FindPossibleMatch, Container, model, MakeDeal,Redo) { }
        public override void Match(SpotOrder o)
        {

            var l = FindPossibleMatch(o,true);// Container.FindByDirAndPos(o);
            if (l == null || l.Count == 0)
            {
                Redo(o);
                return;
            }
            var lc = l.Select(a => a.Count).Sum();
            if (lc < o.Count)
            {
                Redo(o);
                return;
            }

            HandleCount(o, l);
        }
    }
}
