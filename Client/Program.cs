using System;
using System.Globalization;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace AzureRelayDemo.Client
{
    class Program
    {
        private const string RelayNamespace = "";
        private const string ConnectionName = "";
        private const string KeyName = "";
        private const string Key = "";

        static async Task Main(string[] args)
        {
            var token = CreateToken($"sb://{RelayNamespace}/{ConnectionName}", KeyName, Key);
            using var client = new HttpClient();

            client.DefaultRequestHeaders.Add("ServiceBusAuthorization", token);
            var result = await client.GetAsync($"https://{RelayNamespace}/{ConnectionName}/api/Products");            

            Console.WriteLine(result);           

            if(result.IsSuccessStatusCode)
            {
                var contents = await result.Content.ReadAsStringAsync();
                Console.WriteLine(contents);
            }

            Console.ReadLine();
        }

        private static string CreateToken(string resourceUri, string keyName, string key)
        {
            TimeSpan sinceEpoch = DateTime.UtcNow - new DateTime(1970, 1, 1);
            var week = 60 * 60 * 24 * 7;
            var expiry = Convert.ToString((int)sinceEpoch.TotalSeconds + week);
            string stringToSign = HttpUtility.UrlEncode(resourceUri) + "\n" + expiry;
            HMACSHA256 hmac = new HMACSHA256(Encoding.UTF8.GetBytes(key));
            var signature = Convert.ToBase64String(hmac.ComputeHash(Encoding.UTF8.GetBytes(stringToSign)));
            var sasToken = String.Format(CultureInfo.InvariantCulture, "SharedAccessSignature sr={0}&sig={1}&se={2}&skn={3}", HttpUtility.UrlEncode(resourceUri), HttpUtility.UrlEncode(signature), expiry, keyName);
            return sasToken;
        }
    }
}
