using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Com.BitsQuan.Option.Ui.Areas.Supervise.Data
{
    interface IRecharge
    {
        /// <summary>
        /// BTC充值
        /// </summary>
        /// <param name="name">充值用户</param>
        /// <param name="count">充值数量</param>
        /// <param name="address">充值地址（我们提供给用户）</param>
        /// <returns></returns>
        bool BtcRecharge(string name, decimal count, string address);
    }
}
