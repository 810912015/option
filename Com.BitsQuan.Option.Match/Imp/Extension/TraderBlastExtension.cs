using Com.BitsQuan.Option.Core;

namespace Com.BitsQuan.Option.Match.Imp
{
    public static class TraderBlastExtension
    {
        /// <summary>
        /// 账户是否已被监视
        /// </summary>
        static BooleanProperty<int> monitoring = new BooleanProperty<int>();
        /// <summary>
        /// 是否已被监视
        /// </summary>
        /// <param name="t"></param>
        /// <returns></returns>
        public static bool IsMonitoring(this Trader t)
        {
            return monitoring.Get(t.Id);
        }
        /// <summary>
        /// 设置监视
        /// </summary>
        /// <param name="t"></param>
        /// <param name="isMonitoring"></param>
        public static void SetMonitoring(this Trader t, bool isMonitoring)
        {
            monitoring.Set(t.Id, isMonitoring);
        }

        static BooleanProperty<int> blasting = new BooleanProperty<int>();

        public static bool IsBlasting(this Trader t)
        {
            return blasting.Get(t.Id);
        }
        public static void SetBlasting(this Trader t, bool isBlasting)
        {
            blasting.Set(t.Id, isBlasting);
        }

    }
}
