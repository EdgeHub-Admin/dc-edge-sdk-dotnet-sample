using EdgeSync360.EdgeHub.Edge.DotNet.SDK;
using EdgeSync360.EdgeHub.Edge.DotNet.SDK.Model;

EdgeAgentOptions options = new EdgeAgentOptions()
{
    ConnectType = ConnectType.DCCS,                         // Connection type (DCCS, MQTT). The default is DCCS.
    DCCS = new DCCSOptions()                                // If ConnectType is DCCS, the following options must be entered:
    {
        CredentialKey = "YOUR_CREDENTIAL_KEY",              // Credential Key
        APIUrl = "YOUR_API_URL"                             // DCCS API Url
    },
    MQTT = new MQTTOptions()                                // If ConnectType is MQTT, the following options must be entered:
    {
        HostName = "127.0.0.1",
        Port = 1883,
        Username = "admin",
        Password = "admin",
        ProtocolType = Protocol.TCP
    },
    UseSecure = false,
    AutoReconnect = true,
    ReconnectInterval = 1000,
    NodeId = "YOUR_NODE_ID",                                // Get from portal
    Type = EdgeType.Gateway,                                // Configure the edge type as Gateway or Device. The default is Gateway.
    DeviceId = "SmartDevice1",                              // If the type is Device, the DeviceId must be entered. 
    Heartbeat = 60000,                                      // The default is 60 seconds.
    DataRecover = true                                      // Configure whether to recover data when disconnected.
};
EdgeAgent edgeAgent = new EdgeAgent(options);

Console.WriteLine("connecting ...");
await edgeAgent.Connect();
while (!edgeAgent.IsConnected)
{
    Thread.Sleep(1000);
}
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
edgeAgent.UploadConfig(ActionType.Delsert, config).Wait();
Console.WriteLine(String.Format("uploaded!"));


for (var i = 0; i < 10; i++)
{
    Random random = new Random();
    EdgeData data = new EdgeData();

    foreach (var d in config.Node.DeviceList)
    {
        foreach (var tag in d.AnalogTagList)
        {
            EdgeData.Tag aTag = new EdgeData.Tag()
            {
                DeviceId = d.Id,
                TagName = tag.Name,
                Value = random.NextDouble()
            };

            data.TagList.Add(aTag);
        }

        foreach (var tag in d.DiscreteTagList)
        {
            EdgeData.Tag dTag = new EdgeData.Tag()
            {
                DeviceId = d.Id,
                TagName = tag.Name,
                Value = random.Next() % 2
            };

            data.TagList.Add(dTag);
        }

        foreach (var tag in d.TextTagList)
        {
            EdgeData.Tag tTag = new EdgeData.Tag()
            {
                DeviceId = d.Id,
                TagName = tag.Name,
                Value = "test"
            };

            data.TagList.Add(tTag);
        }
    }

    data.Timestamp = DateTime.Now;
    Console.WriteLine(String.Format("upload data ... {0}", data.Timestamp));
    await edgeAgent.SendData(data);
    Console.WriteLine(String.Format("uploaded!"));
    Thread.Sleep(1000);
}
