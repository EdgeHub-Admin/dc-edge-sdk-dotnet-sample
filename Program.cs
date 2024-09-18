using EdgeSync360.EdgeHub.Edge.DotNet.SDK;
using EdgeSync360.EdgeHub.Edge.DotNet.SDK.Model;

EdgeAgentOptions options = new EdgeAgentOptions()
{
    ConnectType = ConnectType.AzureIoTHub,                      // Connection type (DCCS, MQTT). The default is DCCS.
    DCCS = new DCCSOptions()                                    // If ConnectType is DCCS, the following options must be entered:
    {
        CredentialKey = "0cfe34bb4b0ebf504888c557179a1cf1",     // Credential Key
        APIUrl = "http://api-dccs-ensaas.isghpc.wise-paas.com/" // DCCS API Url
    },
    MQTT = new MQTTOptions()                                    // If ConnectType is MQTT, the following options must be entered:
    {
        HostName = "127.0.0.1",
        Port = 1883,
        Username = "admin",
        Password = "admin",
        ProtocolType = Protocol.TCP
    },
    AzureIoTHub = new AzureIoTHubOptions()
    {
        HostName = "edge365-dev.azure-devices.net",
        SASToken = "SharedAccessSignature sr=edge365-dev.azure-devices.net%2Fdevices%2F47fe2070-7405-11ef-bf44-299ec24cc191&sig=TjXYa9ESCgNNL%2FaD5lEbyfp5eQS38LS3S8kDntNAALc%3D&se=4880075151&skn=device"
    },
    UseSecure = false,
    AutoReconnect = true,
    ReconnectInterval = 1000,
    NodeId = "47fe2070-7405-11ef-bf44-299ec24cc191",        // Get from portal
    Type = EdgeType.Gateway,                                // Configure the edge type as Gateway or Device. The default is Gateway.
    DeviceId = "SmartDevice1",                              // If the type is Device, the DeviceId must be entered. 
    Heartbeat = 60000,                                      // The default is 60 seconds.
    DataRecover = true                                      // Configure whether to recover data when disconnected.
};
EdgeAgent edgeAgent = new EdgeAgent(options);

edgeAgent.Connected += edgeAgent_Connected!;
edgeAgent.Disconnected += edgeAgent_Disconnected!;
edgeAgent.MessageReceived += edgeAgent_MessageReceived!;

Console.WriteLine("connecting ...");
await edgeAgent.Connect();
Console.WriteLine("connected!");


EdgeConfig.DeviceConfig device = new EdgeConfig.DeviceConfig()
{
    Id = "Device1",
    Name = "Device1",
    Type = "Smart Device 1",
    Description = "Device 1"
};

EdgeConfig.AnalogTagConfig analogTag = new EdgeConfig.AnalogTagConfig()
{
    Name = "ATag",
    Description = "ATag",
    ReadOnly = false,
    ArraySize = 0,
    SpanHigh = 1000,
    SpanLow = 0,
    EngineerUnit = "V",
    IntegerDisplayFormat = 4,
    FractionDisplayFormat = 2,
    SendWhenValueChanged = true     // Data will only be sent when the value changed
};

EdgeConfig.DiscreteTagConfig discreteTag = new EdgeConfig.DiscreteTagConfig()
{
    Name = "DTag",
    Description = "DTag",
    ReadOnly = false,
    ArraySize = 0,
    State0 = "0",
    State1 = "1",
    State2 = "",
    State3 = "",
    State4 = "",
    State5 = "",
    State6 = "",
    State7 = "",
    SendWhenValueChanged = true     // Data will only be sent when the value changed
};

EdgeConfig.TextTagConfig textTag = new EdgeConfig.TextTagConfig()
{
    Name = "TTag",
    Description = "TTag",
    ReadOnly = false,
    ArraySize = 0,
    SendWhenValueChanged = true     // Data will only be sent when the value changed
};

var blockConfig = new EdgeConfig.BlockConfig("Pump");
blockConfig.AnalogTagList.Add(analogTag);
blockConfig.DiscreteTagList.Add(discreteTag);
blockConfig.TextTagList.Add(textTag);

device.AddBlock("Pump01", blockConfig);
device.AddBlock("Pump02", blockConfig);

EdgeConfig config = new EdgeConfig();
config.Node = new EdgeConfig.NodeConfig();
config.Node.DeviceList.Add(device);

Console.WriteLine("upload config ...");
var isConfigAck = false;
bool result = edgeAgent.UploadConfig(ActionType.Delsert, config).Result;
Console.WriteLine(String.Format("uploaded! result is {0}", result));

while (true)
{
    if (isConfigAck)
    {
        break;
    }

    Console.WriteLine("waiting for config to ack ...");
    Thread.Sleep(1000);
}

Random random = new Random();

for (int i = 0; i < 10; i++)
{
    EdgeData data = new EdgeData();
    foreach (var tag in device.AnalogTagList)
    {
        EdgeData.Tag aTag = new EdgeData.Tag()
        {
            DeviceId = device.Id,
            TagName = tag.Name,
            Value = random.NextDouble()
        };
        data.TagList.Add(aTag);
    }

    foreach (var tag in device.DiscreteTagList)
    {
        EdgeData.Tag dTag = new EdgeData.Tag()
        {
            DeviceId = device.Id,
            TagName = tag.Name,
            Value = random.NextInt64() % 2
        };
        data.TagList.Add(dTag);
    }

    foreach (var tag in device.TextTagList)
    {
        EdgeData.Tag tTag = new EdgeData.Tag()
        {
            DeviceId = device.Id,
            TagName = tag.Name,
            Value = random.NextSingle()
        };
        data.TagList.Add(tTag);
    }

    data.Timestamp = DateTime.Now;
    result = edgeAgent.SendData(data).Result;
    Console.WriteLine(String.Format("data {0} is sent! result is {1}", i + 1, result));

    Thread.Sleep(1000);
}

await edgeAgent.Disconnect();

void edgeAgent_Connected(object sender, EdgeAgentConnectedEventArgs e)
{
    // Connected
    Console.WriteLine("handle connected event");
}

void edgeAgent_Disconnected(object sender, DisconnectedEventArgs e)
{
    // Disconnected
    Console.WriteLine("handle disconnected event");
}

void edgeAgent_MessageReceived(object sender, MessageReceivedEventArgs e)
{
    Console.WriteLine("handle message received event, type: " + e.Type + ", message: ", e.Message);
    switch (e.Type)
    {
        case MessageType.WriteValue:
            WriteValueCommand wvcMsg = (WriteValueCommand)e.Message;
            foreach (var device in wvcMsg.DeviceList)
            {
                Console.WriteLine("deviceId: {0}", device.Id);
                foreach (var tag in device.TagList)
                {
                    Console.WriteLine("tagName: {0}, value: {1}", tag.Name, tag.Value.ToString());
                }
            }
            break;
        case MessageType.WriteConfig:
            break;
        case MessageType.ConfigAck:
            ConfigAck cfgAckMsg = (ConfigAck)e.Message;
            Console.WriteLine("upload config result: {0}", cfgAckMsg.Result.ToString());
            isConfigAck = true;
            break;
    }
}
