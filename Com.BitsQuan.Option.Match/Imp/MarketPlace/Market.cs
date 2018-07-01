using Com.BitsQuan.Option.Core;
using Com.BitsQuan.Option.Match.Imp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Timers;

namespace Com.BitsQuan.Option.Match
{
    /// <summary>
    /// 当前市场行情
    /// </summary>
    public class Market
    {
        public  Dictionary<string, MarketItem> Board { get;private  set; } 
         
        MarketDeals deals;
        IOptionModel model;
        MarketRecordSaver mrs;
        Timer t;
        public Market(IOptionModel model)
        {
            this.model = model;
            Board = new Dictionary<string, MarketItem>();

            var mr = new MarketRecordReader();
            var mrr = mr.Read();

            foreach (var v in model.Contracts.Where(a => a.IsDel == false))
            {
                var vr = mrr.Where(a => a.ContractId == v.Id).FirstOrDefault();
                if (vr == null)
                {
                    if(!Board.ContainsKey(v.Name))
                    Board.Add(v.Name, new MarketItem(v));
                }
                else
                {
                    if (!Board.ContainsKey(v.Name))
                    Board.Add(v.Name, new MarketItem(vr,v));
                }
            }
            deals = new MarketDeals(model);
            mrs = new MarketRecordSaver();
            t = new Timer();
            t.Interval = 30 * 1000;
            t.Elapsed += t_Elapsed;
            t.Start();
        }

        void t_Elapsed(object sender, ElapsedEventArgs e)
        {
            try
            {
                t.Stop();
                var q = Board.Values.Select(a => a.ToRecord()).ToList();
                foreach (var v in q)
                {
                    mrs.Save(v);
                }
            }
            catch (Exception ex)
            {
                Singleton<TextLog>.Instance.Error(ex);
            }
            finally
            {
                if (t != null)
                    t.Start();
            }
        }
        object oSync = new object();
        /// <summary>
        /// 当一个委托成交时更新行情,BuyPrice,BuyCount,SellPrice,SellCount
        /// </summary>
        /// <param name="o"></param>
        public void Handle(Order o)
        {

            if (!Board.ContainsKey(o.Contract.Name))
            {
                lock (oSync)
                {
                     if (!Board.ContainsKey(o.Contract.Name))
                         Board.Add(o.Contract.Name, new MarketItem(o.Contract)); 
                }
               
            }

            Board[o.Contract.Name].Handle(o); 
        }
        public void HandlePosition(List<UserPosition> ups, bool isAdd)
        {
            if (ups == null || ups.Count == 0 || ups[0].Order == null) return;
            var c=ups[0].Order.Contract.Name;
            if (!Board.ContainsKey(c)) return;
            Board[c].HandlePosition(ups, isAdd);
        }
      

        
        
        public void HandleDeal(Deal d)
        {
            deals.Handle(d);
            var c = model.Contracts.Where(a => a.Id == d.ContractId&&a.IsDel==false).First();
            if (c == null) return;
            if (!Board.ContainsKey(c.Name))
            {
                Board.Add(c.Name, new MarketItem(c)); 
            }
            Board[c.Name].Handle(d);
        }
        /// <summary>
        /// 获取一个合约的行情
        /// </summary>
        /// <param name="contractName">合约名称</param>
        /// <returns></returns>
        public MarketItem Get(string contractName)
        {
            if (contractName == null) return null;
            if (!Board.ContainsKey(contractName))
            {
                var c = model.Contracts.Where(a => a.Name==contractName&&a.IsDel==false).FirstOrDefault();
                if (c == null) return null;
                if (!Board.ContainsKey(c.Name))
                {
                    Board.Add(c.Name, new MarketItem(c));
                } 
            }
            return Board[contractName];
        }
        /// <summary>
        /// 获取合约的最新成交价
        /// </summary>
        /// <param name="contractName"></param>
        /// <returns></returns>
        public decimal GetNewestPrice(string contractName)
        {
            var m = Get(contractName);
            if (m == null) return 0;
            return m.NewestDealPrice;
        }

       
        public MarketDeals Deals
        {
            get { return deals; }
        }
    }
}
