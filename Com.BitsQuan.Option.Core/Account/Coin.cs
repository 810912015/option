using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace Com.BitsQuan.Option.Core
{
    /// <summary>
    /// 币种类型:人民币,比特币
    /// </summary>
    public class Coin:IEntityWithId
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int Id { get; set; }
        /// <summary>
        /// 货币名称
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// 维持保证金率:维持保证金计算公式中使用的币值比率
        /// </summary>
        public decimal MainBailRatio { get; set; }
        /// <summary>
        /// 维持保证金中使用的期权价值倍数
        /// </summary>
        public decimal MainBailTimes { get; set; }
        /// <summary>
        /// 合约代码中使用的代码
        /// </summary>
        public string CotractCode { get; set; }

        public override string ToString()
        {
            return Name;
        }


        /// <summary>
        /// 获取货币的人民币价值
        /// </summary>
        /// <param name="c"></param>
        /// <returns></returns>
        public decimal GetPrice()
        {
            switch (Name)
            {
                case "CNY":
                    return 1;
                case "BTC":
                    return BtcPrice.Current;
                case "LTC":
                    return 100;
            }
            return -1;
        }
    }
}
