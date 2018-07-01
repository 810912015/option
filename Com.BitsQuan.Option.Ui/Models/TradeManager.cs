using Com.BitsQuan.Option.Core;
using Com.BitsQuan.Option.Imp;
using Com.BitsQuan.Option.Match;
using Com.BitsQuan.Option.Match.Dto;
using Com.BitsQuan.Option.Match.Imp;
using Com.BitsQuan.Option.Provider;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;


namespace Com.BitsQuan.Option.Ui.Models
{
    [DataContract]
    [KnownType(typeof(OperationResult))]
    [KnownType(typeof(OrderDto))]
    public class OrderResultDto : OperationResult
    {
        [DataMember]
        public string UserOpId { get; set; }
        [DataMember]
        public OrderDto Order { get; set; }
    }

    [DataContract]
    [KnownType(typeof(OperationResult))]
    [KnownType(typeof(SpotOrderDto))]
    public class SpotOrderResultDto : OperationResult
    {
        [DataMember]
        public SpotOrderDto Spot { get; set; }
    }

    public class DealDto
    {
        public string When { get; set; }
        public decimal Price { get; set; }
        public int Count { get; set; }
        public string Type { get; set; }
        public DealDto(Deal a)
        {
            When = a.When.ToString("HH:mm:ss");
            Price = a.Price;
            Count = a.Count;
            Type = a.DealType.ToString();
        }
    }
    public class TradeManager : Com.BitsQuan.Option.Ui.Models.ITradeManager
    {
        public OrderResultDto OrderIt(int uid, string code, OrderPolicy policy,
            decimal price, int count, TradeDirectType direct,
            OrderType openclose, string userOpId)
        {
            if (count <= 0) return new OrderResultDto { ResultCode = 101, Desc = "数量不能小于等于0" };
            if (price == 0 && (policy != OrderPolicy.市价FOK && policy != OrderPolicy.市价IOC && policy != OrderPolicy.市价剩余转限价))
            {
                return new OrderResultDto { ResultCode = 102, Desc = "非市价策略价格不能为空" };
            }
            price = Math.Round(price, 2);
            var srv = MvcApplication.OptionService;
            var contract = srv.Model.Contracts.Where(a => a.Code == code && a.IsDel == false).FirstOrDefault();
            if (contract == null)
                return new OrderResultDto { ResultCode = 100, Desc = "无此合约代码" };
            //if (uid <= 0)
            //    return new OrderResultDto { ResultCode = 500, Desc = "必须登录" };
            var r = MvcApplication.OptionService.AddOrder(uid,
                contract.Id, direct,
                openclose, policy, count, price, userOpId);
            return new OrderResultDto
            {
                Desc = r.Desc,
                ResultCode = r.ResultCode,
                UserOpId = userOpId,
                Order = new OrderDto(r.Order)
            };
        }
        public List<OrderDto> GetMyOrders(int tid)
        {
            var os = MvcApplication.OptionService.Model.Traders.Where(a => a.Id == tid).FirstOrDefault();
            if (os == null) return new List<OrderDto>();
            var oso = os.Orders().Items.Select(a => new OrderDto(a)).ToList<OrderDto>();
            return oso;
        }

        int countPerPage = 10;

        public List<OrderDto> GetMyOrdersByTime_State(int tid, DateTime timeTop, DateTime timeEnd, string state, int pageIndex = 1)
        {

            OptionDbCtx db = new OptionDbCtx();
            using (BaseRepository<Trader> dr = new BaseRepository<Trader>())
            {
                var os = db.Traders.Where(a => a.Id == tid).FirstOrDefault();
                if (os == null) return new List<OrderDto>();
                if (state == "全部")
                {
                    return os.Orders().Items.Select(a => new OrderDto(a)).Where(b => Convert.ToDateTime(b.Time) > timeTop && Convert.ToDateTime(b.Time) < timeEnd).OrderBy(a => a.Id)
                       .Skip((pageIndex - 1) * countPerPage)
                       .Take(countPerPage).ToList();

                }
                else
                {
                    return os.Orders().Items.Select(a => new OrderDto(a)).Where(b => b.State == state && Convert.ToDateTime(b.Time) > timeTop && Convert.ToDateTime(b.Time) < timeEnd).OrderBy(a => a.Id)
                        .Skip((pageIndex - 1) * countPerPage)
                        .Take(countPerPage).ToList();
                }
            }
        }

