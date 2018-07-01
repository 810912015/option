using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Com.BitsQuan.Option.Match;
using Com.BitsQuan.Option.Core;

namespace Com.BitsQuan.Option.Test
{
    [TestClass]
    public class ModelTest
    {
        [TestMethod]
        public void TestMethod1()
        {
            //Assert.IsNotNull(SingletonWithInit<OptionModel>.Instance.Orders);
        }
        [TestMethod]
        public void TestMethod2()
        {
            SingletonWithInit<OptionModel>.Instance.AddOrder(new Order());  
        }
    }
}
