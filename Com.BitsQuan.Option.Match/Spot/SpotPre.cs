using Com.BitsQuan.Option.Core;
using Com.BitsQuan.Option.Core.Spot;
using System.Linq;

namespace Com.BitsQuan.Option.Match.Spot
{

    public class SpotPre
    {
        TextLog log = new TextLog("sellqian.txt");
        public int CouldOrder(SpotOrder so)
        {
            var cc = CheckCount(so);
            if (!cc) return 1;
            var cm = CheckMoney(so);
            if (!cm) return 2;
            return 0;             
        }
        /// <summary>
        /// 卖:只能卖有的,且一个只能卖一次
        /// 买无限制
        /// </summary>
        /// <param name="so"></param>
        /// <returns></returns>
        bool CheckCount(SpotOrder so)
        {
            if (so.Direction == TradeDirectType.卖)
            {
                
                var cc = so.Trader.Account.CacheAccount.BtcAccount.Sum;//.GetCoinAcount(so.Coin).Balance;(可用的)
                log.Info(string.Format("实际:{0},我卖:{1}", cc, so.Count));
                if (cc < so.Count)
                {
                    return false;
                }
                var soc = so.Trader.GetSpotOrders();
                decimal ocs = 0;
                log.Info(string.Format("soc的结果:{0}", soc));
                log.Info(string.Format("比特币的可用资金:{0}", cc));
                if(soc!=null)
                {
                    //注：卖出去的马上就会被冻结（相当于不是我的），为什么还要用可用的减去卖出去的BTC（相当于要扣除了一次BTC）
                    ocs = soc.Where(a => a.Coin == so.Coin && a.Direction == TradeDirectType.卖 && a.State == OrderState.等待中 || a.State == OrderState.部分成交).Select(a => a.Count).Sum();
                  //var kk =  soc.Where(a => a.Coin == so.Coin && a.Direction == TradeDirectType.卖).ToList();
                  //  log.Info(string.Format("soc里面SUM:{0}///{1}", ocs,kk.Count()));


                  //  foreach (var item in kk)
                  //  {
                  //      log.Info(string.Format("数据:{0},{1},{2},{3}", item.Id, item.Direction, item.State,kk.Count()));
                  //  }
                  //  var r = cc - ocs - so.Count;
                      var r = cc- so.Count;
                  //log.Info(string.Format("soc里面计算:{0}={1} - {2}-{3}", r, cc,ocs , so.Count));
                    if (r >= 0) return true;
                    else return false;
                }
                return true;
            }
            return true;
        }
        /// <summary>
        /// 买:只有钱够才能买
        /// 卖:无限制
        /// </summary>
        /// <param name="so"></param>
        /// <returns></returns>
        bool CheckMoney(SpotOrder so)
        {
            if (so.Direction == TradeDirectType.卖) return true;
            var needed =so.GetTotal();// so.Count * so.Price;
            if (so.Trader.Account.CacheAccount.CnyAccount.Sum >= needed)
            {
                return true;
            }
            else {
                return false; }
        }
    }
}
