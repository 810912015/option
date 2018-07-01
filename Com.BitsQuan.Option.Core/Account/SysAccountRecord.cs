using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Com.BitsQuan.Option.Core
{
    public enum SysAccountChangeType
    {
        借款=1,提现=3,
        还款=2,充值=4,收取手续费=6,
        亏损分摊=7,
        手动调整_增加保证金=8,
        手动调整_增加现金=9,
        手动调整_减少保证金=10,
        手动调整_减少现金=11,
        推荐用户手续费返还_划出=12,
        推荐用户奖金_划出=13
    }
    public class SysAccountRecord : IEntityWithId
    {
        [Key]
        public int Id { get; set; }
        public DateTime When { get; set; }
        public decimal Delta { get; set; }

        public virtual Trader Who { get; set; }

        public virtual Order Order { get; set; }
        public SysAccountChangeType ChangedType { get; set; }
        public decimal TraderSum { get; set; }
        public decimal PrivateSum { get; set; }
        public decimal PublicSum { get; set; }
    }
}
