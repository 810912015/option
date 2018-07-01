using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Com.BitsQuan.Option.Core
{
    /// <summary>
    /// 交易用户
    /// </summary>
    public class Trader : IEntityWithId
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int Id { get; set; }
        /// <summary>
        /// 用户名称
        /// </summary>
        public string Name { get; set; }

        public virtual TraderAccount Account { get; set; }
        /// <summary>
        /// 当前持仓
        /// </summary>
        public virtual ICollection<UserPosition> Positions { get; set; }
        /// <summary>
        /// 需要时是否可以自动将现金账户的资金转入保证金账户;由用户设置
        /// </summary>
        public bool IsAutoAddBailFromCache { get; set; }
        /// <summary>
        /// 需要时是否可以执行权力仓自动买平(卖平)
        /// </summary>
        public bool IsAutoSellRight { get; set; }
        /// <summary>
        /// 是否冻结:当账户异常时,设置冻结,此时不允许用户进行交易
        /// </summary>
        public bool IsFrozen { get; set; }
        
        public override string ToString()
        {
            return string.Format("编号:{0},姓名:{1},{2},保证金自动转入:{3},权利仓自动买平:{4}",
                Id, Name, Account, IsAutoSellRight, IsAutoAddBailFromCache);
        }

        public event Action<Trader, decimal> OnRatioChanged;
        public void RaiseRatioChanged(decimal ratio)
        {
            if (OnRatioChanged != null)
            {
                OnRatioChanged(this, ratio);
            }
        }

        public event Action<Trader, decimal> OnBailChanged;

        public void ReraiseBailEvent()
        {
            this.Account.BailAccount.OnBalanceChanged += BailAccount_OnAccountChanged;
        }

        void BailAccount_OnAccountChanged(decimal arg1, decimal arg2)
        {
            if (OnBailChanged != null)
                OnBailChanged(this, arg1);
        }
    }
}
