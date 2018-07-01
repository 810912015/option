using Com.BitsQuan.Option.Core;
using Com.BitsQuan.Option.Core.Spot;
using System;
using System.Collections.Generic;

namespace Com.BitsQuan.Option.Match.Spot
{

    /// <summary>
    /// 限价GFD:手工设定价格，未成交部分一直等待直至撤单。
    /// 限价申报 = 1
    /// 最初的撮合程序:价格匹配,平仓,时间优先
    /// </summary>
    public class SpotPrice : SpotArrange
    {
        public SpotPrice(Func<SpotOrder,bool, List<SpotOrder>> FindPossibleMatch,
            SpotOrderContainer Container,
            SpotModel model,
            Action<SpotOrder, SpotOrder> MakeDeal, Func<SpotOrder, bool> Redo)
            : base(FindPossibleMatch, Container, model, MakeDeal,Redo) { }
        public override void Match(SpotOrder o)
        {
            try
            {
                var r = FindPossibleMatch(o,false);
                HandleCount(o, r);
            }
            catch (Exception e)
            {
                Singleton<TextLog>.Instance.Error(e, "撮合异常");
            }
        }


    }
}
