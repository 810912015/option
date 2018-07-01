using Com.BitsQuan.Option.Core;
using Com.BitsQuan.Option.Core.Spot;
using Com.BitsQuan.Option.Provider;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Com.BitsQuan.Option.Ui.Models
{
    #region order deal
    public abstract class OrderDealModelBase<T>
    {
        public List<T> Deals { get; set; }
        public bool HasNext { get; set; }
        public int PageIndex { get; set; }

        public OrderDealModelBase(Tuple<List<T>, bool> t, int pindex)
        {
            this.Deals = t.Item1; this.HasNext = t.Item2; this.PageIndex = pindex;
        }
    }
    public class OrderDealModel : OrderDealModelBase<OrderDeal>
    {
        public OrderDealModel(Tuple<List<OrderDeal>, bool> t, int pindex) : base(t, pindex) { }
    }
    public class SpotOrderDealModel : OrderDealModelBase<SpotOrderDeal>
    {
        public SpotOrderDealModel(Tuple<List<SpotOrderDeal>, bool> t, int pindex) : base(t, pindex) { }
    }
    public abstract class OrderDealBase
    {
        public Int64 Rid { get; set; }
        public int MainOrderId { get; set; }
        public TradeDirectType? Direction { get; set; }
        public DateTime? OrderTime { get; set; }

        public decimal? Price { get; set; }
        public OrderState? State { get; set; }
        public DateTime DealTime { get; set; }
        public decimal DealPrice { get; set; }
    }
    /// <summary>
    /// 委托成交记录
    /// </summary>
    public class OrderDeal : OrderDealBase
    {
        public OrderPolicy? OrderPolicy { get; set; }
        public OrderType? OrderType { get; set; }

        public int? ReportCount { get; set; }
        public int? Count { get; set; }
        public int DealCount { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
    }
    public class SpotOrderDeal : OrderDealBase
    {
        public decimal? ReportCount { get; set; }
        public decimal? Count { get; set; }
        public decimal DealCount { get; set; }
    }
    public abstract class DealReader<T> where T : OrderDealBase
    {
        OptionDbCtx db;
        public int CountPerPage { get; private set; }
        protected abstract string SPName { get; }

        public DealReader(OptionDbCtx dbctx, int countPerPage = 10)
        {
            this.CountPerPage = countPerPage;
            this.db = dbctx;
        }
        public DealReader() : this(new OptionDbCtx()) { }
        protected abstract void HandleMissing(T v);
        public Tuple<List<T>, bool> Query(string whoByName, int pageIndex, int CountPerPage = 10)
        {
            if (string.IsNullOrEmpty(whoByName) || pageIndex < 1) throw new ArgumentException("用户名不能为空且页码最小为1");
            List<T> data = Enumerable.Empty<T>().ToList();
            try
            {
                using (var db = new OptionDbCtx())
                {
                    data = db.Database.SqlQuery<T>(
                        "EXEC [dbo].[" + SPName + "] @p0,@p1,@p2",
                        whoByName, CountPerPage, (pageIndex - 1) * CountPerPage
                    ).ToList();
                }
            }
            catch (Exception ex)
            {
                Singleton<TextLog>.Instance.Error(ex);
                data = Enumerable.Empty<T>().ToList();
            }
            var hasNext = false;
            if (data != null)
            {
                foreach (var v in data)
                {
                    HandleMissing(v);
                }
                data = data.OrderByDescending(a => a.DealTime).ToList();
                hasNext = data == null ? false : data.Count >= CountPerPage;
            }
            return Tuple.Create<List<T>, bool>(data, hasNext);
        }
    }
    public class OrderDealReader : DealReader<OrderDeal>
    {
        public OrderDealReader(OptionDbCtx dbctx, int countPerPage = 10) : base(dbctx, countPerPage) { }
        public OrderDealReader() : this(new OptionDbCtx()) { }
        string sf = @"QueryUserOrderDeals";
        protected override string SPName
        {
            get
            {
                return sf;
            }
        }

        protected override void HandleMissing(OrderDeal v)
        {
            if (v.Direction != null) return;
            var tv = MvcApplication.OptionService.Matcher.Container.GetOrderById(v.MainOrderId);
            if (tv == null) return;
            v.Direction = tv.Direction;
            v.Count = tv.Count;
            v.OrderTime = tv.OrderTime;
            v.OrderType = tv.OrderType;
            v.OrderPolicy = tv.OrderPolicy;
            v.Price = tv.Price;
            v.ReportCount = tv.ReportCount;
            v.State = tv.State;
        }
    }

    public class SpotOrderDealReader : DealReader<SpotOrderDeal>
    {
        public SpotOrderDealReader(OptionDbCtx dbctx, int countPerPage = 10) : base(dbctx, countPerPage) { }
        public SpotOrderDealReader() : this(new OptionDbCtx()) { }
        string sf = @"QueryUserSpotDeals";
        protected override string SPName
        {
            get
            {
                return sf;
            }
        }

        protected override void HandleMissing(SpotOrderDeal v)
        {
            if (v.Direction != null) return;
            var tv = MvcApplication.SpotService.Model.SpotOrders.Where(a => a.Id == v.MainOrderId).FirstOrDefault();
            if (tv == null) return;
            v.Direction = tv.Direction;
            v.Count = tv.Count;
            v.OrderTime = tv.OrderTime;

            v.Price = tv.Price;
            v.ReportCount = tv.ReportCount;
            v.State = tv.State;
        }
    }
    #endregion


    #region history order

    public class HisModel<T>
    {
        public List<T> Orders { get; set; }
        public bool HasNext { get; set; }
        public int PageIndex { get; set; }

        public HisModel(List<T> os, int pageIndex)
        {
            this.Orders = os; this.HasNext = os.Count == 10;
            this.PageIndex = pageIndex;
        }
    }

    public class HisOrderModel : HisModel<Order>
    {
        public HisOrderModel(List<Order> os, int pageIndex) : base(os, pageIndex) { }
    }
    public class HisSpotOrderModel : HisModel<SpotOrder>
    {
        public HisSpotOrderModel(List<SpotOrder> os, int pageIndex) : base(os, pageIndex) { }
    }
    #endregion
}