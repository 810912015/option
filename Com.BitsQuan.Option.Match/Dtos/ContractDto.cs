using Com.BitsQuan.Option.Core;
using System;
using System.Runtime.Serialization;

namespace Com.BitsQuan.Option.Match.Dto
{
    /// <summary>
    /// 合约
    /// </summary>
    [DataContract]
    public class ContractDto
    {
        /// <summary>
        /// 代码
        /// </summary>
        [DataMember]
        public string Code { get; set; }
        /// <summary>
        /// 名称
        /// </summary>
        [DataMember]
        public string Name { get; set; }
        [DataMember]
        public ContractType ContractType { get; set; }
        /// <summary>
        /// 期权类型
        /// </summary>
        [DataMember]
        public string OptionType { get; set; }
        /// <summary>
        /// 标的
        /// </summary>
        [DataMember]
        public string Target { get; set; }
        /// <summary>
        /// 行权日
        /// </summary>
        [DataMember]
        public decimal ExcutePrice { get; set; }
        /// <summary>
        /// 行权价
        /// </summary>
        [DataMember]
        public string ExcuteTime { get; set; }
        [DataMember]
        public bool IsNotInUse { get; set; }
        /// <summary>
        /// 货币
        /// </summary>
        [DataMember]
        public string Coin { get; set; }
        /// <summary>
        /// 合约单位
        /// </summary>
        [DataMember]
        public decimal CoinCount { get; set; }

        public ContractDto(Contract c)
        {
            Code = c.Code;
            Name = c.Name;
            OptionType = c.OptionType.ToString();
            Target = c.Target;
            ExcutePrice = Math.Round(c.ExcutePrice, 2);
            ExcuteTime = c.ExcuteTime.ToString("yyyy-MM-dd HH:mm:ss");
            IsNotInUse = c.IsNotInUse;
            Coin = c.Coin.Name;
            CoinCount = c.CoinCount;
            this.ContractType = c.ContractType;
        }
    }
}
