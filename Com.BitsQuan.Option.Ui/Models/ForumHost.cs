using Com.BitsQuan.Option.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Com.BitsQuan.Option.Ui.Models
{
   /// <summary>
   /// 主贴
   /// </summary>
    public class ForumHost : IEntityWithId
    {
       /// <summary>
        /// 编号
       /// </summary>
        public int Id { get; set; }
        /// <summary>
        /// 主贴名称
        /// </summary>
        public string Fname { get; set; }
        /// <summary>
        /// 主贴内容
        /// </summary>
        public string Fcontent { get; set; }
        /// <summary>
        /// 发帖人
        /// </summary>
        public string FuserName { get; set; }
  //      public virtual ApplicationUser Fuser { get; set; }
        /// <summary>
        /// 发帖时间
        /// </summary>
        public string FDate { get; set; }
        /// <summary>
        /// 大类型
        /// </summary>
      public int Bigtype { get; set; }
        /// <summary>
        /// 小类型
        /// </summary>
      //  public virtual SmallType Smalltype { get; set; }
       public int Smalltype { get; set; }

        /// <summary>
        /// 回复数量
        /// </summary>
       public int replyCount { get; set; }
       /// <summary>
       /// 置顶日期
       /// </summary>
    //   public string TopTime { get; set; }

    }
}