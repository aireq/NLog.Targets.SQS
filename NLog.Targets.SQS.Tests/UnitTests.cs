using System;
using System.Threading.Tasks;
using Xunit;

namespace NLog.Targets.SQS.Tests
{
    public class UnitTests
    {
        [Fact]
        public async Task TestMethod1()
        {
            Guid guid = Guid.NewGuid();

            string testMessageBody = "Test message from NLog.Targets.SQS {" + guid + "]";

            AwsSqsTarget target = (AwsSqsTarget)LogManager.Configuration.FindTargetByName("SQS Target");

            var region = Amazon.RegionEndpoint.GetBySystemName(target.RegionEndPoint);
            using (var sqs_client = new Amazon.SQS.AmazonSQSClient(target.AwsAccessKeyId, target.AwsSecretAccessKey, region))
            {
                //Purge the target queue of existing messages
                var att =  await sqs_client.GetQueueAttributesAsync(target.QueueUrl, new System.Collections.Generic.List<string>() { "All" });

                if (att.ApproximateNumberOfMessages > 0 | att.ApproximateNumberOfMessagesDelayed > 0 | att.ApproximateNumberOfMessagesNotVisible > 0)
                {
                    await sqs_client.PurgeQueueAsync(target.QueueUrl);
                }

                var logger = LogManager.GetCurrentClassLogger();

                logger.Info(testMessageBody);

                System.Threading.Thread.Sleep(1000);

                Amazon.SQS.Model.ReceiveMessageRequest recReq = new Amazon.SQS.Model.ReceiveMessageRequest(target.QueueUrl);
                recReq.MessageAttributeNames.Add("Level");
                recReq.MessageAttributeNames.Add("Logger");
                recReq.MessageAttributeNames.Add("SequenceID");

                var messages = await sqs_client.ReceiveMessageAsync(recReq);
                    
                Assert.Equal(System.Net.HttpStatusCode.OK, messages.HttpStatusCode);
                Assert.Single(messages.Messages);
                var message = messages.Messages[0];

                try
                {
                    Assert.Equal(testMessageBody, message.Body);
                    Assert.Equal("Info", message.MessageAttributes["Level"].StringValue);
                    Assert.Equal("NLog.Targets.SQS.Tests.UnitTests", message.MessageAttributes["Logger"].StringValue);
                    Assert.NotNull(message.MessageAttributes["SequenceID"].StringValue);
                }
                finally
                {
                    await sqs_client.DeleteMessageAsync(target.QueueUrl, message.ReceiptHandle);
                }
            }
        }
    }
}