        public int GetCount(int tid, DateTime timeTop, DateTime timeEnd, string state)
        {
            OptionDbCtx db = new OptionDbCtx();
            BaseRepository<Trader> dr = new BaseRepository<Trader>();
            var os = db.Traders.Where(a => a.Id == tid).FirstOrDefault();
            if (os == null) return 0;
            if (state == "全部")
            {
                return os.Orders().Items.Select(a => new OrderDto(a)).Where(b => Convert.ToDateTime(b.Time) > timeTop && Convert.ToDateTime(b.Time) < timeEnd).Count();

            }
            else
            {
                return os.Orders().Items.Select(a => new OrderDto(a)).Where(b => b.State == state && Convert.ToDateTime(b.Time) > timeTop && Convert.ToDateTime(b.Time) < timeEnd).Count();
            }
        }



        public List<PositionSummaryDto> GetMyPositions(int tid)
        {
            var os = MvcApplication.OptionService.Model.Traders.Where(a => a.Id == tid).FirstOrDefault();

            if (os == null) return new List<PositionSummaryDto>();

            var oso = os.GetPositionSummaries().Select(a => new PositionSummaryDto(a,
                MvcApplication.OptionService.MarketBoard.GetNewestPrice(a.CName), os.GetClosableCount(a)
                )).ToList();
            return oso;
        }
        public OperationResult Redo(int tid, int orderId)
        {
            var r = MvcApplication.OptionService.RedoOrder(tid, orderId);
            return r;
        }
        public D2Model GetOrdersInMarket(string contractCode, int count)
        {
            List<Order> d1 = null, d2 = null;
            if (MvcApplication.OptionService.Matcher.Container.Orders.ContainsKey(contractCode))
            {
                d1 = MvcApplication.OptionService.Matcher.Container.Orders[contractCode].SellQueue.OrderByDescending(_ => _.Price).ToList();
                d2 = MvcApplication.OptionService.Matcher.Container.Orders[contractCode].BuyQueue;
            }
            //防止买的比卖的高
            var minSell=d1 == null || d1.Count() == 0 ? 0 : d1.Min(a => a.Price);
            if (minSell > 0)
            {
                var tmp= d2.Where(a => a.Price < minSell);
                if (tmp == null)
                    d2 = new List<Order>();
                else d2 = tmp.ToList();
            }
            var m = new D2Model(d1, d2, count);
            return m;
        }
        public List<DealDto> GetDealsInMarket(string cname)
        {
            var r = MvcApplication.OptionService.MarketBoard.Deals.GetDealsByContract(cname)//.Where(a => a.When >= dt)
                   .Select(a =>
               new DealDto(a)).ToList();
            return r;
        }
        public MarketDto QueryMarket(string cname)
        {
            if (string.IsNullOrEmpty(cname))
                return new MarketDto();
            var m = MvcApplication.OptionService.MarketBoard.Get(cname);
            return new MarketDto(m, MvcApplication.OptionService.Matcher.Container.Get1PriceAndCount);
        }
        public List<Contract> GetContracts()
        {
            return MvcApplication.OptionService.Model.Contracts.Where(a => a.IsDel == false).ToList();
        }
        public MyAccount GetMyAccount(int uid)
        {
            var t= MvcApplication.OptionService.Model.Traders
                .Where(a => a.Id == uid).FirstOrDefault();
            if (t == null) return new MyAccount { Orignal = new TraderAccount(), CurBail = new MyBailAccount() };
            var ac = t.SnapshotBail();
            return new MyAccount { CurBail = ac, Orignal = t.Account };

        }
    }

    public class MyAccount
    {
        public TraderAccount Orignal { get; set; }
        public MyBailAccount CurBail { get; set; }
    }
}