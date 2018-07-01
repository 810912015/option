using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Com.BitsQuan.Option.Core
{
    /// <summary>
    /// 现金账户
    /// </summary>
    public class CacheAccount:IEntityWithId
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int Id { get; set; }
        
        /// <summary>
        /// 人民币
        /// </summary>
        public virtual Account CnyAccount { get; set; }
        
        /// <summary>
        /// 比特币
        /// </summary>
        public virtual Account BtcAccount { get; set; }
        /// <summary>
        /// 账户总金额:现金账户加上比特币折算人民币
        /// </summary>
        public decimal Total
        {
            get
            {
                return CnyAccount.Sum + BtcAccount.Sum * BtcPrice.Current;
            }
        }

        public override string ToString()
        {
            return string.Format("现金账户--{0}-{1}-总金额:{2}", CnyAccount, BtcAccount, Total);
        }
    }
}
