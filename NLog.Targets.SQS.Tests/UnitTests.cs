using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace NLog.Targets.SQS.Tests
{
    [TestClass]
    public class UnitTests
    {




        [TestMethod]
        public void TestMethod1()
        {
            try
            {
                Guid guid = Guid.NewGuid();

                string testMessageBody = "Test message from NLog.Targets.SQS {" + guid + "]";

                NLog.Targets.SQS.AwsSqsTarget target = (NLog.Targets.SQS.AwsSqsTarget)NLog.LogManager.Configuration.FindTargetByName("SQS Target");

                var region = Amazon.RegionEndpoint.GetBySystemName(target.RegionEndPoint);
                using (var sqs_client = new Amazon.SQS.AmazonSQSClient(target.AwsAccessKeyId, target.AwsSecretAccessKey, region))
                {

                    //Purge the target queue of existing messages
                    var att = sqs_client.GetQueueAttributes(target.QueueUrl, new System.Collections.Generic.List<string>() { "All" });

                    if (att.ApproximateNumberOfMessages > 0 | att.ApproximateNumberOfMessagesDelayed > 0 | att.ApproximateNumberOfMessagesNotVisible > 0)
                    {
                        sqs_client.PurgeQueue(target.QueueUrl);
                    }




                    var logger = NLog.LogManager.GetCurrentClassLogger();


                    logger.Info(testMessageBody);


                    System.Threading.Thread.Sleep(1000);

                    var messages = sqs_client.ReceiveMessage(target.QueueUrl);

                    Assert.AreEqual(System.Net.HttpStatusCode.OK, messages.HttpStatusCode);
                    Assert.AreEqual(1, messages.Messages.Count);
                    var message = messages.Messages[0];

                    try
                    {
                        Assert.AreEqual(testMessageBody, message.Body);
                        Assert.AreEqual("Info", message.Attributes["Level"]);
                        Assert.AreEqual("NLog.Targets.SQS.Tests.UnitTests", message.Attributes["Logger"]);
                        Assert.AreEqual("0", message.Attributes["SequenceID"]);
                    }
                    finally
                    {
                        sqs_client.DeleteMessage(target.QueueUrl, message.ReceiptHandle);
                    }
                }
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}
