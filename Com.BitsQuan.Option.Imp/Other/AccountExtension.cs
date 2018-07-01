using Com.BitsQuan.Option.Core;

namespace Com.BitsQuan.Option.Imp
{
    public static class AccountExtension
    {
        /// <summary>
        /// 保证金是否足够
        /// </summary>
        /// <param name="a"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        public static bool IsBailSufficient(this TraderAccount a, decimal count)
        {
            return  a.BailAccount.Sum >= count*(1+TraderService.CommissionRatio);
        }
       
    }
}
