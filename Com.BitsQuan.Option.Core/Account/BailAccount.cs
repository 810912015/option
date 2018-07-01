
using System;
using System.Threading;
namespace Com.BitsQuan.Option.Core
{
    /// <summary>
    /// 保证金账户
    /// </summary>
    public class BailAccount : Account {
        public override string ToString()
        {
            return "保证金账户:"+ base.ToString();
        }
         
    }
}
