using Microsoft.Azure.ServiceBus;
using System;
using System.Threading.Tasks;

namespace Hacker1_Monitor
{
    class Program
    {
        private static QueueClient queueClient;
        private const string ServiceBusConnectionString = "Endpoint=sb://iothub-ns-iothubteam-1063841-9c12ea224b.servicebus.windows.net/;SharedAccessKeyName=iothubowner;SharedAccessKey=5KI7ix7zs7T6H/KXaBhyl6J0OlTECXRAkI8qPEs6nkA=";
        private const string QueueName = "iothubteam4-operationmonitoring";

        static void Main(string[] args)
        {
            MainAsync(args).GetAwaiter().GetResult();
        }

        private static async Task MainAsync(string[] args)
        {
            // Creates a ServiceBusConnectionStringBuilder object from the connection string, and sets the EntityPath.
            var connectionStringBuilder = new ServiceBusConnectionStringBuilder(ServiceBusConnectionString)
            {
                EntityPath = QueueName
            };

            // Initializes the static QueueClient variable that will be used in the ReceiveMessages method.
            queueClient = new QueueClient(connectionStringBuilder,ReceiveMode.PeekLock);

            await ReceiveMessages();

            // Close the client after the ReceiveMessages method has exited.
            await queueClient.CloseAsync();
        }

        // Receives messages from the queue in a loop
        private static async Task ReceiveMessages()
        {
            Console.WriteLine("Press ctrl-c to exit receive loop.");
            while (true)
            {
                try
                {
                    // Receive the next message from the queue
                    var message = await queueClient.

                    // Write the message body to the console
                    Console.WriteLine($"Received message: {message.GetBody<string>()}");

                    // Complete the messgage so that it is not received again
                    await message.CompleteAsync();
                }
                catch (Exception exception)
                {
                    Console.WriteLine($"{DateTime.Now} > Exception: {exception.Message}");
                }

                // Delay by 10 milliseconds so that the console can keep up
                await Task.Delay(10);
            }
        }

    }
}
