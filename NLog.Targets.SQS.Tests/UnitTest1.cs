using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace NLog.Targets.SQS.Tests
{
    [TestClass]
    public class UnitTest1
    {




        [TestMethod]
        public void TestMethod1()
        {
            try
            {
                var logger = NLog.LogManager.GetCurrentClassLogger();
                logger.Info("This message from NLog.Targets.SQS");
            }
            catch (Exception)
            {
                
                throw;
            }
        }
    }
}
