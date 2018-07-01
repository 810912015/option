using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;

namespace Com.BitsQuan.Option.Ui.Areas.Supervise.Data
{
    public class Withdraw:IWithdraw
    {
        public bool BtcWithdraw(string name, int count, string address)
        {
            if (name != "hello9") {
                return false;
            }

          if(address!="地址格式"){
        　    return false;
          }

            return true;
        }
    }
}