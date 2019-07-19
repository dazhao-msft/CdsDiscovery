using Microsoft.Identity.Client;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security;
using System.Threading.Tasks;

namespace CdsDiscovery
{
    class Program
    {
        static async Task Main(string[] args)
        {
            string globalDiscoUrl = "https://globaldisco.crm.dynamics.com/";
            string clientId = "";
            string account = "";
            string password = "";

            var options = new PublicClientApplicationOptions()
            {
                ClientId = clientId,
                AadAuthorityAudience = AadAuthorityAudience.AzureAdMultipleOrgs,
            };

            var app = PublicClientApplicationBuilder
                .CreateWithApplicationOptions(options)
                .Build();

            var securePassword = new SecureString();
            foreach (char c in password)
            {
                securePassword.AppendChar(c);
            }

            var authResult = await app.AcquireTokenByUsernamePassword((new[] { $"{globalDiscoUrl}user_impersonation" }), account, securePassword).ExecuteAsync();

            HttpClient client = new HttpClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authResult.AccessToken);
            client.Timeout = new TimeSpan(0, 2, 0);
            client.BaseAddress = new Uri(globalDiscoUrl);

            HttpResponseMessage response = client.GetAsync("api/discovery/v1.0/Instances", HttpCompletionOption.ResponseHeadersRead).Result;

            var instances = new List<Instance>();

            if (response.IsSuccessStatusCode)
            {
                //Get the response content and parse it.
                string result = response.Content.ReadAsStringAsync().Result;
                JObject body = JObject.Parse(result);
                JArray values = (JArray)body.GetValue("value");

                if (values.HasValues)
                {
                    instances = JsonConvert.DeserializeObject<List<Instance>>(values.ToString());
                }

                Console.WriteLine(result);
            }
            else
            {
                throw new Exception(response.ReasonPhrase);
            }
        }

        /// <summary>
        /// Object returned by the discovery service
        /// </summary>
        class Instance
        {
            public string Id { get; set; }
            public string UniqueName { get; set; }
            public string UrlName { get; set; }
            public string FriendlyName { get; set; }
            public int State { get; set; }
            public string Version { get; set; }
            public string Url { get; set; }
            public string ApiUrl { get; set; }
            public DateTime LastUpdated { get; set; }
        }
    }
}
