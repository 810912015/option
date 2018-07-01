using Com.BitsQuan.Option.Core;
using Com.BitsQuan.Option.Core.Spot;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Com.BitsQuan.Option.Match.Spot
{

    /// <summary>
    /// 市价IOC：按对手方最优报价最大限度成交，不成交部分系统自动撤单。
    /// 市价IOC = 2,
    /// </summary>
    public class SpotMarketIoc : SpotArrange
    {
        public SpotMarketIoc(Func<SpotOrder,bool, List<SpotOrder>> FindPossibleMatch,
            SpotOrderContainer Container,
            SpotModel model,
            Action<SpotOrder, SpotOrder> MakeDeal, Func<SpotOrder, bool> Redo)
            : base(FindPossibleMatch, Container, model, MakeDeal,Redo) { }
        public override void Match(SpotOrder o)
        {
            var l = FindPossibleMatch(o,true);
            if (l == null)
            {
                Redo(o);
                return;
            }
            var ll = FindPossibleMatch(o, true).Where(d => d.Price == l.FirstOrDefault().Price).ToList();
            HandleCount(o, ll);
            if (o.State != OrderState.已成交)
            {
                Redo(o);
            }
        }

    }
}
