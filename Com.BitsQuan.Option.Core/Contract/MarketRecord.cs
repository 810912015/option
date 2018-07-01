using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Com.BitsQuan.Option.Core
{
    public class MarketRecord
    {
        [Key]
        public int Id { get; set; }
        public int ContractId { get; set; }
        public decimal NewestPrice { get; set; }
        public int SellCount { get; set; }
        public int BuyCount { get; set; }
        public decimal FuseMax { get; set; }
        public decimal FuseMin { get; set; }
        public int Times { get; set; }
        public int Copies { get; set; }
        public decimal NewestDealPrice { get; set; }
        public decimal Total { get; set; }
        public int OpenTotal { get; set; }
        public int Open24 { get; set; }
        public int Close24 { get; set; }
        public int PureOpen24 { get; set; }
        public decimal High24 { get; set; }
        public decimal Low24 { get; set; }

        public MarketRecord() { }
    }
}
