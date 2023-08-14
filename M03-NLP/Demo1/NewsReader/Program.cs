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

namespace text_analysis
{
    struct SearchResult
    {
        public String jsonResult;
        public Dictionary<String, String> relevantHeaders;
    }

    class Program
    {
        static IConfiguration configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetParent(AppContext.BaseDirectory).FullName)
            .AddJsonFile("appsettings.json", false)
            .Build();
 
        static void Main()
        {
            string aiAccessKey = configuration.GetSection("CognitiveServiceKey").Value;
            string aiEndpoint = configuration.GetSection("CognitiveServices").Value;

            string searchAccessKey=  configuration.GetSection("SearchKey").Value;
            string searchEndpoint = configuration.GetSection("SearchEndpoint").Value;
            string searchTerm  = configuration.GetSection("SearchTerm").Value;
            string cogendpoint = searchEndpoint + "/v7.0/search";


            try
            {

                // Create a dictionary to store relevant headers
                Dictionary<String, String> relevantHeaders = new Dictionary<String, String>();

                Console.OutputEncoding = Encoding.UTF8;

                Console.WriteLine("Searching the Web for: " + searchTerm);

                // Construct the URI of the search request
                var uriQuery = cogendpoint + "?q=" + Uri.EscapeDataString(searchTerm);

                // Perform the Web request and get the response
                WebRequest request = HttpWebRequest.Create(uriQuery);
                request.Headers["Ocp-Apim-Subscription-Key"] = searchAccessKey;
                HttpWebResponse response = (HttpWebResponse)request.GetResponseAsync().Result;
                string json = new StreamReader(response.GetResponseStream()).ReadToEnd();

                // Extract Bing HTTP headers
                foreach (String header in response.Headers)
                {
                    if (header.StartsWith("BingAPIs-") || header.StartsWith("X-MSEdge-"))
                        relevantHeaders[header] = response.Headers[header];
                }

   
                dynamic parsedJson = JsonConvert.DeserializeObject(json);
                var docs = new Dictionary<int, string>();
                var key = 0;
                foreach (var news in parsedJson["news"].value)
                {
                    docs[key++] = news.description;
                }

                AzureKeyCredential aicredentials = new AzureKeyCredential(aiAccessKey);
                Uri aiendpoint = new Uri(aiEndpoint);
                TextAnalyticsClient CogClient = new TextAnalyticsClient(aiendpoint, aicredentials);

                Console.OutputEncoding = System.Text.Encoding.UTF8;

                foreach (var id in docs.Keys)
                {
                    Console.WriteLine("------------------------------------------------");
                    Console.WriteLine($"News: {docs[id]}");

                    DetectedLanguage detectedLanguage = CogClient.DetectLanguage(docs[id]);
                    Console.WriteLine($"Language: {detectedLanguage.Name}");

                    DocumentSentiment sentimentAnalysis = CogClient.AnalyzeSentiment(docs[id]);
                    Console.WriteLine($"Sentiment: {sentimentAnalysis.Sentiment}");

                    CategorizedEntityCollection entities = CogClient.RecognizeEntities(docs[id]);
                    if (entities.Count > 0)
                    {
                        Console.WriteLine("\nEntities:");
                        foreach (CategorizedEntity entity in entities)
                        {
                            Console.WriteLine($"\t{entity.Text} ({entity.Category})");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            Console.ReadLine();
        }

        static SearchResult BingNewsSearch(string toSearch)
        {
            string region = configuration.GetSection("CognitiveServicesRegion").Value;
            string accessKey=  configuration.GetSection("CognitiveServiceKey").Value;

            var uriBase = $"https://{region}.api.cognitive.microsoft.com/bing/v7.0/news/search";
            var uriQuery = uriBase + "?mkt=en-us&q=" + Uri.EscapeDataString(toSearch);

            WebRequest request = WebRequest.Create(uriQuery);
            request.Headers["Ocp-Apim-Subscription-Key"] = accessKey;
            HttpWebResponse response = (HttpWebResponse)request.GetResponseAsync().Result;
            string json = new StreamReader(response.GetResponseStream()).ReadToEnd();


            // Create the result object for return
            var searchResult = new SearchResult()
            {
                jsonResult = json,
                relevantHeaders = new Dictionary<String, String>()
            };

            // Extract Bing HTTP headers
            foreach (String header in response.Headers)
            {
                if (header.StartsWith("BingAPIs-") || header.StartsWith("X-MSEdge-"))
                    searchResult.relevantHeaders[header] = response.Headers[header];
            }
            return searchResult;
        }

       
    }
}
