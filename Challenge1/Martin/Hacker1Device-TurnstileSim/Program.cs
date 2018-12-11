using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Hacker1Device_TurnstileSim
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Press ESC to stop");
            do
            {
                while (!Console.KeyAvailable)
                {
                    int milliseconds = 1000; //1 second = 1000 milliseconds
                    Thread.Sleep(milliseconds);
                    SendEventAsync().Wait();
                }
            } while (Console.ReadKey(true).Key != ConsoleKey.Escape);
        }

        static async Task SendEventAsync()
        {
            var uri = "https://iotHubTeam4.azure-devices.net/devices/Hacker1Device/messages/events?api-version=2016-02-03";
            var sas = "SharedAccessSignature sr=iotHubTeam4.azure-devices.net%2Fdevices%2FHacker1Device&sig=K9aHqSTzuVw5w5hlC67a%2FR0dwpHETqCRp1BzuoGEkLc%3D&se=1544540157";

            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(uri);
                client.DefaultRequestHeaders.TryAddWithoutValidation("Authorization", sas);
                var data = new PostData() { entryTime = DateTime.UtcNow, ticketId = Guid.NewGuid().ToString() };
                var dataJson = JsonConvert.SerializeObject(data);
                var contentPost = new StringContent(dataJson, Encoding.UTF8, "application/json");
                var response = await client.PostAsync(uri, contentPost);
                if (response.IsSuccessStatusCode)
                {
                    Console.WriteLine($"Sent {data.ticketId} sucesfully");
                }
                else
                {
                    Console.WriteLine($"{data.ticketId} failed");
                }
            }

        }
    }

    class PostData
    {
        public string ticketId { get; set; }
        public DateTime entryTime { get; set; }
    }
}
