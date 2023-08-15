using System;
using System.IO;
using System.Text;
using Microsoft.Extensions.Configuration;
using System.Net;
using System.Collections.Generic;
using Azure;
using Azure.AI.TextAnalytics;
using static System.Net.WebRequestMethods;
using Newtonsoft.Json;
using System.Net.Http;
using System.Threading.Tasks;
using text_analysis;

namespace translatordemo
{

    class Program
    {
        static IConfiguration configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetParent(AppContext.BaseDirectory).FullName)
            .AddJsonFile("appsettings.json", false)
            .Build();

        static string key = configuration.GetSection("CognitiveServicesKey").Value;
        static string endpoint = configuration.GetSection("CognitiveServicesUrl").Value;
        static string region = configuration.GetSection("CognitiveServicesRegion").Value; 

        static void Main()
        {
            var senstences = new Dictionary<string, string>() {
                {  "it", "Hello, what is your name?"},
                {  "es", "Hello, what is your name?"},
                {  "de", "Hello, what is your name?"}
            };

            foreach (string lang in senstences.Keys )
            {
                Console.WriteLine(senstences[lang]);
               TranslateAsync(senstences[lang], lang, "en").Wait();
            }
        }
 
        static async Task TranslateAsync(string textToTranslate,string toLang, string fromLang)
        {
            string route = $"/translate?api-version=3.0&from={fromLang}&to={toLang}";

            object[] body = new object[] { new { Text = textToTranslate } };
            var requestBody = JsonConvert.SerializeObject(body);

            using (var client = new HttpClient())
            using (var request = new HttpRequestMessage())
            {
                // Build the request.
                request.Method = HttpMethod.Post;
                request.RequestUri = new Uri(endpoint + route);
                request.Content = new StringContent(requestBody, Encoding.UTF8, "application/json");
                request.Headers.Add("Ocp-Apim-Subscription-Key", key);
                // location required if you're using a multi-service or regional (not global) resource.
                request.Headers.Add("Ocp-Apim-Subscription-Region", region);

                // Send the request and get response.
                HttpResponseMessage response = await client.SendAsync(request).ConfigureAwait(false);
                // Read response as a string.
                string json = await response.Content.ReadAsStringAsync();
               
                //Results parsedJson = JsonConvert.pa.DeserializeObject<Results>(json);

                Results[] parsedJson = System.Text.Json.JsonSerializer.Deserialize<Results[]>(json);

                Console.WriteLine($"Translation to '{toLang}' : '{parsedJson[0].translations[0].text}'");
            }
        } 
       
    }
}
