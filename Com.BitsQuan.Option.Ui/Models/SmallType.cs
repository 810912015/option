using Com.BitsQuan.Option.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Com.BitsQuan.Option.Ui.Models
{
    /// <summary>
    /// 小类型
    /// </summary>
    public class SmallType : IEntityWithId
    {
        /// <summary>
        /// 编号
        /// </summary>
        public int Id { get; set; }
        /// <summary>
        /// 类型名称
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// 类型说明
        /// </summary>
        public string Explain { get; set; }
        /// <summary>
        /// 版主
        /// </summary>
        public string EditionUser { get; set; }
        /// <summary>
        /// 对应的大类型
        /// </summary>
     //   public virtual BigType BigType { get; set; }
        public int BigType { get; set; }
    }
}