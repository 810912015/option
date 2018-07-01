using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Com.BitsQuan.Option.Ui.Areas.Supervise.Data
{
    public class Recharge:IRecharge
    {
        public class Account{
            static public decimal BTC = 100;
            public Account()
            {
              
                
            }

            public static decimal GetBtc(decimal count)
            {
               return BTC += count;
            }
        }

        public bool BtcRecharge(string name, decimal count, string address)
        {

            ////1.充值地址与我所提供的地址不一致
            //if (name!="hello9" || address != "地之一")
            //{
            //    return false;
            //}

            //decimal btc=Account.GetBtc(count);
            ////2.我方的账户资金没有增加
            //if (count <= 0 || Account.BTC != btc)
            //{
            //    return false;
            // }
            return true;
        }
    }
}