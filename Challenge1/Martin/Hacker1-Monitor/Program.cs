using Microsoft.ServiceBus.Messaging;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Hacker1_Monitor
{
    class Program
    {
        static string connectionString = "Endpoint=sb://iothub-ns-iothubteam-1063874-205ecba883.servicebus.windows.net/;SharedAccessKeyName=iothubowner;SharedAccessKey=iU/2kWT+XhKWHFbisUQim7dUrUpoY084vdKTSg6EoMc=";
        static string monitoringEndpointName = "iothubteam4-martin-operationmonitoring";
        static EventHubClient eventHubClient;

        static void Main(string[] args)
        {
            Console.WriteLine("Monitoring. Press Enter key to exit.\n");

            eventHubClient = EventHubClient.CreateFromConnectionString(connectionString, monitoringEndpointName);
            var d2cPartitions = eventHubClient.GetRuntimeInformation().PartitionIds;
            CancellationTokenSource cts = new CancellationTokenSource();
            var tasks = new List<Task>();

            foreach (string partition in d2cPartitions)
            {
                tasks.Add(ReceiveMessagesFromDeviceAsync(partition, cts.Token));
            }

            Console.ReadLine();
            Console.WriteLine("Exiting...");
            cts.Cancel();
            Task.WaitAll(tasks.ToArray());
        }

        private static async Task ReceiveMessagesFromDeviceAsync(string partition, CancellationToken ct)
        {
            var eventHubReceiver = eventHubClient.GetDefaultConsumerGroup().CreateReceiver(partition, DateTime.UtcNow);
            while (true)
            {
                if (ct.IsCancellationRequested)
                {
                    await eventHubReceiver.CloseAsync();
                    break;
                }

                EventData eventData = await eventHubReceiver.ReceiveAsync(new TimeSpan(0, 0, 10));

                if (eventData != null)
                {
                    string data = Encoding.UTF8.GetString(eventData.GetBytes());
                    Console.WriteLine("Message received. Partition: {0} Data: '{1}'", partition, data);
                }
            }
        }
    }
}
