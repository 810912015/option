using Com.BitsQuan.Option.Core;
using System.Collections.Generic;
using System.Linq;
using Com.BitsQuan.Option.Match.Imp;
using Com.BitsQuan.Option.Match;
using System.Collections.ObjectModel;
using System;

namespace Com.BitsQuan.Option.Imp
{  
    /// <summary>
    /// 下单后处理
    /// </summary>
    public class OrderPostHandler
    {
        TextLog log = new TextLog("OrderPostHandler.txt");
        /// <summary>
        /// 下单后,冻结维持保证金或需要的金额
        /// 同时要添加到用户的下单列表
        ///     下单列表只保留最近的10000单
        /// </summary>
        /// <param name="o"></param>
        public bool Handle(Order o)
        {
            try
            {
                log.Info(string.Format("下单后==下单订单信息:{0}", o));
                if (o.IsMarketPrice())
                {
                    o.Price = 0;
                }

                bool bailresult = true;
                //如果是市价,则不冻结也不解冻
                if (!o.IsMarketPrice() && o.IsNeedBailExceptMaintainBail())
                {
                    var c = o.GetSellOpenCountToFreeze();

                    bailresult = TraderService.OperateAccount(o.Trader, c, AccountChangeType.保证金冻结, "system", o);
                    log.Info(string.Format("下单后==保证金冻结成功,冻结{0}", o.GetNeededCache()));

                }
                if (bailresult)
                {
                    o.State = OrderState.等待中;
                    o.Trader.Orders().Add(o);


                    //成功下单,重新计算保证率
                    o.Trader.RaiseRatioChangedAfterOrder();

                    log.Info(string.Format("下单后==已将{0}添加到用户的下单列表", o));
                }
                return bailresult;
            }
            catch (Exception e)
            {
                Singleton<TextLog>.Instance.Error(e, "order post handle");
                return false;
            }
        }
    }
}
