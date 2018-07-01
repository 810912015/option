using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Com.BitsQuan.Option.Core
{
    public enum ContractTimeSpanType
    {
        日=1,周=2,月=3,季=4,年=5
    }
    /// <summary>
    /// 合约：一种期权
    /// </summary>
    public class Contract:IEntityWithId
    {
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        [Key]
        public int Id { get; set; }
        /// <summary>
        /// 代码
        /// </summary>
        public string Code { get; set; }
        /// <summary>
        /// 名称
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// 合约类型:货币,期权,期货
        /// </summary>
        public ContractType ContractType { get; set; }
        /// <summary>
        /// 期权类型:认购期权,认沽期权
        /// </summary>
        public OptionType OptionType { get; set; }
        [ForeignKey("Coin")]
        public int CoinId { get; set; }
        /// <summary>
        /// 货币类型:人民币,比特币
        /// </summary>
        public virtual Coin Coin { get; set; }
        /// <summary>
        /// 合约单位:此合约包含了多少个虚拟币
        /// </summary>
        public decimal CoinCount { get; set; }
        /// <summary>
        /// 标的
        /// </summary>
        public string Target { get; set; }
        /// <summary>
        /// 行权日
        /// </summary>
        public DateTime ExcuteTime { get; set; }
        /// <summary>
        /// 行权价
        /// </summary>
        public decimal ExcutePrice { get; set; }

        /// <summary>
        /// 行权基准价
        /// </summary>
        public decimal ExcuteBasePrice { get; set; }
        /// <summary>
        /// 撤销订单排序认购字段
        /// </summary>
        public decimal GouPrice { get; set; }
        /// <summary>
        /// 撤销订单排序认股字段
        /// </summary>
        public decimal GuPrice { get; set; }
        /// <summary>
        /// 是否已不适用:当为true时表示该合约已被废弃或行权期已过
        /// </summary>
        public bool IsNotInUse { get; set; }

        public bool IsDel { get; set; }

        public ContractTimeSpanType TimeSpanType { get; set; }

        public override string ToString()
        {
            return string.Format("{0}-{1}", 
                
                Code, Name, ContractType);
        }
        public decimal GetMaintainForContract(decimal curPrice)
        {
            if (OptionType == OptionType.认购期权)
            {
                var btnVal = Coin.MainBailRatio*CoinCount * Coin.GetPrice();
                var m1 = (curPrice + btnVal);
                var m2 = curPrice * 3;
                return m1 >= m2 ? m1 : m2;
            }
            else//认沽保证金
            {
                var btnval = Coin.MainBailRatio*CoinCount *ExcutePrice;
                var m1 = (curPrice + btnval);
                var m3 = curPrice * 3;

                var m4 = m1 > m3 ? m1 : m3;

                var m2 = ExcutePrice*CoinCount;
                return m4>= m2 ? m2 : m4;
            }

        }

        public string GetDesc()
        {
            return string.Format("本合约为{0}类的{1},标的为{2},每份合约含{3}个{4},行权价为{5},到期日为{6}",
                ContractType, OptionType, Coin.Name, CoinCount,Coin.Name, ExcutePrice.ToString("C2"), ExcuteTime.ToString("yyyy-MM-dd"));
        }
    }
}
