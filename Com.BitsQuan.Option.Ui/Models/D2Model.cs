using Com.BitsQuan.Option.Core;
using Com.BitsQuan.Option.Core.Spot;
using Com.BitsQuan.Option.Match.Dto;
using Com.BitsQuan.Option.Ui.Controllers;
using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;

namespace Com.BitsQuan.Option.Ui.Models
{
    public class BankOpModel
    {
        [Required]
        [StringLength(100, ErrorMessage = "银行卡,不能超过100个字符")]
        [Display(Name = "银行卡")]
        public string BankAccountId { get; set; }
        [Required]
        [Range(0, 10000)]
        [Display(Name = "操作金额")]
        [RegularExpression("^[1-9].*$", ErrorMessage = "金额必须大于0")]
        [System.Web.Mvc.Remote("IsMoneyTrue", "cache", ErrorMessage = "人民币不足")]

        public decimal Delta { get; set; }
        [Required]
        [Display(Name = "交易密码")]
        [System.Web.Mvc.Remote("IsTradeCodeTrue", "cache", ErrorMessage = "交易密码不正确")]
        public string Tradepwd { get; set; }


        [Required]
        [Display(Name = "手机验证码")]
        [System.Web.Mvc.Remote("IsPhoneCodeTrue", "cache", ErrorMessage = "验证码不正确")]
        public string PhoneCode { get; set; }
    }

    public class BankOpModel2
    {
        [Required]
        [Display(Name = "银行卡")]
        public string BankAccountId { get; set; }
        [Required]
        [Range(0, 10000000)]
        [Display(Name = "确认金额")]
        [RegularExpression(@"^[1-9].*$", ErrorMessage = "金额格式不正确")]
        public decimal Delta { get; set; }


    }

    public class OpModelBTCw
    {

        [Required]
        [StringLength(100, ErrorMessage = "银行卡,不能超过100个字符")]
        [Display(Name = "提现地址")]
        public string AddressNum { get; set; }
        [Required]
        [Display(Name = "提现数量")]
        [RegularExpression("^[1-9].*$", ErrorMessage = "数量必须大于0")]
        [System.Web.Mvc.Remote("IsNumTrue", "cache", ErrorMessage = "BTC不足")]


        public decimal Num { get; set; }

        [Required]
        [Display(Name = "交易密码")]
        [System.Web.Mvc.Remote("IsTradeCodeTrue", "cache", ErrorMessage = "交易密码不正确")]
        public string Tradepwd { get; set; }


        [Required]
        [Display(Name = "手机验证码")]
        [System.Web.Mvc.Remote("IsPhoneCodeTrue", "cache", ErrorMessage = "验证码不正确")]
        public string PhoneCode { get; set; }
    }

    public class OpModelBTCr
    {
        [Required]
        [Display(Name = "用户UID")]
        public string UID { get; set; }
        [Required]
        [Display(Name = "BTC充值地址")]
        public string RBtcAddress { get; set; }
        [Required]
        [Display(Name = "BTC充值数量")]
        public decimal RBtcNum { get; set; }
        [Required]
        [Display(Name = "实际充值数量")]
        public decimal RBtcNumtrue { get; set; }
    }

    public class ReplyModel
    {

        [Required]
        [Display(Name = "回帖内容")]
        public string Rcontent { get; set; }

        [Required]
        [Display(Name = "父编号")]
        public int fid { get; set; }
    }

    public class HostModel
    {

        [Required]
        [Display(Name = "帖子标题")]
        public string Title { get; set; }

        [Required]
        [Display(Name = "帖子内容")]
        public string Hcontent { get; set; }

        [Required]
        [Display(Name = "帖子种类")]
        public string Htype { get; set; }
    }
    public class AuditModel
    {
        [Display(Name = "请求编号")]
        public int BrId { get; set; }
        [Display(Name = "是否通过")]
        public bool IsApproved { get; set; }
        [Display(Name = "实际金额")]
        [Range(0, 10000)]
        public decimal ActualDelta { get; set; }
        [Display(Name = "系统银行卡")]
        [Required]
        public string SysBank { get; set; }
        [Display(Name = "描述")]
        public string Desc { get; set; }
        [Required]
        [Display(Name = "授权码")]
        [System.Web.Mvc.Remote("IsTranPwdTrue", "audit", ErrorMessage = "授权码错误")]
        public string code { get; set; }
    }
    //交易密码
    public class UpdL_PwdModel
    {
        [Display(Name = "原始交易密码")]
        public string Lpwd { get; set; }

