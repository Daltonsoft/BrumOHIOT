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
            var uri = "https://iotHubTeam4-Martin.azure-devices.net/devices/Hacker1Device/messages/events?api-version=2016-02-03";
            var sas = "SharedAccessSignature sr=iotHubTeam4-Martin.azure-devices.net%2Fdevices%2FHacker1Device&sig=U%2BDsHTk9KNfxS9r6ep8nKbtorti8jVkEvuA63uSmwx0%3D&se=1544538219";

            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(uri);
                client.DefaultRequestHeaders.TryAddWithoutValidation("Authorization", sas);

                var data = new PostData() { entryTime = DateTime.UtcNow, ticketId = Guid.NewGuid().ToString() };

                HttpContent contentPost = new StringContent(JsonConvert.SerializeObject(data), Encoding.UTF8, "application/json");

                var response = await client.PostAsync(uri, contentPost);

                var responseString = await response.Content.ReadAsStringAsync();

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
