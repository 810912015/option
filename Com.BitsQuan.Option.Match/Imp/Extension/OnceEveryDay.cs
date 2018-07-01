using System;

namespace Com.BitsQuan.Option.Match
{
    /// <summary>
    /// 操作每天仅执行一次
    ///     今天只执行一次
    /// </summary>
    public class OnceEveryDay
    {
        DateTime? Date { get; set; }
        /// <summary>
        /// 今天是否允许
        /// </summary>
        /// <returns></returns>
        public bool Should()
        {
            if (Date == null)
            {
                Date = DateTime.Now.Date;
                return true;
            }
            else
            {
                var d = (DateTime)Date;
                if (DateTime.Now.Date == d)
                {
                    return false;
                }
                else
                {
                    Date = DateTime.Now.Date;
                    return true;
                }
            }
        }
    }
}
