using Com.BitsQuan.Option.Core;
using Com.BitsQuan.Option.Imp;
using Com.BitsQuan.Option.Match.Dto;
using Com.BitsQuan.Option.Match.Imp;
using Com.BitsQuan.Option.Match.Imp.Share;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Com.BitsQuan.Option.Match
{
    public class PositionSvcImp:SvcImpBase
    {
        public PosDataSaver pds { get; set; }
        Market market;
        public event Action<string, PositionSummaryDto> OnPositionSummaryChanged;

        public PositionSvcImp( Market market)
        {
            this.market = market;
        }

        public void UpdatePositionSummary(Trader obj, List<UserPosition> ups, bool isAdd)
        {
            LogError(() =>
            {
                lock (obj.Account.BailAccount)
                {
                    foreach (var v in ups)
                    {
                        try
                        {
                            var m = market.Get(v.Order.Contract.Name);
                            obj.Calc(v, isAdd, m == null ? 0 : m.NewestDealPrice);

                        }
                        catch (Exception e)
                        {
                            Singleton<TextLog>.Instance.Error(e, "UpdatePositionSummary");
                        }
                    }
                }
                if (OnPositionSummaryChanged != null && ups.Count > 0)
                {
                    var u = ups[0];
                    var ps = obj.GetPositionSummary(u.Order.Contract.Code,
                        u.Order.PositionType);
                    var v = ps;
                    var pd = new PositionSummaryData
                    {
                        OrderType = v.OrderType,
                        Commission = 0,
                        TotalValue = v.TotalValue,
                        Maintain = 0,
                        ClosableCount = obj.GetClosableCount(v),
                        FloatProfit = v.FloatProfit,
                        BuyTotal = v.BuyTotal,
                        BuyPrice = v.BuyPrice,
                        Count = v.Count,
                        PositionType = v.PositionType,
                        ContractId = v.Contract.Id,
                        CloseProfit = v.CloseProfit,
                        TraderId = obj.Id,
                        When = DateTime.Now
                    };
                    pds.Save(pd);

                    RaisePC(ps, obj);

                }
            });

        }
        public void RaisePC(PositionSummary p, Trader t)
        {
            try
            {
                if (p == null) return;
                var m = this.market.Get(p.CName);
                var np = m == null ? 0m : m.NewestDealPrice;
                var c = t.GetClosableCount(p);
                var pd = new PositionSummaryDto(p, np, c);
                if (OnPositionSummaryChanged != null)
                {
                    OnPositionSummaryChanged.BeginInvoke(t.Name, pd, null, null);
                }
            }
            catch (Exception ex)
            {
                Singleton<TextLog>.Instance.Error(ex, "raisepc");
            }
        }
    }
}
