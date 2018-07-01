using Com.BitsQuan.Option.Core.Spot;
using Com.BitsQuan.Option.Match.Imp;
using Com.BitsQuan.Option.Provider;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Com.BitsQuan.Option.Match.Spot
{
    public class SpotModel
    {
        public IOptionModel Model { get; private set; }

        public List<SpotOrder> SpotOrders { get; private set; }
        SpotDealSaver sds;
        SpotOrderSaver sos;
        TempSpotOrderSaver tos;
        public void Flush()
        {
            sds.Flush();
            sos.Flush();
            tos.Flush();
        }
        public SpotModel(IOptionModel model)
        {
            this.Model = model;
            SpotOrders = new List<SpotOrder>();
            sds = new SpotDealSaver();
            sos = new SpotOrderSaver();
            tos = new TempSpotOrderSaver();

            RestoreOrders();
        }

        public void RestoreOrders()
        {
            FlushSpotOrder fso = new FlushSpotOrder();
            var ok = fso.GetOldGeneration();
            if (ok == null) return;
            string sql =string.Format(@"select * from tempspotorders where id in 
(select id from tempspotorders where detail='{0}' except select id from spotorders)",(int)ok);
            using (var db = new OptionDbCtx())
            {
               
                var q = db.Database.SqlQuery<SpotOrder>(sql);
                foreach (var v in q)
                {
                    var coin = Model.Coins.Where(a => a.Id == v.CoinId).FirstOrDefault();
                    var trader = Model.Traders.Where(a => a.Id == v.TraderId).FirstOrDefault();
                    var so = new SpotOrder
                    {
                        Id =v.Id,  CoinId=v.CoinId, Count =v.Count, Detail =v.Detail,
                         Direction =v.Direction, DoneCount =v.DoneCount , DonePrice =v.DonePrice ,IsBySystem =v.IsBySystem,
                          OrderPolicy =v.OrderPolicy , OrderTime =v.OrderTime , Price =v.Price , ReportCount =v.ReportCount ,
                           RequestStatus =v.RequestStatus, State =v.State , TotalDoneCount=v.TotalDoneCount , TotalDoneSum =v.TotalDoneSum,
                            TraderId=v.TraderId,Coin =coin, Trader=trader
                    };
                    SpotOrders.Add(so);
                }
            }
        }
string f = "update TempSpotOrders set Count ={0},DoneCount ={1},TotalDoneCount ={2},DonePrice ={3},TotalDoneSum={4},State ={5},price={6} where Id ={7}";
        public void UpdatePartial(SpotOrder so)
        {
            Task.Factory.StartNew(() =>
            {
                string sql = string.Format(f, so.Count, so.DoneCount, so.TotalDoneCount, so.DonePrice, so.TotalDoneSum, (int)so.State, so.Price, so.Id);
                using (DBServer db = new DBServer())
                {
                    db.ExecNonQuery(sql);
                }
            });
        }

        public void SaveUndeal(SpotOrder so)
        {
            tos.Save(so);
        }
        public void SaveDeal(SpotDeal sd)
        {
            sds.Save(sd);
        }
        public void SaveOrder(SpotOrder so)
        {
            sos.Save(so);
        }
    }
}
