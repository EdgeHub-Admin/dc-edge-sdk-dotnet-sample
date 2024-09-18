# EdgeSync360 EdgeHub DotNet SDK Example

This repository provides an example of how to use the EdgeSync360.EdgeHub.Edge.DotNet.SDK to connect an edge device to the EdgeHub, configure devices, upload data, and send real-time telemetry using different types of tags.

# Prerequisites

.NET 6.0 or later installed on your machine
A valid API Key and Node ID from the EdgeSync360 EdgeHub platform

# Installation

1. Clone this repository:

```bash
git clone https://github.com/your-repository/edgesync360-sdk-example.git
cd edgesync360-sdk-example
```

2. Install the required SDK by adding the EdgeSync360.EdgeHub.Edge.DotNet.SDK to your project.

You can do this using the .csproj file or by running:

```bash
dotnet add package EdgeSync360.EdgeHub.Edge.DotNet.SDK
```

# Usage

1. Set Up the Configuration

   In `Program.cs`, the `EdgeAgentOptions` class is used to configure the connection settings. You will need to replace the placeholders with your actual configuration values.

   ```csharp
   EdgeAgentOptions options = new EdgeAgentOptions()
   {
       ConnectType = ConnectType.DCCS,                         // DCCS is the default connection type.
       DCCS = new DCCSOptions()
       {
           CredentialKey = "YOUR_CREDENTIAL_KEY",              // Replace with your actual DCCS Credential Key
           APIUrl = "YOUR_API_URL"                             // Replace with the API URL of the DCCS service
       },
       MQTT = new MQTTOptions()                                // If using MQTT, update these settings accordingly:
       {
           HostName = "127.0.0.1",
           Port = 1883,
           Username = "admin",                                 // Replace with your MQTT username
           Password = "admin",                                 // Replace with your MQTT password
           ProtocolType = Protocol.TCP
       },
       AzureIoTHub = new AzureIoTHubOptions()
       {
           HostName = "YOUR_HOST_NAME",
           SASToken = "YOUR_SAS_TOKEN"
       },
       NodeId = "YOUR_NODE_ID",                                // Replace with your Node ID
       Type = EdgeType.Gateway,                                // Gateway or Device, default is Gateway
       DeviceId = "SmartDevice1",                              // Device ID if type is set to Device
   };
   ```

- CredentialKey: Your DCCS API credential key.
- APIUrl: The URL for the DCCS service.
- NodeId: The node identifier from the portal.
- HostName: azure iot hub host name.
- SASToken: azure iot hub sas token.

2. Run the Program

   This code connects to the EdgeHub, uploads device configurations, and sends telemetry data in real-time.

   You can run the program by executing the following command:

   ```bash
   dotnet run
   ```

   The program will:

   1. Connect to the EdgeHub.
   2. Upload a configuration that defines devices, blocks, and tags.
   3. Periodically upload simulated data (analog, discrete, and text tag data) every second for 10 iterations.

3. Code Overview

   - EdgeAgentOptions: Defines the connection and edge configuration.
   - DCCSOptions: Holds DCCS-specific settings like the CredentialKey and APIUrl.
   - MQTTOptions: Holds MQTT-specific settings like HostName, Port, and credentials.
   - EdgeAgent: The main agent responsible for connecting and uploading data to the EdgeHub.
   - EdgeConfig: Contains the configuration for devices and tags (analog, discrete, text).
   - EdgeData: Represents the telemetry data being sent.

4. Customize the Configuration
   In Program.cs, you can customize the devices and tags by modifying the following sections:

   - AnalogTagConfig: Configuration for an analog sensor tag.
   - DiscreteTagConfig: Configuration for a discrete tag with binary states.
   - TextTagConfig: Configuration for a text-based tag.

   Example for adding a new tag:

   ```csharp
   EdgeConfig.AnalogTagConfig newAnalogTag = new EdgeConfig.AnalogTagConfig()
   {
       Name = "NewAnalogTag",
       Description = "New Analog Tag Description",
       SpanHigh = 100,
       SpanLow = 0,
       EngineerUnit = "Celsius"
   };
   blockConfig.AnalogTagList.Add(newAnalogTag);
   ```

5. Uploading Data
   Simulated data is uploaded using the EdgeAgent.SendData() method. In the example, the telemetry data is generated randomly and sent every second.

   ```csharp
   for (var i = 0; i < 10; i++)
   {
       EdgeData data = new EdgeData();
       data.Timestamp = DateTime.Now;
       // Add analog, discrete, and text tag data here
       await edgeAgent.SendData(data);
   }
   ```

6. Error Handling
   Ensure the connection to EdgeHub is successful before proceeding with uploading data. If the connection fails, it will retry until a connection is established.

   ```csharp
   Console.WriteLine("connecting ...");
   await edgeAgent.Connect();
   while (!edgeAgent.IsConnected)
   {
       Thread.Sleep(1000);
   }
   Console.WriteLine("connected!");
   ```

# Conclusion

This example demonstrates how to use the EdgeSync360.EdgeHub.Edge.DotNet.SDK to connect, configure devices, and send telemetry data to EdgeHub. Customize the tags and device configurations as needed for your specific use case.
