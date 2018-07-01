using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Com.BitsQuan.Boss.App
{
    public enum ContractType
    {
        Call=1,Put=2
    }
    public class Contract
    {
        public int Id { get; set; }
        public string Target { get; set; }
        public DateTime ExecuteTime { get; set; }
        public decimal ExecutePrice { get; set; }

        public string Name { get; set; }
    }
}
