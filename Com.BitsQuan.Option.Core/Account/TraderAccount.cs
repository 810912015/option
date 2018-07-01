using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Com.BitsQuan.Option.Core
{
    /// <summary>
    /// 交易员资金账户
    /// </summary>
    public class TraderAccount:IEntityWithId
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int Id { get; set; }
         
        /// <summary>
        /// 现金账户
        /// </summary>
        public virtual CacheAccount CacheAccount { get; set; }
        
        /// <summary>
        /// 保证金账户
        /// </summary>
        public virtual BailAccount BailAccount { get; set; }
        
        /// <summary>
        /// 开仓保证率
        /// </summary>
        public static readonly string TAGMainBailRatio = "TAGMainBailRatio";

        public override string ToString()
        {
            return string.Format("账户信息--{0}-{1}", CacheAccount, BailAccount);
        }
        
    }
}
