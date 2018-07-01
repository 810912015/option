using System;
using System.Runtime.Serialization;

namespace Com.BitsQuan.Option.Core
{
    [DataContract]
    /// <summary>
    /// 用于用户显示的账户信息
    /// </summary>
    public class MyBailAccount
    {
        [DataMember]
        /// <summary>
        /// 实时保证金
        /// </summary>
        public decimal Total { get; set; }
        /// <summary>
        /// 维持保证金
        /// </summary>
        [DataMember]
        public decimal Maintain { get; set; }
        /// <summary>
        /// 可用保证金
        /// </summary>
        [DataMember]
        public decimal Usable { get; set; }
        /// <summary>
        /// 保证率
        /// </summary>
        [DataMember]
        public decimal Ratio { get; set; }
        [DataMember]
        public decimal Frozen { get; set; }

        public MyBailAccount(decimal total, decimal maintain, decimal usable, decimal ratio,decimal frozen)
        {
            this.Total = Math.Round(total, 2);
            this.Maintain = Math.Round(maintain, 2);
            this.Usable = Math.Round(usable, 2);
            this.Ratio = Math.Round(ratio, 2);
            this.Frozen = Math.Round(frozen, 2);
        }
        public MyBailAccount() { }
    }
}
