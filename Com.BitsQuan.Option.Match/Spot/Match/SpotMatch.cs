using Com.BitsQuan.Option.Core;
using Com.BitsQuan.Option.Core.Spot;
using Com.BitsQuan.Option.Imp;
using System;
using System.Collections.Generic;

namespace Com.BitsQuan.Option.Match.Spot
{
    

    public class SpotMatch
    {
        public SpotOrderContainer Container { get; private set; }
        
        SpotModel model;
        SpotArranger arranger;
        public SpotMatch(SpotModel sm)
        {
            this.model = sm;
            Container = new SpotOrderContainer((a) => {
                if (this.OnFinish != null) OnFinish(a);
            });

            arranger = new SpotArranger(FindPossibleMatch, Container, model, MakeDeal,Redo);
        }
        List<SpotOrder> FindPossibleMatch(SpotOrder so,bool ignorePrice)
        {
            var c = Container.Get(so.Coin);
            return c.FindOpposite(so,ignorePrice);
        }
        public void Match(SpotOrder so)
        {
            arranger.Match(so);
        }
        void MakeDeal(SpotOrder main, SpotOrder slave)
        {
            var count = Math.Min(main.Count, slave.Count);
            var price = slave.Price;
            var total = count * price;
            //设置成交价格和成交数量
            main.DonePrice = slave.DonePrice = price;
            main.DoneCount = slave.DoneCount = count;

            main.TotalDoneCount += count;
            slave.TotalDoneCount += count;

            main.Count -= count;
            slave.Count -= count;

            main.TotalDoneSum += total;
            slave.TotalDoneSum += total;
            //设置状态
            main.State = main.IsDone() ? OrderState.已成交 : OrderState.部分成交;
            slave.State = slave.IsDone() ? OrderState.已成交 : OrderState.部分成交;
            //从容器中移除如果已经成交
            if (slave.IsDone())
                Container.Remove(slave);
            //处理账户和金额
            HandleCache(main);
            HandleCache(slave);
            //引发事件
            RaiseDeal(main, slave);
            RaiseFinish(main);
            RaiseFinish(slave); 
        }
        
        public event Action<SpotDeal> OnDeal;
        void RaiseDeal(SpotOrder main, SpotOrder slave)
        {
            SpotDeal sd = new SpotDeal
            {
                Id = IdService<SpotDeal>.Instance.NewId(),
                When=DateTime.Now,
                 Count=main.DoneCount,
                 Price =main.DonePrice,
                  MainId =main.Id ,
                   SlaveId=slave.Id,
                    MainTraderName=main.Trader.Name,
                     SlaveTraderId=slave.Trader.Name,
                      Coin=main.Coin,
                       CoinId=main.CoinId,
                        MainOrderDir=main.Direction
            };
            if (OnDeal != null)
                OnDeal(sd);
        }
        //TextLog log = new TextLog("jd.txt");
        void HandleCache(SpotOrder so)
        {
            so.UnFreeze();
            if (so.Direction == TradeDirectType.卖)
            {
                var ca=so.Trader.Account.CacheAccount.BtcAccount;
                
                //收款
                TraderService.OperateAccount(so.Trader, so.DonePrice * so.DoneCount, AccountChangeType.现金收款, so.Trader.Name, null);
                
                TraderService.OperateAccount(so.Trader, so.DoneCount, AccountChangeType.BTC付款, so.Trader.Name, null);
            }
            else
            {
              
                TraderService.OperateAccount(so.Trader, so.DoneCount * so.DonePrice, AccountChangeType.现金付款, so.Trader.Name, null);
               
                TraderService.OperateAccount(so.Trader, so.DoneCount, AccountChangeType.BTC收款, so.Trader.Name, null);

            }
        }


       

        public bool Redo(SpotOrder so)
        {
            if (so == null) return false;
            if (!so.IsArrangable()) return false;
            if (so.IsArranging()) return false;
            so.State = OrderState.已撤销;
            var r = Container.Remove(so);

            so.UnFreeze();
            if (OnFinish != null)
                foreach (var v in OnFinish.GetInvocationList())
                {
                    ((Action<SpotOrder>)v)(so);
                }
            return r;
        }

        public event Action<SpotOrder> OnPartialFinish;
        public event Action<SpotOrder> OnFinish;
        void RaisePartialFinish(SpotOrder so)
        {
            if (so.State == OrderState.部分成交 && OnPartialFinish != null)
                foreach (var v in OnPartialFinish.GetInvocationList())
                {
                    ((Action<SpotOrder>)v)(so);
                } 
        }
        void RaiseFinish(SpotOrder so)
        {
            if (so.IsDone())
            {
                so.Trader.RemoveSpotOrder(so);
                model.SpotOrders.Remove(so);
                if (OnFinish != null)
                    foreach (var v in OnFinish.GetInvocationList())
                    {
                        ((Action<SpotOrder>)v)(so);
                    }
            }
            else
            {
                model.UpdatePartial(so);
                RaisePartialFinish(so);
            }
                
        }
    }
}
