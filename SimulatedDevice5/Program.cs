using Microsoft.Azure.Devices.Client;
using Microsoft.Azure.EventHubs;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TransportType = Microsoft.Azure.Devices.Client.TransportType;

namespace SimulatedDevice5
{
    class Program
    {
        private static readonly string defaultSasKey = "5KI7ix7zs7T6H/KXaBhyl6J0OlTECXRAkI8qPEs6nkA=";
        private static readonly string defaultPath = "iothubteam4";
        private static readonly string defaultEndpoint = "sb://iothub-ns-iothubteam-1063841-9c12ea224b.servicebus.windows.net/";
        private static readonly string defaultConnectionString = "HostName=iotHubTeam4.azure-devices.net;DeviceId=Hacker5Device;SharedAccessKey=RcjKuynhLAY/J+zy3pP0iRXt2tY5Lntp99uT9u4QZl0=";

        static void Main(string[] args)
        {
            Console.Write("Please select an option 1 (send) ,2 (get) , or exit (anything else)");
            var key = Console.ReadLine();
            switch (key)
            {
                case "1":
                    SendDeviceToCloudMessageAsyc();
                    Console.ReadLine();
                    break;
                case "2":
                    ReadDataFromIotDevice();
                    Console.ReadLine();
                    break;
            }
        }


        private static async Task ReadDataFromIotDevice()
        {
            Console.WriteLine("enter the hub endpoint");
            var endpoint = Console.ReadLine();
            Console.WriteLine("enter the path");
            var path = Console.ReadLine();
            var sasKeyName = "iothubowner";
            Console.WriteLine("enter the sas key");
            var sasKey = Console.ReadLine();


            if (string.IsNullOrWhiteSpace(endpoint))
                endpoint = defaultEndpoint;
            if (string.IsNullOrWhiteSpace(path))
                path = defaultPath;
            if (string.IsNullOrWhiteSpace(sasKey))
                sasKey = defaultSasKey;

            var connectionString = new EventHubsConnectionStringBuilder(new Uri(endpoint), path, sasKeyName, sasKey);
            var client = EventHubClient.CreateFromConnectionString(connectionString.ToString());


            var runtimeInfo = await client.GetRuntimeInformationAsync();
            var deviceToCloudPartitions = runtimeInfo.PartitionIds;

            CancellationTokenSource cts = new CancellationTokenSource();

            Console.CancelKeyPress += (s, e) =>
            {
                e.Cancel = true;
                cts.Cancel();
                Console.WriteLine("Exiting...");
            };

            var tasks = new List<Task>();

            foreach (string partition in deviceToCloudPartitions)
            {
                tasks.Add(ReceiveMessagesFromDeviceAsync(partition, client, cts.Token));
            }

            Task.WaitAll(tasks.ToArray());
        }

        private static async Task ReceiveMessagesFromDeviceAsync(string partition, EventHubClient client, CancellationToken token)
        {
            var eventHubReceiver = client.CreateReceiver("$Default", partition, EventPosition.FromEnqueuedTime(DateTime.Now));
            Console.WriteLine("Create receiver on partition: " + partition);
            while (true)
            {
                if (token.IsCancellationRequested)
                    break;
                Console.WriteLine("Listening for messages on: " + partition);
                var events = await eventHubReceiver.ReceiveAsync(100);

                if (events == null)
                    continue;

                foreach (EventData eventData in events)
                {
                    string data = Encoding.UTF8.GetString(eventData.Body.Array);
                    Console.WriteLine("Message received on partition {0}:", partition);
                    Console.WriteLine("  {0}:", data);
                    Console.WriteLine("Application properties (set by device):");
                    foreach (var prop in eventData.Properties)
                    {
                        Console.WriteLine("  {0}: {1}", prop.Key, prop.Value);
                    }
                    Console.WriteLine("System properties (set by IoT Hub):");
                    foreach (var prop in eventData.SystemProperties)
                    {
                        Console.WriteLine("  {0}: {1}", prop.Key, prop.Value);
                    }
                }
            }
        }

        private static async Task SendDeviceToCloudMessageAsyc()
        {
            Console.WriteLine("Sending Data To Hub");
            Console.WriteLine("Please enter the connection string");
            var connectionString = Console.ReadLine();

            if (string.IsNullOrWhiteSpace(connectionString))
                connectionString = defaultConnectionString;

            try
            {
                var deviceClient = DeviceClient.CreateFromConnectionString(connectionString, TransportType.Mqtt);

                while (true)
                {
                    var data = JsonConvert.SerializeObject(new
                    {
                        ticketId = Guid.NewGuid(),
                        entryTime = DateTime.UtcNow
                    });

                    var message = new Message(Encoding.ASCII.GetBytes(data));

                    await deviceClient.SendEventAsync(message);
                    Console.WriteLine("{0} > Sending Message: {1}", DateTime.UtcNow, data);
                    await Task.Delay(1000);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }
    }
}
