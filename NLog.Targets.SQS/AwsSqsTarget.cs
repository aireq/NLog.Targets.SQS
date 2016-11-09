using NLog.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NLog.Targets.SQS
{
    [Target("SQS")]
    public class AwsSqsTarget : NLog.Targets.TargetWithLayout, IDisposable
    {

        public const int DEFAULT_BATCH_SIZE = 10;
        public const int MIN_BATCH_SIZE = 1;
        public const int MAX_BATCH_SIZE = 10;

        [NLog.Config.RequiredParameter]
        public string RegionEndPoint { get; set; }

        public bool ThrowExceptions { get; set; }

        [NLog.Config.RequiredParameter]
        public string QueueUrl { get; set; }

        public int DelaySeconds { get; set; }

        [NLog.Config.RequiredParameter]
        public string AwsAccessKeyId { get; set; }


        [NLog.Config.RequiredParameter]
        public string AwsSecretAccessKey { get; set; }


        public AwsSqsTarget()
        {
            BatchSize = DEFAULT_BATCH_SIZE;
        }

        private int _batchSize;
        public int BatchSize
        {
            get
            {
                return _batchSize;
            }
            set
            {

                if (value < MIN_BATCH_SIZE | value > MAX_BATCH_SIZE)
                {
                    string m = "BatchSize can not be set to " + value.ToString() + ", it must be between " + MIN_BATCH_SIZE + " and " + MAX_BATCH_SIZE + ". Defaulting to " + DEFAULT_BATCH_SIZE;
                    InternalLogger.Error(m);

                    _batchSize = DEFAULT_BATCH_SIZE;

                    if (ThrowExceptions) throw new ArgumentException(m);
                }
                else
                {
                    _batchSize = value;
                }
            }
        }



        private Amazon.SQS.AmazonSQSClient _client;


        protected override void InitializeTarget()
        {
            base.InitializeTarget();
            var region = Amazon.RegionEndpoint.GetBySystemName(RegionEndPoint);
            _client = new Amazon.SQS.AmazonSQSClient(AwsAccessKeyId, AwsSecretAccessKey, region);
        }


        protected override void Write(AsyncLogEventInfo logEvent)
        {
            Write(new[] { logEvent });
        }

        protected override void Write(AsyncLogEventInfo[] logEvents)
        {
            try
            {
                List<AsyncLogEventInfo> batchList = new List<AsyncLogEventInfo>();

                foreach (var logEvent in logEvents)
                {
                    batchList.Add(logEvent);

                    if (batchList.Count == BatchSize)
                    {
                        SendBatch(batchList.Select(e => e.LogEvent));
                        batchList.Clear();
                    }
                }

                if (batchList.Count > 0) SendBatch(batchList.Select(e => e.LogEvent));
            }
            catch (Exception exp)
            {
                InternalLogger.Error(exp, "Error while sending log messages to Amazon SQS: message=\"{0}\"", exp.Message);
                if (ThrowExceptions) throw;
            }
        }


        private void SendBatch(IEnumerable<LogEventInfo> logEvents)
        {
            //Checks target is initialized and not disposed
            if (_client == null)
            {
                throw new ObjectDisposedException("AwsSqsTarget", "AmazonSQSClient for AwsSqsTarget is NULL. Target may not have been initialized or was disposed.");
            }

            //Create batch of messages
            List<Amazon.SQS.Model.SendMessageBatchRequestEntry> sendMesssageReqs = new List<Amazon.SQS.Model.SendMessageBatchRequestEntry>();

            int id = 0;

            foreach (var logEvent in logEvents)
            {
                //Renders the message using the current layout
                Amazon.SQS.Model.SendMessageBatchRequestEntry batchReqEntry = new Amazon.SQS.Model.SendMessageBatchRequestEntry();


                batchReqEntry.DelaySeconds = DelaySeconds;
                batchReqEntry.MessageBody = this.Layout.Render(logEvent);
                batchReqEntry.Id = id.ToString();


                //Write message attributes
                batchReqEntry.MessageAttributes.Add("Logger", new Amazon.SQS.Model.MessageAttributeValue() { StringValue = logEvent.LoggerName, DataType = "String" });
                batchReqEntry.MessageAttributes.Add("Level", new Amazon.SQS.Model.MessageAttributeValue() { StringValue = logEvent.Level.ToString(), DataType = "String" });
                batchReqEntry.MessageAttributes.Add("SequenceID", new Amazon.SQS.Model.MessageAttributeValue() { StringValue = logEvent.SequenceID.ToString(), DataType = "Number" });


                //Other LogEventInfo Properties ...

                //logEvent.Parameters;
                //logEvent.Properties;
                //logEvent.Message;
                //logEvent.FormattedMessage;
                //logEvent.TimeStamp;
                //logEvent.Exception;
                //logEvent.FormatProvider;
                //logEvent.HasStackTrace;
                //logEvent.StackTrace;
                //logEvent.UserStackFrame;
                //logEvent.UserStackFrameNumber;



                sendMesssageReqs.Add(batchReqEntry);

                id++; //increment message id;
            }


            //Sends the batch of messages
            Amazon.SQS.Model.SendMessageBatchRequest batchReq = new Amazon.SQS.Model.SendMessageBatchRequest(QueueUrl, sendMesssageReqs);
            var result = _client.SendMessageBatchAsync(batchReq).ConfigureAwait(false).GetAwaiter().GetResult();


            //Check result for failed messages
            if (result.Failed.Count > 0)
            {
                var m = result.Failed.Count.ToString() + " messages sent to Amazon SQS failed. See internal NLog log for details.";

                InternalLogger.Error(m);

                foreach (var f in result.Failed)
                {
                    InternalLogger.Error("Message failed to send. Code = " + f.Code
                        + ", Id = " + f.Id
                        + ", SenderFault = " + f.SenderFault.ToString()
                        + ", Message = " + f.Message);
                }

                if (ThrowExceptions) throw new Exception(m);
            }
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
