using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Threading;

namespace Com.BitsQuan.Option.Core
{ 
    /// <summary>
    /// 账户
    /// </summary>
    public class Account:IEntityWithId
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int Id { get; set; }
        
        public virtual  Coin CacheType { get; set; }
        decimal sum;
        /// <summary>
        /// 可用金额
        /// </summary>
        public decimal Sum { get;set; }
        /// <summary>
        /// 冻结金额
        /// </summary>
        public decimal Frozen { get; set; }
        /// <summary>
        /// 总金额:包含可用金额,冻结金额,和其他(如维持保证金金额,在继承类中定义)
        /// </summary>
        public virtual decimal Total { get { return Sum + Frozen; } }

        public override string ToString()
        {
            return string.Format("币种:{0};金额:{1};冻结金额:{2}", CacheType, Sum.ToString("C2"), Frozen.ToString("C2"));
        }
        /// <summary>
        /// 账户金额改变事件
        /// 参数:改变的值,改变后的值
        /// </summary>
        public event Action<decimal, decimal> OnBalanceChanged;
        public static event Action<Account> OnTotalChanged;
        void RaiseAccountChanged(decimal delta)
        {
            if (OnBalanceChanged != null)
            {
                OnBalanceChanged(delta, this.sum);
            }
            if (OnTotalChanged != null)
            {
                OnTotalChanged(this);
            }
        }
        public virtual bool Freeze(decimal d)
        {
            if (d > Sum) return false;
            lock (this)
            {
                Sum -= d; Frozen += d;
            }
            RaiseAccountChanged(d);
            return true;
        }
        public virtual  bool UnFreeze(decimal d)
        {
            if (Frozen <= 0) return false;
            // 如果要求解冻的金额大于冻结的金额,则冻结的金额全部解冻
            if (d > Frozen)
                d = Frozen;
            lock (this)
            {
                Sum += d; Frozen -= d;
            }
            RaiseAccountChanged(d);
            return true;
        }
        
        public virtual  bool Add(decimal d)
        {
            if (d <= 0) return false;
            lock (this)
            {
                Sum += d;
            }
            RaiseAccountChanged(d);
            return true;
        }
        public virtual  bool Sub(decimal d)
        {
            if (d <= 0||Sum<d) return false;
            lock (this)
            {
                Sum -= Math.Round(d, 2, MidpointRounding.AwayFromZero);
            }
            RaiseAccountChanged(d);
            return true;
        }
    }
}
