using Com.BitsQuan.Option.Core;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Com.BitsQuan.Option.Ui.Areas.Supervise.Models
{
    public class ContractModel
    {
        /// <summary>
        /// 币种即是标的?
        /// </summary>
        [Display(Name = "币种")]
        public string CoinName { get; set; }
        [Required]
        [Display(Name = "行权日")]
        public DateTime ExecuteTime { get; set; }
        [Required]
        [Display(Name = "行权价")]
        public decimal ExecutePrice { get; set; }
        [Display(Name = "合约类型")]
        public OptionType OptionType { get; set; }
        [Display(Name = "合约单位")]
        [Range(0.001,100000)]
        public decimal CoinCount { get; set; }
    }
}