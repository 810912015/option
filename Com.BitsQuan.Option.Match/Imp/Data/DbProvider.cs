using Com.BitsQuan.Option.Core;
using Com.BitsQuan.Option.Provider;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
namespace Com.BitsQuan.Option.Match.Imp
{ 
    public class DbProvider : Com.BitsQuan.Option.Match.Imp.IDbModel
    {
        OrderSaver osaver = new OrderSaver();
        UserPositionSaver upsaver = new UserPositionSaver();
        DealSaver dealSaver = new DealSaver();
        TempOrderSaver tos;

        public DbProvider(LegacyOrders legcy)
        {
            tos = new TempOrderSaver(legcy);
        }

        public void UpdatePartialOrder(Order o)
        {
            Task.Factory.StartNew(() =>
            {
                string f = "update TempOrders set Count ={0},DoneCount ={1},TotalDoneCount ={2},DonePrice ={3},TotalDoneSum={4},State ={5},price={6} where Id ={7}";
                string sql = string.Format(f, o.Count, o.DoneCount, o.TotalDoneCount, o.DonePrice, o.TotalDoneSum, (int)o.State, o.Price, o.Id);
                using (DBServer db = new DBServer())
                {
                    db.ExecNonQuery(sql);
                }
            });
        }

        public void AddOrder(Order o)
        {
            tos.Save(o);
        }
        public void UpdateOrder(Order o)
        {
            osaver.Save(o);
        }
      
        public void AddContract(Contract c)
        {
            Task.Factory.StartNew(() =>
            {
                using (OptionDbCtx db = new OptionDbCtx())
                {
                    var coin = db.Set<Coin>().Find(c.Coin.Id);
                    if (coin != null) c.Coin = coin;
                    db.Contracts.Add(c);
                    db.SaveChanges();
                }
            });
        }
        void ExecuteSql(string sql)
        {
            var ds = new DBServer();
            ds.ExecNonQuery(sql);
            ds.Dispose();
            ds = null;
        }
        public void AddTrader(Trader t)
        {
            Task.Factory.StartNew(() =>
            {
                using (OptionDbCtx db = new OptionDbCtx())
                {
                    try
                    {
                        t.Account.BailAccount.CacheType = db.Set<Coin>().Find(t.Account.BailAccount.CacheType.Id);
                        t.Account.CacheAccount.CnyAccount.CacheType = db.Set<Coin>().Find(t.Account.CacheAccount.CnyAccount.CacheType.Id);
                        t.Account.CacheAccount.BtcAccount.CacheType = db.Set<Coin>().Find(t.Account.CacheAccount.BtcAccount.CacheType.Id);

                        db.Traders.Add(t);
                        db.SaveChanges();
                    }
                    catch (Exception e)
                    {
                        Singleton<TextLog>.Instance.Error(e, "add trader");
                    }
                } 
            });

        }
        public void UpdateTraderPosition(Trader t, List<UserPosition> ups, bool isAdd)
        {
            if (isAdd)
            {
                foreach (var v in ups)
                {
                    upsaver.Save(v);
                }
            }
            else
            {
                Task.Factory.StartNew(() =>
                {
                    string f1 = "update UserPositions set Count=Count-{0} where id={1}";
                    foreach (var v in ups)
                    {
                        string sq = string.Format(f1,v.Count, v.Id);
                        ExecuteSql(sq);
                    }
                });

            }
        }


        public void SaveDeal(Deal d)
        {
            dealSaver.Save(d);
        }

        public void Flush()
        {
            tos.Flush();
            osaver.Flush();
            upsaver.Flush();
            dealSaver.Flush();
        }

        public void Dispose()
        {
            if (osaver != null)
            {
                osaver.Flush();
                osaver.Dispose(); osaver = null;
            }
            if (upsaver != null)
            {
                upsaver.Flush();
                upsaver.Dispose(); upsaver = null;
            }
            if (dealSaver != null)
            {
                dealSaver.Flush();
                dealSaver.Dispose(); dealSaver = null;
            }
        }
    }
}
