using Com.BitsQuan.Option.Core;

namespace Com.BitsQuan.Option.Imp
{
    public static class BailAccontExtension
    {
        
        /// <summary>
        /// 从保证金账户付钱
        ///     如果保证金账户钱不够,向系统资金账户借钱并计入系统亏损
        /// </summary>
        /// <param name="ba"></param>
        /// <param name="delta"></param>
        /// <returns></returns>
        //public static bool Pay(this BailAccount ba, decimal delta,Order o)
        //{
        //    if (ba.Sum < delta)
        //    {
        //        var needed = delta - ba.Sum;
        //        var r = SystemAccount.Instance.Borrow(delta, o);
        //    }
        //    return ba.Sub(delta);
        //}
        /// <summary>
        /// 往保证金中充钱
        /// </summary>
        /// <param name="ba"></param>
        /// <param name="delta"></param>
        /// <returns></returns>
        public static bool Collect(this BailAccount ba, decimal delta)
        {
            return ba.Add(delta);
        }
    }
}
