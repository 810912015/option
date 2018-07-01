using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Com.BitsQuan.Option.Ui.Models
{
    public class BankModel
    {
        /// <summary>
        /// 卡号
        /// </summary>
        [Required]
        [StringLength(20,ErrorMessage="字符太长")]
        [RegularExpression("^[0-9]{15,20}$", ErrorMessage = "卡号不正确")]
        public string Number { get; set; }
        /// <summary>
        /// 银行名称
        /// </summary>
        [Required]
        [StringLength(50, ErrorMessage = "字符太长")]
        public string Bank { get; set; }
        /// <summary>
        /// 开户行
        /// </summary>
        [Required]
        [StringLength(50, ErrorMessage = "字符太长")]
        public string Province { get; set; }
        [Required]
        [StringLength(50, ErrorMessage = "字符太长")]
        public string City { get; set; }

        /// <summary>
        /// 营业部名称
        /// </summary>
        public string SalesOfficeName { get; set; }
        /// <summary>
        /// 开户姓名
        /// </summary>
        [Required]
        [StringLength(50, ErrorMessage = "字符太长")]
        public string Name { get; set; }
    }

    public class AddressModel
    {
        
        /// <summary>
        /// 卡号
        /// </summary>
 
        public int Id { get; set; }
        /// <summary>
        /// 地址名称
        /// </summary>
       [Required]
       [StringLength(34, ErrorMessage = "字符太长")]
        public string Name { get; set; }
        /// <summary>
        /// 地址
        /// </summary>
        [Required]
        [StringLength(34, ErrorMessage = "字符太长")]
        public string Address { get; set; }
        /// <summary>
        /// 货币类型
        /// </summary>
       [Required]
        public string Coin { get; set; }
    }
}