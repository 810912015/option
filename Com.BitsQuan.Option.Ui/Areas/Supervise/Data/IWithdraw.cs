using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Com.BitsQuan.Option.Ui.Areas.Supervise.Data
{
    interface IWithdraw
    {
        /// <summary>
        /// 比特币提现
        /// </summary>
        /// <param name="name">提现用户</param>
        /// <param name="count">提现数量</param>
        /// <param name="address">提现地址(用户提供给我们)</param>
        /// <returns></returns>
        bool BtcWithdraw(string name, int count, string address);
    }
}
