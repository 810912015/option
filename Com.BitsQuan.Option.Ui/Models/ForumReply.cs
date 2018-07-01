using Com.BitsQuan.Option.Core;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace Com.BitsQuan.Option.Ui.Models
{
    /// <summary>
    /// 回帖
    /// </summary>
    public class ForumReply : IEntityWithId
    {
        /// <summary>
        /// 编号
        /// </summary>
        public int Id { get; set; }
        /// <summary>
        /// 对应主贴编号
        /// </summary>
        public int Fid { get; set; }
        /// <summary>
        /// 回帖内容
        /// </summary>
        public string Rcontent { get; set; }
        /// <summary>
        /// 回帖用户
        /// </summary>
        public string Uid { get; set; }
        /// <summary>
        /// 回帖时间
        /// </summary>
        public string RDate { get; set; }

        public int Smalltype { get; set; }
        public int Bigtype { get; set; }
    }
}