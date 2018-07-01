using Com.BitsQuan.Option.Core;

namespace Com.BitsQuan.Option.Match.Spot
{
    public class CoinAccount
    {
        public Coin Coin { get; set; }
        public decimal Balance { get; set; }
        public decimal Frozen { get; set; }
        object bl;
        public CoinAccount(Coin c)
        {
            this.Coin = c; bl = new object();
            Balance = Frozen = 0;
        }
        

        public void Add(decimal count)
        {
            if (count <= 0) return;
            lock (bl)
            {
                Balance += count;
            }
        }
        public bool Sub(decimal count)
        {
            if (count <= 0 || Balance < count) return false;
            lock (bl) Balance -= count;
            return true;
        }
        public bool Freeze(decimal count)
        {
            if (count <= 0 || Balance < count) return false;
            lock (bl)
            {
                Balance -= count;
                Frozen += count;
                
            }
            return true;
        }
        public bool Unfreeze(decimal count)
        {
            if (count <= 0 || Frozen < count) return false;
            lock (bl)
            {
                Frozen -= count;
                Balance += count;
            }
            return true;
        }
    }
}
