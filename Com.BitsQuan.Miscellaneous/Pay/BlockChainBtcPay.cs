using Com.BitsQuan.Option.Core;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Com.BitsQuan.Miscellaneous
{
    public class TransId
    {
        public string tx_hash { get; set; }
        public string message { get; set; }
        public string notice { get; set; }

        [JsonIgnore]
        public bool IsConfirmed { get; set; }
        [JsonIgnore]
        public string to { get; set; }
        [JsonIgnore]
        public decimal amount { get; set; }
        [JsonIgnore]
        public DateTime when { get; set; }
    }
    public class TransContainer
    {
        Dictionary<string, TransId> dic;
        public TransContainer()
        {
            dic = new Dictionary<string, TransId>();
        }
        public void AddPayment(TransId ti)
        {
            if (!dic.ContainsKey(ti.tx_hash))
            {
                dic.Add(ti.tx_hash, ti);
            }
        }
    }
    /// <summary>
    /// https://blockchain.info/api/blockchain_wallet_api
    /// </summary>
    public class BlockChainBtcPay : IBtcPay
    {
        static TextLog Log = new TextLog("btcPay.txt");
        public string WalletId { get; private set; }
        public string MainPwd { get; private set; }
        public string SecondPwd { get; private set; }
        //TransContainer tc;

        public BlockChainBtcPay(string walletId, string mainPwd, string secondPwd)
        {
            this.WalletId = walletId;
            this.MainPwd = mainPwd;
            this.SecondPwd = secondPwd;
            //tc = new TransContainer();
        }
        public BlockChainBtcPay() : this("23de454f-ac64-4792-ae30-25dc666e8daf",
            "qiuzijun1234", "bitsquan") { }
        /// <summary>
        /// $guid should be replaced with your My Wallet identifier
        /// $main_password Your Main My wallet password
        //$second_password Your second My Wallet password if double encryption is enabled.
        //$to Recipient Bitcoin Address.
        //$amount Amount to send in satoshi.
        //$from Send from a specific Bitcoin Address (Optional)
        //$fee Transaction fee value in satoshi (Must be greater than default fee) (Optional)
        //$note A public note to include with the transaction -- can only be attached when outputs are greater than 0.005 BTC. (Optional)

        //{ "message" : "Response Message" , "tx_hash": "Transaction Hash", "notice" : "Additional Message" }

        /// </summary>
        ///string payUrl = "https://blockchain.info/zh-cn/merchant/{0}/payment?password={1}&second_password={2}&to={3}&amount={4}&from={5}&fee={6}&note={7}";
        string payUrl = "https://blockchain.info/zh-cn/merchant/{0}/payment?password={1}&second_password={2}&to={3}&amount={4}";
        public bool Pay(string to, decimal amount)
        {
            return Except<bool>(() =>
            {
                var toUnit = amount * 100000000 - 10000;
                var url = string.Format(payUrl, WalletId, MainPwd, SecondPwd, to, toUnit);
                var r = HttpExecutor.Get(url);
                Log.Info(r);
                //var ro = JsonConvert.DeserializeObject<TransId>(r);
                //ro.to = to; ro.amount = amount; ro.when = DateTime.Now;
                //tc.AddPayment(ro);
                return true;
            });            
        }
        T Except<T>(Func<T> f)
        {
            try
            {
                return f();
            }
            catch(Exception e)
            {
                Log.Error(e, "btcPay");
                return default(T);
            }
        }

        //value The value of the payment received in satoshi (not including fees). Divide by 100000000 to get the value in BTC.
        //transaction_hash The transaction hash.
        //input_address The bitcoin address that received the transaction.
        //confirmations The number of confirmations of this transaction.
        //{Custom Parameters} Any parameters included in the callback URL will be past back to the callback URL in the notification.

        //In order to acknowledge successful processing of the callback the server should respond with the text "*ok*".

        public string CallBack(decimal value, string transaction_hash, string input_address, decimal confirmations, string addtionalPrm)
        {
            return Except<string>(() =>
            {
                Log.Info(string.Format("value:{0},hash:{1},addr:{2},confirmations:{3},addtion:{4}", value, transaction_hash, input_address, confirmations, addtionalPrm));
                if (OnSuccess != null)
                {
                    OnSuccess(value, input_address);
                }

                return "*ok*";
            });
        }
        public event Action<decimal, string> OnSuccess;

        //        https://blockchain.info/zh-cn/merchant/$guid/sendmany?password=$main_password&second_password=$second_password&recipients=$recipients&fee=$fee

        //$main_password Your Main My wallet password
        //$second_password Your second My Wallet password if double encryption is enabled.
        //$recipients Is a JSON Object using Bitcoin Addresses as keys and the amounts to send as values (See below).
        //$from Send from a specific Bitcoin Address (Optional)
        //$fee Transaction fee value in satoshi (Must be greater than default fee) (Optional)
        //$note A public note to include with the transaction -- can only be attached to transactions where all outputs are greater than 0.005 BTC.(Optional)
        public string PayMany()
        {
            throw new NotImplementedException();
        }
        string balanceUrl = "https://blockchain.info/zh-cn/merchant/{0}/balance?password={1}";
        public string GetBalance()
        {
            var u = string.Format(balanceUrl, WalletId, MainPwd);
            var r = HttpExecutor.Get(u);
            return r;
        }
        //https://blockchain.info/zh-cn/merchant/$guid/list?password=$main_password
        //$confirmations The minimum number of confirmations transactions must have before being included in balance of addresses (Optional)
        string addrUrl = "https://blockchain.info/zh-cn/merchant/{0}/list?password={1}";
        public string GetAddresses()
        {
            var u = string.Format(addrUrl, WalletId, MainPwd);
            var r = HttpExecutor.Get(u);
            return r;
        }


        //        https://blockchain.info/zh-cn/merchant/$guid/address_balance?password=$main_password&address=$address&confirmations=$confirmations
        //$main_password Your Main My wallet password
        //$address The bitcoin address to lookup
        //$confirmations Minimum number of confirmations required. 0 for unconfirmed.
        public string GetBalanceByAddress()
        {
            throw new NotImplementedException();
        }

        //        https://blockchain.info/zh-cn/merchant/$guid/new_address?password=$main_password&second_password=$second_password&label=$label
        //$main_password Your Main My wallet password
        //$second_password Your second My Wallet password if double encryption is enabled.
        //$label An optional label to attach to this address. It is recommended this is a human readable string e.g. "Order No : 1234". You May use this as a reference to check balance of an order (documented later)
        string gaddr = "https://blockchain.info/zh-cn/merchant/{0}/new_address?password={1}&second_password={2}&label={3}";
        public string GenerateAddress(string label)
        {
            string u = string.Format(gaddr, WalletId, MainPwd, SecondPwd, label);
            string r = HttpExecutor.Get(u);
            return r;
        }

        //        https://blockchain.info/zh-cn/merchant/$guid/archive_address?password=$main_password&second_password=$second_password&address=$address

        //$main_password Your Main My wallet password
        //$second_password Your second My Wallet password if double encryption is enabled.
        //$address The bitcoin address to archive
        public string ArchiveAddress()
        {
            throw new NotImplementedException();
        }
        //https://blockchain.info/zh-cn/merchant/$guid/unarchive_address?password=$main_password&second_password=$second_password&address=$address

        //$main_password Your Main My wallet password
        //$second_password Your second My Wallet password if double encryption is enabled.
        //$address The bitcoin address to unarchive
        public string UnarchiveAddress()
        {
            throw new NotImplementedException();
        }
        //        https://blockchain.info/zh-cn/merchant/$guid/auto_consolidate?password=$main_password&second_password=$second_password&days=$days

        //$main_password Your Main My wallet password
        //$second_password Your second My Wallet password if double encryption is enabled.
        //$days Addresses which have not received any transactions in at least this many days will be consolidated.
        public string ConsolidateAddress()
        {
            throw new NotImplementedException();
        }
    }
}
