using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NLog.Targets.SQS
{
    [Target("SQS")]
    public class AwsSqsTarget : NLog.Targets.Target, IDisposable
    {

        [NLog.Config.RequiredParameter]
        public string RegionEndPoint { get; set; }

        [NLog.Config.RequiredParameter]
        public string QueueUrl { get; set; }

        public int DelaySeconds { get; set; }

        [NLog.Config.RequiredParameter]
        public string AwsAccessKeyId { get; set; }

        [NLog.Config.RequiredParameter]
        public string AwsSecretAccessKey { get; set; }


        private Amazon.SQS.AmazonSQSClient _client;


        protected override void InitializeTarget()
        {
            base.InitializeTarget();
            var region = Amazon.RegionEndpoint.GetBySystemName(RegionEndPoint);
            _client = new Amazon.SQS.AmazonSQSClient(AwsAccessKeyId, AwsSecretAccessKey,region);
        }

        protected override void Write(LogEventInfo logEvent)
        {

            _client.SendMessage(QueueUrl, logEvent.Message);



            Amazon.SQS.Model.SendMessageRequest req = new Amazon.SQS.Model.SendMessageRequest(QueueUrl, "message body");


            //logEvent.Exception;
            //logEvent.FormatProvider;
            //logEvent.FormattedMessage;
            //logEvent.HasStackTrace;
            //logEvent.Level;
            //logEvent.LoggerName;
            //logEvent.Message;
            //logEvent.Parameters;
            //logEvent.Properties;
            //logEvent.SequenceID;
            //logEvent.StackTrace;
            //logEvent.TimeStamp;
            //logEvent.UserStackFrame;
            //logEvent.UserStackFrameNumber;





            //req.MessageBody;
            //req.QueueUrl;
            //req.DelaySeconds;




            //if (_messageDespatcher == null)
            //    throw new ArgumentNullException(nameof(_messageDespatcher));

            //if (logEvent?.LoggerName?.Equals("Amazon", StringComparison.InvariantCultureIgnoreCase) ?? false)   //prevent an infinite loop
            //    return;

            //var message = Layout.Render(logEvent);
            //_messageDespatcher.DespatchAsync(GetTopicArn(), message).ConfigureAwait(false).GetAwaiter().GetResult();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_client != null)
                {
                    _client.Dispose();
                    _client = null;
                }
            }

            base.Dispose(disposing);
        }
    }
}