        [Required(ErrorMessage = "请输入新交易密码")]
        [Display(Name = "新交易密码")]
        [System.Web.Mvc.Remote("IsLoginPwd", "Secure", ErrorMessage = "登录密码和交易密码不能相同")]
        [RegularExpression("^[a-zA-Z]{1}([a-zA-Z0-9]|[._]){7,19}$", ErrorMessage = "密码格式不正确")]

        public string Lnpwd { get; set; }


        [Required(ErrorMessage = "请确认新交易密码")]
        [Display(Name = "再次输入新交易密码")]
        [Compare("Lnpwd", ErrorMessage = "与确认新密码不匹配")]
        public string Lnpwd2 { get; set; }



        [Required]
        [Display(Name = "手机验证码")]
        [System.Web.Mvc.Remote("IsCodeTrue", "Secure", ErrorMessage = "验证码错误")]
        public string Code { get; set; }
    }
    //登录
    public class UpdL_PwdModel2
    {
        [Required(ErrorMessage = "请输入原始密码")]
        [Display(Name = "登录密码")]
        [System.Web.Mvc.Remote("IsLoginPwdTrue", "Secure", ErrorMessage = "原始密码不正确")]

        public string Lpwd { get; set; }

        [Required(ErrorMessage = "密码不能为空")]
        [Display(Name = "新登录密码")]
        [RegularExpression("^[a-zA-Z]{1}([a-zA-Z0-9]|[._]){7,19}$", ErrorMessage = "密码格式不正确")]
        [System.Web.Mvc.Remote("IsTranPwd", "Secure", ErrorMessage = "登录密码和交易密码不能相同")]
        public string Lnpwd { get; set; }


        [Required(ErrorMessage = "请确认新密码")]
        [Display(Name = "再次输入新密码")]
        [Compare("Lnpwd", ErrorMessage = "与新密码不匹配")]
        public string Lnpwd2 { get; set; }



        [Required(ErrorMessage = "请输入验证码")]
        [Display(Name = "手机验证码")]
        [System.Web.Mvc.Remote("IsCodeTrue", "Secure", ErrorMessage = "验证码错误")]
        public string Code { get; set; }
    }
    //手机
    public class Upd_Phone
    {
        [Required]
        [Display(Name = "原手机验证码")]
        [System.Web.Mvc.Remote("IsCodeTrue", "Secure", ErrorMessage = "验证码错误")]
        public string Code { get; set; }

        [Required]
        [Display(Name = "绑定新手机号")]
        [System.Web.Mvc.Remote("IsPhoneBoundnewPhone", "Secure", ErrorMessage = "该手机号已被绑定")]
        [RegularExpression("^1[3|5|7|8|][0-9]{9}", ErrorMessage = "手机格式不正确")]
        public string newPhone { get; set; }

        [Required]
        [Display(Name = "新手机验证码")]
        [System.Web.Mvc.Remote("IsCodeTrue_new", "Secure", ErrorMessage = "验证码错误")]

        public string newPhoneCode { get; set; }

    }

    //设置手机号
    public class setPhoneModel
    {
        [Required]
        [Display(Name = "请输入您的手机号码")]
        [RegularExpression("^1[3|5|7|8|][0-9]{9}", ErrorMessage = "手机格式不正确")]
        [System.Web.Mvc.Remote("IsPhoneBound", "Secure", ErrorMessage = "该手机号已被绑定")]

        public string Phone { get; set; }

        [Required]

        [Display(Name = "手机验证码")]
        [System.Web.Mvc.Remote("IsCodeTrue", "Secure", ErrorMessage = "验证码错误")]
        public string Code { get; set; }

        public string PhonNumber
        {
            get { return Phone; }
        }
    }
    //实名认证
    public class Account_iden
    {
        [Required]
        [Display(Name = "证件类型")]
        public string IdentityType { get; set; }

        [Required]
        [Display(Name = "真实姓名")]
        [RegularExpression("^[\u4e00-\u9fa5]*$", ErrorMessage = "姓名格式不正确")]
        public string RealityName { get; set; }

        [Required]
        [Display(Name = "证件号码")]
        public string IdentityId { get; set; }

    }

    //流水查询条件
    public class WhereModel
    {

        [Display(Name = "开始时间")]
        public DateTime? StartTime { get; set; }

        [Display(Name = "结束时间")]
        public DateTime? EndTime { get; set; }

        private int _type = -1;

        [Display(Name = "类型")]
        public int Type { get { return _type; } set { _type = value; } }
    }
}