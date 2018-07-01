using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.Entity.Migrations;
using Com.BitsQuan.Option.Core;
using System.Threading;

namespace Com.BitsQuan.Option.Provider.Migrations
{
    public enum DbInitType { Test,Init}
    public interface IDbInit
    {
        DbInitType Type { get; }
        void Init(DbContext db);
    }
     
    public class TraderInitor:IDbInit
    {
        public DbInitType Type
        {
            get { return DbInitType.Test; }
        }

        public void Init(DbContext db)
        {
            try
            {
                var c1= new Coin { Id =1, Name ="CNY", MainBailRatio=1 ,MainBailTimes =1,CotractCode="9"};
                var c2= new Coin { Id =2, Name ="BTC",MainBailRatio =0.1m,MainBailTimes =3,CotractCode ="1"};
                var c3 = new Coin { Id = 3, Name = "LTC", MainBailRatio = 0.2m, MainBailTimes = 4, CotractCode = "2" };
                db.Set<Coin>().AddOrUpdate(a => a.Id, c1, c2, c3);
                int id = 1;
                List<Trader> l = new List<Trader>();

                for (int i = -2; i<0; i++)
                {
                    var tid = i + 1;
                    Trader t = new Trader
                    {
                        Id = tid,
                        Name = "robot" + i*(-1),
                        Account = new TraderAccount
                        {
                            Id = tid,
                            BailAccount = new BailAccount { Id = id++, CacheType = c1, Sum = 500000 },
                            CacheAccount = new CacheAccount
                            {
                                Id = tid,
                                CnyAccount = new Account { Id = id++, CacheType = c1, Sum = 500000 },
                                BtcAccount = new Account { Id = id++, CacheType = c1, Sum = 5000 }
                            }
                        }
                    };
                    l.Add(t);
                }
                var r = Com.BitsQuan.Miscellaneous.AppSettings.Read<bool>("hasHello", false);
                if (r)
                {
                    for (int i = 0; i < 100; i++)
                    {
                        var tid = i + 1;
                        Trader t = new Trader
                        {
                            Id = tid,
                            Name = "hello" + i,
                            Account = new TraderAccount
                            {
                                Id = tid,
                                BailAccount = new BailAccount { Id = id++, CacheType = c1, Sum = 1000000 },
                                CacheAccount = new CacheAccount
                                {
                                    Id = tid,
                                    CnyAccount = new Account { Id = id++, CacheType = c1, Sum = 1000000 },
                                    BtcAccount = new Account { Id = id++, CacheType = c2, Sum = 1000 }
                                }
                            }
                        };
                        l.Add(t);
                    }
                }

                db.Set<Trader>().AddOrUpdate(l.ToArray());
                db.SaveChanges();
            }
            catch (Exception e)
            {
                Singleton<TextLog>.Instance.Error(e, "trader init");
            }
        }
    }


    public class ContractInitor : IDbInit
    {
        int id = 1;
        Coin c2 = new Coin { Id = 2, Name = "BTC", MainBailRatio = 0.1m, MainBailTimes = 3, CotractCode = "1" };
        Contract Create(string coinName, DateTime exeTime, decimal exePrice, OptionType optionType, string target)
        {
            var c = new Contract
            {
                Id = id++,
                ExcutePrice = exePrice,
                ExcuteTime = exeTime,
                CoinId = 2,
                ContractType = ContractType.期权,
                OptionType = optionType,
                Target = target,
                CoinCount=1, TimeSpanType =GetTsType(exeTime)
            };
            SetCodeAndName(c); 
            return c;
        }
        ContractTimeSpanType GetTsType(DateTime exeTime)
        {
            var dt = exeTime.Subtract(DateTime.Now);
            if (dt.TotalDays >= 365) return ContractTimeSpanType.年;
            else if (dt.TotalDays >= 90) return ContractTimeSpanType.季;
            else if (dt.TotalDays >= 30) return ContractTimeSpanType.月;
            else if (dt.TotalDays >= 7) return ContractTimeSpanType.周;
            else return ContractTimeSpanType.日;
        }
        int DutyIdOfThisYear=0;
        /// <summary>
        /// 累加的当年认购(权利仓)编号
        /// </summary>
        int RightIdOfThisYear=0;
        public string GenerateOptionContractCode(Coin coinType, ContractType contractType, DateTime year,
            OptionType optionType, ContractTimeSpanType timeSpanType)
        {
            if (contractType == 0 || optionType == 0)
                throw new ArgumentException("产生期权代码时货币类型或合约类型或持仓类型不能为0");
            if (optionType == OptionType.认购期权)
            {
                if (RightIdOfThisYear == 0) Interlocked.Exchange(ref RightIdOfThisYear, 1);
                else
                    Interlocked.Add(ref RightIdOfThisYear, 2);
            }

            if (optionType == OptionType.认沽期权)
            {
                if (DutyIdOfThisYear == 0) Interlocked.Exchange(ref DutyIdOfThisYear, 2);
                else
                    Interlocked.Add(ref DutyIdOfThisYear, 2);
            }
            return string.Format("{0}{1}{2}{3}{4:D4}",
                timeSpanType,
                 coinType.CotractCode,
                (int)contractType - 1,
                contractType == ContractType.货币 ? 0 : year.Year % 10,
                contractType == ContractType.货币 ? 0 :
                optionType == OptionType.认购期权 ? RightIdOfThisYear :
                DutyIdOfThisYear);
        }

        /// <summary>
        /// 生成期权合约名称
        /// </summary>
        /// <param name="coinType"></param>
        /// <param name="executeTime"></param>
        /// <param name="optionType"></param>
        /// <param name="executePrice"></param>
        /// <returns></returns>
        string GenerateOptionContractName(DateTime executeTime, OptionType optionType, decimal executePrice)
        {
            if (optionType == 0 || executePrice < 0 || executeTime < DateTime.Now)
                throw new ArgumentException("期权货币类型,持仓类型错误或行权价为0或行权日已过");
            return string.Format("{0}{1}{2}{3}", c2.Name, executeTime.ToString("yyyyMMdd"),
                optionType == OptionType.认沽期权 ? "沽" : "购", (int)executePrice);
        }
        void SetCodeAndName(Contract entity)
        {
            if (entity.Code == null)
                entity.Code =GenerateOptionContractCode(c2,
                               entity.ContractType, entity.ExcuteTime, entity.OptionType,entity.TimeSpanType);
            if (entity.Name == null)
                entity.Name = GenerateOptionContractName(entity.ExcuteTime, entity.OptionType, entity.ExcutePrice);
        }


        public DbInitType Type
        {
            get { return DbInitType.Test; }
        }

        public void Init(DbContext db)
        {
            var l = new List<Contract>();
            for (int i = 0; i < 10; i++)
            {
                var r = Create("btc", DateTime.Now.AddMonths(i + 1), 100, OptionType.认购期权, "比特币");
                l.Add(r);
            }

            for (int i = 0; i < 10; i++)
            {
                var r = Create("btc", DateTime.Now.AddMonths(i + 1), 100, OptionType.认沽期权, "比特币");
                l.Add(r);
            }
            db.Set<Contract>().AddOrUpdate(a => a.Id, l.ToArray());
            db.SaveChanges();
        }
    }

  }
