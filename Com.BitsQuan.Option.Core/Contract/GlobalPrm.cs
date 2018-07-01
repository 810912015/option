using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Com.BitsQuan.Option.Core.Infra
{
    /// <summary>
    /// 系统级参数
    /// </summary>
    public class GlobalPrm
    {
        [Key]
        public string Name { get; set; }
        public string Value { get; set; }
    }
}
