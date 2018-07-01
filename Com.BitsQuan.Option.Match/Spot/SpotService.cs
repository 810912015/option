using Com.BitsQuan.Option.Core;
using Com.BitsQuan.Option.Core.Spot;
using Com.BitsQuan.Option.Imp;
using Com.BitsQuan.Option.Match.Imp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Com.BitsQuan.Option.Match.Spot
{
    public class SpotService : Com.BitsQuan.Option.Match.Spot.ISpotService
    {
        SpotModel model;
        SpotPre pre;
        SpotPost post;
        public SpotMarket Market { get; private set; }
        public SpotMatch match { get; private set; }
        SpotOrderCreator creator;
        public DeepDataPool3 DeepPool { get; private set; }
        public KlineDataPool KlinePool { get; private set; }
        OhlcGenarator ohlcGen;
        public SpotOrderContainer Container
        {
            get { return match.Container; }
        }
        public SpotModel Model
        {
            get { return model; }
        }

        public SpotService(IOptionModel om)
        {
            isStop = Com.BitsQuan.Miscellaneous.AppSettings.Read<bool>("isDisableBtcTrade", true);
            model = new SpotModel(om);
            pre = new SpotPre();
            post = new SpotPost();
            Market = new SpotMarket();
            match = new SpotMatch(model);
            creator=new SpotOrderCreator (model); 
            KlinePool =new KlineDataPool (
                new Func<int,Contract>((i)=>{
                    return new Contract{ Code="-2", Name="BTC"};
                }));
            KlineDataPoolInitor.InitSpot(KlinePool);
            ohlcGen = new OhlcGenarator();
            OhlcMaker.OnOhlcMaked += OhlcMaker_OnOhlcMaked;

            match.OnDeal += match_OnDeal;
            match.OnFinish += match_OnFinish;
            foreach (var v in model.SpotOrders.ToList())
            {
                match.Match(v); 
            }
            var qc=om.Coins .Where(a=>a.Name .ToLower()=="btc").FirstOrDefault();
            DeepPool = new DeepDataPool3(match.Container.Get(qc));
            SetMaxId();
        }

        void SetMaxId()
        {
            var sql = "select max(id) from tempspotorders";
            int mid=-1;
            using (DBServer db = new DBServer())
            {
                var r = db.ExcuteScale(sql);
                if (r is int)
                {
                    mid = (int)r;
                }
            }
            if(mid>0)
            IdService<SpotOrder>.Instance.SetCurrentOrderId(mid+1);
        }

        public IKlineData GetKlineDataByCoinId(int coinId)
        {
            var r= KlinePool.GetByConctractCode("-2");

            KlineDataDto rr;
            if (r == null)
            {
                rr = new KlineDataDto("-2", "BTC");

            }
            else
            {
                rr = new KlineDataDto(r);
            }

            var m5 = ohlcGen.GetCurrent(-2, OhlcType.M5);
            if (m5 != null)
                rr.M5.Add(m5);

            var m15 = ohlcGen.GetCurrent(-2, OhlcType.M15);
            if (m15 != null)
                rr.M15.Add(m15);
            var m30 = ohlcGen.GetCurrent(-2, OhlcType.M30);
            if (m5 != null)
                rr.M30.Add(m30);
            var m60 = ohlcGen.GetCurrent(-2, OhlcType.M60);
            if (m60 != null)
                rr.M60.Add(m60);
            var m1440 = ohlcGen.GetCurrent(-2, OhlcType.M1440);
            if (m1440 != null)
                rr.M1440.Add(m1440);
            var m480 = ohlcGen.GetCurrent(-2, OhlcType.M480);
            if (m480 != null)
                rr.M480.Add(m480);
            return rr;
        }
        public List<double> GetLatestKline(OhlcType type=OhlcType.M5)
        {
         
            var r = ohlcGen.GetCurrent(-2, type);
            return r;
        }
        void OhlcMaker_OnOhlcMaked(Ohlc obj)
        {
            KlinePool.Add(obj); 
        }
        bool isStop = false;
        public void Flush()
        {
            if (isStop) return;
            isStop = true;
            Thread.Sleep(30 * 1000);
            model.Flush();
            FlushSpotOrder fso = new FlushSpotOrder();
            fso.Flush((sb, nk) =>
            {
                foreach (var v in match.Container.Orders)
                {
                    foreach (var s in v.SellOrders)
                    {
                        fso.FlushTempOrder(sb, s, nk);
                    }
                    foreach (var b in v.BuyOrders)
                    {
                        fso.FlushTempOrder(sb, b, nk);
                    }
                }
            });
            Singleton<TextLog>.Instance.Info("虚拟币交易系统成功停止");
        }
    
        public SpotOrderResult AddOrder(int trader, int coinId, Core.TradeDirectType dir,OrderPolicy policy,
            decimal count, decimal price)
        {
            try
            {
                if (isStop) return new SpotOrderResult { ResultCode = 330,  Desc ="系统已停止,请稍后重试" };
                var r = creator.Create(trader, coinId, dir,policy, count, price);
                if (r.ResultCode != 0) return r;
                var pr = pre.CouldOrder(r.Spot);
                if (pr!=0)
                {
                    if(pr==1)
                    return new SpotOrderResult
                    {
                      
                        Spot = r.Spot,
                        ResultCode = 100,
                        Desc = "虚拟币不足"
                    };
                    else if(pr==2)
                        return new SpotOrderResult
                        {
                            Spot = r.Spot,
                            ResultCode = 100,
                            Desc = "现金资金不足"
                        };
                }
                post.Handle(r.Spot);
                match.Match(r.Spot);
                model.SaveUndeal(r.Spot);
                return new SpotOrderResult
                {
                    ResultCode = 0,
                    Spot = r.Spot,
                    Desc = "下单成功"
                };
            }
            catch (Exception e)
            {
                Singleton<TextLog>.Instance.Error(e, "spot");
                return new SpotOrderResult { ResultCode = 100, Desc = "服务器错误" };
            }
               
        }

        public bool Redo(int soId)
        {
            if (isStop) return false;
            var so = model.SpotOrders.Where(a => a.Id == soId).FirstOrDefault();
            if (so == null) return false;
            if (!so.IsArrangable()) return false;
            if (so.IsArranging()) return false;
            return match.Redo(so); 
        }

        void match_OnFinish(SpotOrder obj)
        {
            model.SaveOrder(obj); 
        }

        void match_OnDeal(SpotDeal obj)
        {
            Market.Get(obj.Coin).HandleDeal(obj);
            model.SaveDeal(obj);
            ohlcGen.Handle(obj);
        }
    }

    class FlushSpotOrder : FlushBase<SpotOrder>
    {

        public override void FlushTempOrder(System.Text.StringBuilder sb, SpotOrder o, int nk)
        {
            string sql = @" update TempSpotOrders set [Count] ={0},DoneCount ={1},TotalDoneCount ={2},[state]={3},DonePrice ={4},TotalDoneSum={5},Detail='{6}'
where Id={7} ;";
            sb.AppendFormat(sql, o.Count, o.DoneCount, o.TotalDoneCount, (int)o.State, o.DonePrice, o.TotalDoneCount, nk, o.Id);
        }

        protected override string GKey
        {
            get { return "spotGeneration"; }
        }
    }
}
