using Com.BitsQuan.Option.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Com.BitsQuan.Option.Ui.Models
{
    public class SiteParameter : IEntityWithId
    {

        /// <summary>
        /// 编号
        /// </summary>
        public int Id { get; set; }
        /// <summary>
        /// 版权
        /// </summary>
        public string Copyright { get; set; }

        /// <summary>
        /// Mata描述
        /// </summary>
        public string Describe { get; set; }


        /// <summary>
        /// Mata关键字
        /// </summary>
        public string Keyword { get; set; }

        /// <summary>
        /// 网站名称
        /// </summary>
        public string SiteName { get; set; }

        /// <summary>
        /// 网站Url
        /// </summary>
        public string SiteUrl { get; set; }



        /// <summary>
        /// 开启网站
        /// </summary>
        public bool OpenSite { get; set; }

        /// <summary>
        /// 开启用户登陆
        /// </summary>
        public bool siteState { get; set; }


        /// <summary>
        /// 开启用户注册
        /// </summary>
        public bool userRegiste { get; set; }

        /// <summary>
        /// 开启交易
        /// </summary>
      //  public bool trade { get; set; }
        public bool trader { get; set; }

        /// <summary>
        /// 开启交易使用交易密码
        /// </summary>
        public bool tradePwd { get; set; }

        /// <summary>
        /// 用户登录过期时间
        /// </summary>
        public string outTime { get; set; }

        /// <summary>
        /// 邮箱认证用户密码
        /// </summary>
        public string emailUserPwd { get; set; }

        /// <summary>
        /// 邮箱认证用户名
        /// </summary>
        public string emailUserName { get; set; }

        /// <summary>
        /// 显示发送邮件
        /// </summary>
        public string SendEmail { get; set; }

        /// <summary>
        /// 显示发送者名称
        /// </summary>
        public string emaiSendName { get; set; }

        /// <summary>
        /// 发送账号冻结邮件
        /// </summary>
        public bool sendFreezeEmail { get; set; }

        /// <summary>
        /// 发送账号恢复邮件
        /// </summary>
        public bool sendRecoverEmail { get; set; }

        /// <summary>
        /// 发送转账邮件
        /// </summary>
        public bool sendZhuanzEmail { get; set; }

        /// <summary>
        /// 发送提现邮件
        /// </summary>
        public bool sendWithdrawEmail { get; set; }

        /// <summary>
        /// 发送充值邮件
        /// </summary>
        public bool sendreChargeEmail { get; set; }
        /// <summary>
        /// 发送账号冻结短信
        /// </summary>
        public bool sendFreezeMsg { get; set; }

        /// <summary>
        /// 发送账号解冻短信
        /// </summary>
        public bool sendThawMsg { get; set; }

        /// <summary>
        /// 发送转账短信
        /// </summary>
        public bool sendZhuanzMsg { get; set; }

        /// <summary>
        /// 发送提现短信
        /// </summary>
        public bool sendWithdrawMsg { get; set; }
        /// <summary>
        /// 发送充值短信
        /// </summary>
        public bool sendreChargeMsg { get; set; }

        /// 开启银行转账汇款
        /// </summary>
        public bool BankZhuanz { get; set; }

        /// <summary>
        /// 开启在线支付
        /// </summary>
        public bool  OnlinePayment { get; set; }


    }
}