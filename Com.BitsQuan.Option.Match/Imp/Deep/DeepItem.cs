using System.Collections.Generic;

namespace Com.BitsQuan.Option.Match
{


    public class DeepItem
    {
        public decimal Price { get; set; }
        public decimal Count { get; set; }
        public decimal Total { get; set; }

        public List<decimal> List
        {
            get { return new List<decimal> { Price, this.Total, this.Count }; }
        }
    }
}
