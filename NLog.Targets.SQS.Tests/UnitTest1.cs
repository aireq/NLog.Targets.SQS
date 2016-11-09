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
                string testMessageBody = "This message from NLog.Targets.SQS";

                NLog.Targets.SQS.AwsSqsTarget target = (NLog.Targets.SQS.AwsSqsTarget)NLog.LogManager.Configuration.FindTargetByName("SQS Target");

                var region = Amazon.RegionEndpoint.GetBySystemName(target.RegionEndPoint);
                using (var sqs_client = new Amazon.SQS.AmazonSQSClient(target.AwsAccessKeyId, target.AwsSecretAccessKey, region))
                {
                    var logger = NLog.LogManager.GetCurrentClassLogger();
                    logger.Info(testMessageBody);

                    System.Threading.Thread.Sleep(1000);

                    var message = sqs_client.ReceiveMessage(target.QueueUrl);

                    Assert.AreEqual(System.Net.HttpStatusCode.OK, message.HttpStatusCode);
                    Assert.AreEqual(1, message.Messages.Count);
                    Assert.AreEqual(testMessageBody, message.Messages[0].Body);

                    sqs_client.DeleteMessage(target.QueueUrl, message.Messages[0].ReceiptHandle);
                }
            }
            catch (Exception)
            {

                throw;
            }
        }
    }
}
