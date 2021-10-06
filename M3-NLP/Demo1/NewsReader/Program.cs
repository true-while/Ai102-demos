using System;
using System.IO;
using System.Text;
using Microsoft.Extensions.Configuration;
using System.Net;
using System.Collections.Generic;
using Azure;
using Azure.AI.TextAnalytics;


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
            string accessKey=  configuration.GetSection("CognitiveServiceKey").Value;
            string region = configuration.GetSection("CognitiveServicesRegion").Value;
            string searchTerm  = configuration.GetSection("SearchTerm").Value;
            string cogendpoint = "https://" + region + ".api.cognitive.microsoft.com/";

            try
            {
                SearchResult result = BingNewsSearch(searchTerm);
                dynamic jsonObj = Newtonsoft.Json.JsonConvert.DeserializeObject(result.jsonResult);
                var docs = new Dictionary<int, string>();
                var key = 0;
                foreach (var news in jsonObj["value"])
                {
                    docs[key++] = news.description;
                }

                AzureKeyCredential credentials = new AzureKeyCredential(accessKey);
                Uri endpoint = new Uri(cogendpoint);
                TextAnalyticsClient CogClient = new TextAnalyticsClient(endpoint, credentials);

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
