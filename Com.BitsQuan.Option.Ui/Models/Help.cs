using Com.BitsQuan.Option.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Com.BitsQuan.Option.Ui.Models
{
    /// <summary>
    /// 帮助
    /// </summary>
    public class Help : IEntityWithId
    {
        //编号
        public int Id { get; set; }
        /// <summary>
        /// 新闻标题
        /// </summary>
        public string Htitle { get; set; }
        /// <summary>
        /// 新闻内容
        /// </summary>
        public string Hcontent { get; set; }
        /// <summary>
        /// 发布时间
        /// </summary>
        public string Hdate { get; set; }
        /// <summary>
        /// 发布人
        /// </summary>
        public string Hperson { get; set; }
        /// <summary>
        /// 最后更新时间
        /// </summary>
        public string HlastDate { get; set; }
        /// <summary>
        /// 是否在前台显示
        /// </summary>
        public bool HforeShow { get; set; }

        public string imgSrc { get; set; }

        public int ReadTime { get; set; }

        public ContentType type { get; set; }
    }
}