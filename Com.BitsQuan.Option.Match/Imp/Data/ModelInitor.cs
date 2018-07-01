using Com.BitsQuan.Option.Core;
using Com.BitsQuan.Option.Match.Imp;
using Com.BitsQuan.Option.Provider;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Com.BitsQuan.Option.Match
{
    public class ModelInitor : IInitialbe
    {
        public List<Coin> coins { get; private set; }
        public ObservableCollection<Trader> traders { get; private set; }
        public List<Contract> contracts { get; private set; }
        public List<Deal> LatestDeals { get; private set; }
        public LegacyOrders legcy { get; private set; }
        public ModelInitor()
        {
            legcy = new LegacyOrders();
        }
        /// <summary>
        /// 从数据库读出数据至内存中
        /// </summary>
        void ReadDb()
        {
            Dictionary<int, Account> ad = new Dictionary<int, Account>();
            Dictionary<int, Coin> cd = new Dictionary<int, Coin>();
            Dictionary<int, BailAccount> bd = new Dictionary<int, BailAccount>();
            Dictionary<int, CacheAccount> cad = new Dictionary<int, CacheAccount>();
            Dictionary<int, TraderAccount> tad = new Dictionary<int, TraderAccount>();
            Dictionary<int, Contract> cond = new Dictionary<int, Contract>();
            Dictionary<int, Order> od = new Dictionary<int, Order>();
            Dictionary<int, Trader> td = new Dictionary<int, Trader>();
            //Dictionary<int, UserPosition> upd = new Dictionary<int, UserPosition>();
            using (var db = new OptionDbCtx())
            {
                foreach (var v in db.Set<Coin>())
                {
                    cd.Add(v.Id, new Coin
                    {
                        Id = v.Id,
                        CotractCode = v.CotractCode,
                        MainBailRatio = v.MainBailRatio,
                        MainBailTimes = v.MainBailTimes,
                        Name = v.Name
                    });
                }
                coins = cd.Values.ToList();
                foreach (var v in db.Set<Account>())
                {
                    if (!cd.ContainsKey(v.CacheType.Id)) continue;
                    var a = new Account { Id = v.Id, Frozen = v.Frozen, Sum = v.Sum, CacheType = cd[v.CacheType.Id] };
                    ad.Add(v.Id, a);
                }
                foreach (var v in db.Set<BailAccount>())
                {
                    if (!cd.ContainsKey(v.CacheType.Id)) continue;
                    bd.Add(v.Id, new BailAccount { Id = v.Id, Sum = v.Sum, Frozen = v.Frozen, CacheType = cd[v.CacheType.Id] });
                }

                foreach (var v in db.Set<CacheAccount>())
                {
                    if (!ad.ContainsKey(v.CnyAccount.Id)) continue;
                    cad.Add(v.Id, new CacheAccount { Id = v.Id, BtcAccount = ad[v.BtcAccount.Id], CnyAccount = ad[v.CnyAccount.Id] });
                }

                foreach (var v in db.Set<TraderAccount>())
                {
                    if (!cad.ContainsKey(v.CacheAccount.Id)) continue;
                    tad.Add(v.Id, new TraderAccount { Id = v.Id, BailAccount = bd[v.BailAccount.Id], CacheAccount = cad[v.CacheAccount.Id] });
                }

                foreach (var v in db.Set<Contract>().Where(a => a.IsNotInUse == false && a.IsDel == false))
                {
                    if (!cd.ContainsKey(v.Coin.Id))
                        continue;
                    cond.Add(v.Id, new Contract
                    {
                        Id = v.Id,
                        Code = v.Code,
                        CoinId = v.CoinId,
                        Coin = cd[v.Coin.Id],
                        ContractType = v.ContractType,
                        ExcutePrice = v.ExcutePrice,
                        ExcuteTime = v.ExcuteTime,
                        Name = v.Name,
                        OptionType = v.OptionType,
                        Target = v.Target,
                        IsDel = v.IsNotInUse,
                        CoinCount = v.CoinCount,
                        IsNotInUse = v.IsNotInUse
                    });
                }

                foreach (var v in db.Traders)
                {
                    if (!tad.ContainsKey(v.Account.Id)) continue;
                    td.Add(v.Id, new Trader
                    {

                        Id = v.Id,
                        Account = tad[v.Account.Id],
                        IsAutoAddBailFromCache = v.IsAutoAddBailFromCache,
                        IsAutoSellRight = v.IsAutoSellRight,
                        IsFrozen = v.IsFrozen,
                        Name = v.Name,
                        Positions = new List<UserPosition>()
                    });
                }

            }
            RestoreAccount(td);
            RestoreUndealOrders(od, td, cond);
            RestorePositionSummary(td, cond);
            RestoreDeals(cond);
            foreach (var v in od.Values)
            {
                td[v.Trader.Id].Orders().Add(v);
            }
            //orders = od;
            traders = new ObservableCollection<Trader>(td.Values);
            contracts = cond.Values.ToList();
        }
        void RestoreDeals(Dictionary<int, Contract> cond)
        {
            LatestDeals = new List<Deal>();
            var ks = cond.Keys.ToList();
            using (OptionDbCtx db = new OptionDbCtx())
            {
                var q = db.Deals.Where(a => ks.Contains(a.ContractId)).Take(1000);
                foreach (var v in q)
                {
                    LatestDeals.Add(v);
                }
            }
        }
        void RestoreUndealOrders(Dictionary<int, Order> od,
            Dictionary<int, Trader> td, Dictionary<int, Contract> cond)
        {
            FlushOrder fo = new FlushOrder();

            var k = fo.GetOldGeneration();
            if (k == null) return;
            string f = @"select * from temporders where 
id in(
select id from TempOrders where
 detail='{0}'  except select id from Orders)";

            string sql = string.Format(f, (int)k);

            using (var db = new OptionDbCtx())
            {
                var q = db.Database.SqlQuery<Order>(sql);
                foreach (var v in q)
                {
                    if (!cond.ContainsKey(v.ContractId)) continue;
                    if (!td.ContainsKey(v.TraderId)) continue;
                    var o = new Order
                    {
                        Id = v.Id,
                        State = v.State,
                        Count = v.Count,
                        Contract = cond[v.ContractId],
                        Direction = v.Direction,
                        DoneCount = v.DoneCount,
                        OrderPolicy = v.OrderPolicy,
                        OrderTime = v.OrderTime,
                        OrderType = v.OrderType,
                        Price = v.Price,
                        RequestStatus = v.RequestStatus,
                        Trader = td[v.TraderId],
                        TraderId = v.TraderId,
                        ContractId = v.ContractId,
                        Detail = v.Detail,
                        DonePrice = v.DonePrice,
                        IsBySystem = v.IsBySystem,
                        ReportCount = v.ReportCount,
                        TotalDoneCount = v.TotalDoneCount,
                        TotalDoneSum = v.TotalDoneSum,

                    };
                    if (o.State == OrderState.部分成交 || o.State == OrderState.等待中)
                    {
                        if (!o.IsBySystem)
                        {
                            legcy.Add(o);
                            od.Add(v.Id, o);
                            o.Trader.Orders().Add(o);
                        }
                    }

                }
            }


        }

        void RestoreAccount(Dictionary<int, Trader> td)
        {
            var sql = @"select * from AccountTradeRecords where id in(
select  max(id) from AccountTradeRecords
group by WhoId )";

            using (var db = new OptionDbCtx())
            {
                var rs = db.Database.SqlQuery<AccountTradeRecord>(sql);
                foreach (var v in rs)
                {
                    if (!td.ContainsKey(v.WhoId)) continue;
                    var t = td[v.WhoId];
                    t.Account.BailAccount.Sum = v.BailSum;
                    t.Account.BailAccount.Frozen = v.BailFrozen;
                    t.Account.CacheAccount.CnyAccount.Sum = v.CnySum;
                    t.Account.CacheAccount.CnyAccount.Frozen = v.CnyFrozen;
                    t.Account.CacheAccount.BtcAccount.Frozen = v.BtcFrozen;
                    t.Account.CacheAccount.BtcAccount.Sum = v.BtcSum;
                }
            }
        }

        void RestorePositionSummary(Dictionary<int, Trader> td, Dictionary<int, Contract> cond)
        {
            foreach (var v in td.Values)
            {
                v.Positions = new List<UserPosition>();
            }
            var sql = @"select * from PositionSummaryDatas where Id in
(select MAX(Id) from PositionSummaryDatas group by ContractId,TraderId)";

            using (var db = new OptionDbCtx())
            {
                var rs = db.Database.SqlQuery<PositionSummaryData>(sql);
                foreach (var v in rs)
                {
                    if (!cond.ContainsKey(v.ContractId)) continue;
                    if (!td.ContainsKey(v.TraderId)) continue;
                    v.Contract = cond[v.ContractId];
                    v.Trader = td[v.TraderId];
                    v.Trader.InitPosition(v);
                }
            }
        }

        public void Init()
        {
            ReadDb();
        }
    }
}
