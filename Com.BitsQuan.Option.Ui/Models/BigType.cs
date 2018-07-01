using Com.BitsQuan.Option.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Com.BitsQuan.Option.Ui.Models
{
    public class BigType : IEntityWithId
    {
        /// <summary>
        /// 编号
        /// </summary>
        public int Id { get; set; }
        /// <summary>
        /// 类型名称
        /// </summary>
        public string Name { get; set; }
    }
}