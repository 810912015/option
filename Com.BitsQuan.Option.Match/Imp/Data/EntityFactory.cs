using Com.BitsQuan.Option.Core;
using Com.BitsQuan.Option.Imp;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Com.BitsQuan.Option.Match
{
    public class EntityFactory
    {
        public static Coin AddCoin(string name, string contractCode, decimal mainBailRatio, decimal mainBailTimes)
        {
            Coin c = new Coin
            {
                Id = IdService<Coin>.Instance.NewId(),
                Name = name,
                CotractCode = contractCode,
                MainBailRatio = mainBailRatio,
                MainBailTimes = mainBailTimes
            };

            return c;
        }
        public static Contract CreateContract(string coinName, DateTime exeTime, decimal exePrice, OptionType optionType,
           string target, decimal coinCount)
        {
            if (exePrice <= 0 || exeTime <= DateTime.Now) return null;
            var coin = CoinRepo.Instance.GetByName(coinName);
            if (coin == null) return null;
            var c = new Contract
            {
                Id = IdService<Contract>.Instance.NewId(),
                ExcutePrice = exePrice,
                ExcuteTime = exeTime,
                Coin = coin,
                ContractType = ContractType.期权,
                OptionType = optionType,
                Target = coin.Name,
                CoinCount = coinCount,
                TimeSpanType = GetTsType(exeTime)
            };
            c.SetCodeAndName();

            return c;
        }
        static ContractTimeSpanType GetTsType(DateTime exeTime)
        {
            var dt = exeTime.Subtract(DateTime.Now);
            if (dt.TotalDays >= 365) return ContractTimeSpanType.年;
            else if (dt.TotalDays >= 90) return ContractTimeSpanType.季;
            else if (dt.TotalDays > 15) return ContractTimeSpanType.月;
            else if (dt.TotalHours > 30) return ContractTimeSpanType.周;
            else return ContractTimeSpanType.日;
        }
        static readonly object createOrderSync = new object();
        public static Order CreateOrder(Trader who, Contract contract, TradeDirectType dir, OrderType orderType, OrderPolicy policy, int count, decimal price1)
        {
            var w = who;
            if (w == null) return null;
            var c = contract;
            if (c == null) return null;
            if (count <= 0) return null;
            var price = Math.Round(price1, 2);
            if (price < 0) return null;
            if (price == 0 && (policy != OrderPolicy.市价剩余转限价 && policy != OrderPolicy.市价IOC && policy != OrderPolicy.市价FOK))
                return null;
            Order o;
            lock (createOrderSync)
            {
                o = new Order
                {
                    Id = IdService<Order>.Instance.NewId(),
                    Contract = c,
                    Trader = w,
                    Count = count,
                    ReportCount = count,
                    Direction = dir,
                    Price = price,
                    OrderType = orderType,
                    OrderPolicy = policy,
                    State = OrderState.等待中,
                    OrderTime = DateTime.Now
                };
            }
            return o;
        }
        public static Trader CreateTrader(string name)
        {
            var t = new Trader
            {
                Id = IdService<Trader>.Instance.NewId(),
                Name = name,
                IsAutoAddBailFromCache = false,
                IsAutoSellRight = false,
                //Orders = new ObservableCollection<Order>(),
                Positions = new List<UserPosition>(),
                Account = new TraderAccount
                {
                    Id = IdService<TraderAccount>.Instance.NewId(),
                    BailAccount = new BailAccount
                    {
                        Id = IdService<Account>.Instance.NewId(),
                        CacheType = CoinRepo.Instance.CNY,
                        Sum = MatchParams.UseFake ? MatchParams.FakeBail : 0,
                        Frozen = 0
                    },
                    CacheAccount = new CacheAccount
                    {
                        Id = IdService<CacheAccount>.Instance.NewId(),
                        CnyAccount = new Account
                        {
                            Id = IdService<Account>.Instance.NewId(),
                            CacheType = CoinRepo.Instance.CNY,
                            Sum = MatchParams.UseFake ? MatchParams.FakeCny : 0,
                            Frozen = 0
                        },
                        BtcAccount = new Account
                        {
                            Id = IdService<Account>.Instance.NewId(),
                            CacheType = CoinRepo.Instance.BTC,
                            Sum = MatchParams.UseFake ? MatchParams.FakeBtc : 0,
                            Frozen = 0
                        }

                    }
                },

            };
            return t;
        }
    }
}
