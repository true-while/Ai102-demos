using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Xml;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace TranslateFunction
{
    public static class Translate
    {
        static string TranslatorUri = "https://api.microsofttranslator.com/v2/http.svc/";
        static string CognitiveServicesTokenUri = "https://<your-service-name>.cognitiveservices.azure.com/sts/v1.0/issuetoken";
        static string SubscriptionKey = "<your key>";
        static string token;

        [FunctionName("Translate")]
        public static async Task<IActionResult> Run(
             [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequest req,
            ILogger log)
        {

            token = Task.Run(GetBearerTokenForTranslator).Result;
            log.LogInformation("Token requested");

            string recordId = null;
            string originalText = null;
            string originalTo =  req.Query["to"];
            string originalFrom = req.Query["from"];

            log.LogInformation($"to:'{originalTo}' from:'{originalFrom}'");

            string requestBody = new StreamReader(req.Body).ReadToEnd();
            dynamic data = JsonConvert.DeserializeObject(requestBody);


            // Validation
            if (data?.values == null)
            {
                return new BadRequestObjectResult(" Could not find values array");
            }
            if (data?.values.HasValues == false || data?.values.First.HasValues == false)
            {
                return new BadRequestObjectResult("Could not find valid records in values array");
            }            

            WebApiEnricherResponse response = new WebApiEnricherResponse();
            response.values = new List<WebApiResponseRecord>();
            foreach (var record in data?.values)
            {
                log.LogInformation($"record:'{record}'");

                recordId = record.recordId?.Value as string;
                originalText = record.data?.text?.Value as string;
                originalTo = originalTo ?? record.data?.to?.Value as string;
                originalFrom = originalFrom ?? record.data?.from?.Value as string;

                if (recordId == null)
                {
                    return new BadRequestObjectResult("recordId cannot be null");
                }

                // Put together response.
                WebApiResponseRecord responseRecord = new WebApiResponseRecord();
                responseRecord.data = new Dictionary<string, object>();
                responseRecord.recordId = recordId;
                var text = DoTranslate(originalText, originalFrom, originalTo);
                responseRecord.data.Add("text", text);

                log.LogInformation($"text:'{text}'");

                response.values.Add(responseRecord);
            }

            return (ActionResult)new OkObjectResult(response);
        }

        public static async Task<string> GetBearerTokenForTranslator()
        {
            var azureSubscriptionKey = SubscriptionKey;
            var azureAuthToken = new APIToken(azureSubscriptionKey, CognitiveServicesTokenUri);
            return await azureAuthToken.GetAccessTokenAsync();
        }

        public static string DoTranslate(string input, string inputLang, string outputLang)
        {
            try
            {
                var test = ReadRespons($"{TranslatorUri}Translate?text={HttpUtility.UrlEncode(input)}&to={outputLang}" + (string.IsNullOrEmpty(inputLang) ? "" : $"&from={inputLang}"));
                return test;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public static string ReadRespons(string uri)
        {
            WebRequest translationWebRequest = WebRequest.Create(uri);
            translationWebRequest.Headers.Add("Authorization", token);

            using (WebResponse response = translationWebRequest.GetResponse())
            {
                using (Stream stream = response.GetResponseStream())
                {
                    Encoding encode = Encoding.GetEncoding("UTF-8");
                    using (StreamReader translatedStream = new StreamReader(stream, encode))
                    {
                        XmlDocument xTranslation = new XmlDocument();
                        xTranslation.LoadXml(translatedStream.ReadToEnd());
                        return xTranslation.InnerText;
                    }
                }
            }
        }
    }


    public class WebApiResponseError
    {
        public string message { get; set; }
    }

    public class WebApiResponseWarning
    {
        public string message { get; set; }
    }

    public class WebApiResponseRecord
    {
        public string recordId { get; set; }
        public Dictionary<string, object> data { get; set; }
        public List<WebApiResponseError> errors { get; set; }
        public List<WebApiResponseWarning> warnings { get; set; }
    }

    public class WebApiEnricherResponse
    {
        public List<WebApiResponseRecord> values { get; set; }
    }

    public class APIToken
    {
        private Uri srvUrl;
        private TimeSpan cacheDuration = new TimeSpan(0, 3, 0);

        private string _tokenValue = "";
        private DateTime _tokenTime = DateTime.MinValue;

        public string APIKey { get; }

        public APIToken(string key, string url)
        {
            srvUrl = new Uri(url);
            APIKey = key;
        }

        public async Task<string> GetAccessTokenAsync()
        {
            if ((DateTime.Now - _tokenTime) < cacheDuration)
            {
                return _tokenValue;
            }

            HttpRequestMessage request = null;
            HttpClient client = null;

            try
            {
                client = new HttpClient()
                {
                    Timeout = TimeSpan.FromSeconds(180)
                };
                request = new HttpRequestMessage()
                {
                    Method = HttpMethod.Post,
                    RequestUri = srvUrl,
                    Content = new StringContent(string.Empty),
                };
                request.Headers.TryAddWithoutValidation("Ocp-Apim-Subscription-Key", APIKey);

                var response = await client.SendAsync(request);
                response.EnsureSuccessStatusCode();

                var token = await response.Content.ReadAsStringAsync();
                _tokenTime = DateTime.Now;
                _tokenValue = $"Bearer {token}";

                return _tokenValue;

            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                if (client != null) client.Dispose();
                if (request != null) client.Dispose();
            }

        }
    }
}

