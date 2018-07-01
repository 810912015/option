using Com.BitsQuan.Option.Core.Spot;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Com.BitsQuan.Option.Match.Spot
{
    /// <summary>
    /// 限价FOK：手工设定价格，申报后立即全部成交，否则系统自动撤单。
    /// 限价FOK = 4,
    /// </summary>
    public class SpotPriceFok : SpotArrange
    {
        public SpotPriceFok(Func<SpotOrder,bool, List<SpotOrder>> FindPossibleMatch,
            SpotOrderContainer Container,
            SpotModel model,
            Action<SpotOrder, SpotOrder> MakeDeal, Func<SpotOrder, bool> Redo)
            : base(FindPossibleMatch, Container, model, MakeDeal,Redo) { }
        public override void Match(SpotOrder o)
        {
            var r = FindPossibleMatch(o,false);
            if (r.Count == 0)
            {
                Redo(o);
                return;
            }
            var rc = r.Select(a => a.Count).Sum();
            if (rc < o.Count)
            {
                Redo(o);
                return;
            }
            HandleCount(o, r);
        }
    }
}
