using Com.BitsQuan.Option.Ui.ClientValidation;
using System.ComponentModel.DataAnnotations;

namespace Com.BitsQuan.Option.Ui.Models
{
    public class ExternalLoginConfirmationViewModel
    {
        [Required]
        [Display(Name = "用户名")]
        public string UserName { get; set; }
    }

    public class ManageUserViewModel
    {
        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "当前密码")]
        public string OldPassword { get; set; }

        [Required]
        [StringLength(100, ErrorMessage = "{0} 必须至少包含 {2} 个字符。", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "新密码")]
        public string NewPassword { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "确认新密码")]
        [Compare("NewPassword", ErrorMessage = "新密码和确认密码不匹配。")]
        public string ConfirmPassword { get; set; }
    }

    public class LoginViewModel
    {
        [Required]
        [Display(Name = "用户名")]
        public string UserName { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "密码")]
        public string Password { get; set; }

        //[Required]
        //[Display(Name = "验证码")]
        //[System.Web.Mvc.Remote("IsCodeTrue", "account", ErrorMessage = "验证码不正确")]
        //public string Code { get; set; }

        //[Display(Name = "记住我?")]
        //public bool RememberMe { get; set; }
    }

    public class RegisterViewModel
    {
        [Required(ErrorMessage = "用户名不能为空")]
        [Display(Name = "用户名")]
        [System.Web.Mvc.Remote("IsUserNameUsable", "account", ErrorMessage = "用户名已使用")]
        [MinLength(5, ErrorMessage = "用户名不能小于5个字符")]
        [MaxLength(30, ErrorMessage = "用户名不能大于30个字符")]
        [RegularExpression(@"^[a-zA-Z0-9_]+$", ErrorMessage = "用户名只能包含数字、字母、下划线")]
        public string UserName { get; set; }


        [Required(ErrorMessage = "请填写登录密码")]
        [Display(Name = "登录密码")]
        [RegularExpression("^[a-zA-z][a-zA-Z0-9_.]*$", ErrorMessage = "必须以字母开头字串(可带数字或_或.)")]
        [MinLength(8, ErrorMessage = "密码不能小于8位")]
        [MaxLength(20, ErrorMessage = "密码不能大于20位")]
        [NotEqual("TradePassword", ErrorMessage = "登录密码和确认交易密码不能相同。")]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        //[Required(ErrorMessage = "请确认登录密码")]
        //[DataType(DataType.Password)]
        //[Display(Name = "确认密码")]
        //[Compare("Password", ErrorMessage = "与登录密码不匹配。")]
        public string ConfirmPassword { get; set; }

        const string TRADEPASSWORD_ERR1 = "8-20个以字母开头、可带数字、“_”、“.”的字串";

        [NotEqual("Password", ErrorMessage = "登录密码和交易密码不能相同。")]
        [Required(ErrorMessage = "交易密码不能为空")]
        [Display(Name = "交易密码")]
        [DataType(DataType.Password)]
        [MinLength(8, ErrorMessage = TRADEPASSWORD_ERR1)]
        [MaxLength(20, ErrorMessage = TRADEPASSWORD_ERR1)]
        [RegularExpression("^[a-zA-Z][a-zA-Z0-9_.]*$", ErrorMessage = TRADEPASSWORD_ERR1)]
        public string TradePassword { get; set; }

        //[Required(ErrorMessage = "请确认交易密码")]
        //[DataType(DataType.Password)]
        //[Display(Name = "确认密码")]
        //[Compare("TradePassword", ErrorMessage = "与确认交易密码不匹配。")]
        public string TradeConfirmPassword { get; set; }

        [Required(ErrorMessage = "邮箱地址不能为空")]
        [Display(Name = "邮箱")]
        [EmailAddress(ErrorMessage = "请输入有效邮箱地址")]
        [System.Web.Mvc.Remote("IsUserEmailUsable", "account", ErrorMessage = "该邮箱已被注册")]

        public string Email { get; set; }


        //[Required]
        //[Display(Name = "验证码")]
        //[System.Web.Mvc.Remote("IsCodeTrue", "account", ErrorMessage = "验证码不正确")]
        public string Code { get; set; }


        //[Required(ErrorMessage="请先选择同意服务条款")]
        //[Display(Name = "阅读条款")]
        //public bool IsCheck { get; set; }

        //[Required]
        //[Display(Name="手机号")]
        //[Phone]
        //public string PhoneNumber { get; set; }


        //[Required]
        //[Display(Name="身份证号")]
        //[RegularExpression(@"^(\d{15}|\d{17}[\dx])$",ErrorMessage="必须是身份证号")]
        //public string IdNumber { get; set; }
    }

    public class EmailContentViewModel {
        public string Email { get; set; }
        public string Message { get; set; }
        public string Url { get; set; }
    }
}
