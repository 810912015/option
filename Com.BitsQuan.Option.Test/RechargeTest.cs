using Com.BitsQuan.Miscellaneous;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Com.BitsQuan.Option.Test
{
    [TestClass]
    class RechargeTest
    {
        BlockChainBtcPay bb = new BlockChainBtcPay();
        [TestMethod]
        public string rtest() {
          var hh =  bb.CallBack(300000000, "*OK", "1HsuPyJ6KLnepAgZavznDM5PfAPQMGMTUm", 300000000, "hello1");
          return hh;
        }
    }
}
