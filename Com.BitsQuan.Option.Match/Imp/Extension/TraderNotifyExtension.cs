using Com.BitsQuan.Option.Core;
using Com.BitsQuan.Option.Imp;
using Com.BitsQuan.Option.Match.Imp;
using System;
using System.Threading;

namespace Com.BitsQuan.Option.Match
{

    public static class TraderNotifyExtension
    {
        static MyProperty<Trader, OnceEveryDay> tMsgDay1 = new MyProperty<Trader, OnceEveryDay>();
        static MyProperty<Trader, OnceEveryDay> tEmaioDay1 = new MyProperty<Trader, OnceEveryDay>();
        static MyProperty<Trader, OnceEveryDay> tInstant = new MyProperty<Trader, OnceEveryDay>();
        public static bool ShouldInstantMsg(this Trader t)
        {
            var oed = tInstant.Get(t);
            if (oed == null)
            {
                oed = new OnceEveryDay();
                tInstant.Set(t, oed);
            }
            return oed.Should();
        }
        /// <summary>
        /// 是否应该发送短信
        ///     一天仅发送一条
        /// </summary>
        /// <param name="t"></param>
        /// <returns></returns>
        public static bool ShouldMsg(this Trader t)
        {
            var oed = tMsgDay1.Get(t);
            if (oed == null)
            {
                oed = new OnceEveryDay();
                tMsgDay1.Set(t, oed);
            }
            return oed.Should();
        }
        /// <summary>
        /// 是否应该发送邮件
        ///     一天仅发送一封
        /// </summary>
        /// <param name="t"></param>
        /// <returns></returns>
        public static bool ShouldEmail(this Trader t)
        {
            var oed = tEmaioDay1.Get(t);
            if (oed == null)
            {
                oed = new OnceEveryDay();
                tEmaioDay1.Set(t, oed);
            }
            return oed.Should();
        }

        static TextLog log = new TextLog("traderMsgAndEmail.txt");
        public static void SendEmail(this Trader t, string msg)
        {
            if (t.ShouldEmail())
            {
                log.Info(string.Format("邮件|{0}:{1}", t.Name, msg));
            }
        }
        public static void SendMsg(this Trader t, string msg)
        {
            if (t.ShouldMsg())
            {
                log.Info(string.Format("短信|{0}:{1}", t.Name, msg));
            }
        }
    }
}
