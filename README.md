# NLog.Targets.SQS

[NLog](http://nlog-project.org/) target for [Amazon SQS](https://aws.amazon.com/sqs/). This target will publish log messages to a specified Amazon SQS queue.

License: [MIT](https://raw.githubusercontent.com/aireq/NLog.Targets.SQS/master/LICENSE).

### Example Config

```xml
    <target xsi:type="SQS"
            name="SQS Target Name"
            layout="${message}"
            ThrowExceptions="false"
            DelaySeconds="0"            
            BatchSize="10" 
            RegionEndpoint ="us-west-1"
            QueueUrl ="https://sqs.us-west-1.amazonaws.com/000000000000/sqs-queue-name"
            AwsAccessKeyId="XXXXXXX"
            AwsSecretAccessKey="YYYYYYYYYY"/>
```

###General Options
_name_ - Name of the target.

_layout_ - Message to be sent. Default: `${longdate}|${level:uppercase=true}|${logger}|${message}`

_ThrowExceptions_ - Will the target throw exceptions. Default: false.

###Amazon SQS Options

_DelaySeconds_ -  Postpone the delivery of new messages by X seconds. Default: 0

_BatchSize_ - Number of messages to send in batches (1 to 10). Default: 10

_RegionEndpoint_ - The AWS region endpoint name. **Required**.

_QueueUrl_ - The URL of the target AWS SQS queue. **Required**.

_AwsAccessKeyId_ - AWS Access Key Id used to access the SQS queue. **Required**.

_AwsSecretAccessKey_ - AWS Secrete Access Key used to access the SQS queue. **Required**.
