
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Net.Http;
using Newtonsoft.Json;
using System.Net.Http.Headers;
using DigitalMeet.Models;
using System.Collections.Generic;
using System.Xml.Linq;

namespace DigitalMeet.Services
{
    public class CognitiveService
    {
        private HttpClient httpClient;

        public CognitiveService()
        {
            httpClient = new HttpClient();
        }

        public async Task<IList<string>> ReadTextFromImage(byte[] image)
        {
            httpClient.DefaultRequestHeaders.Clear();
            httpClient.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", Config.Instance.CognitiveComputeVisionKey);

            var content = new ByteArrayContent(image);
            content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");

            HttpResponseMessage response = await httpClient.PostAsync(new Uri(Config.Instance.CognitiveOcrEndpoint), content);

            IList<string> wordsRecognized = new List<string>();

            var ocrResult = JsonConvert.DeserializeObject<OcrResult>(await response.Content.ReadAsStringAsync());

            if (ocrResult != null && ocrResult.regions != null)
            {
                foreach (var region in ocrResult.regions)
                {
                    foreach (var line in region.Lines)
                    {
                        foreach (var word in line.Words)
                        {
                            wordsRecognized.Add(word.Text);
                        }
                    }
                }
            }
            return wordsRecognized;
        }

        public async Task<IList<string>> TranslateRecognizedWords(IList<string> recognizedWords, string fromLanguage, string toLanguage)
        {

            Windows.Web.Http.HttpClient client = new Windows.Web.Http.HttpClient();

            client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", Config.Instance.MicrosoftTranslatorKey);

            string body = "<TranslateArrayRequest>" +
                           "<AppId />" +
                           "<From>{0}</From>" +
                           "<Options>" +
                           " <Category xmlns=\"http://schemas.datacontract.org/2004/07/Microsoft.MT.Web.Service.V2\" />" +
                               "<ContentType xmlns=\"http://schemas.datacontract.org/2004/07/Microsoft.MT.Web.Service.V2\">{1}</ContentType>" +
                               "<ReservedFlags xmlns=\"http://schemas.datacontract.org/2004/07/Microsoft.MT.Web.Service.V2\" />" +
                               "<State xmlns=\"http://schemas.datacontract.org/2004/07/Microsoft.MT.Web.Service.V2\" />" +
                               "<Uri xmlns=\"http://schemas.datacontract.org/2004/07/Microsoft.MT.Web.Service.V2\" />" +
                               "<User xmlns=\"http://schemas.datacontract.org/2004/07/Microsoft.MT.Web.Service.V2\" />" +
                           "</Options>";

            string wordsToTranslate = "<Texts>";
            foreach (var word in recognizedWords)
            {
                wordsToTranslate += "<string xmlns=\"http://schemas.microsoft.com/2003/10/Serialization/Arrays\">{0}</string>";
                wordsToTranslate = String.Format(wordsToTranslate, word);
            }
            wordsToTranslate += "</Texts>";
            body += (wordsToTranslate + "<To>{2}</To>" + "</TranslateArrayRequest>");

            string stringContent = String.Format(body, fromLanguage, "text/plain", toLanguage);
            var content = new Windows.Web.Http.HttpStringContent(stringContent, Windows.Storage.Streams.UnicodeEncoding.Utf8, "text/xml");

            Windows.Web.Http.HttpResponseMessage response = await client.PostAsync(new Uri(Config.Instance.MicrosoftTranslatorEndpoint), content);
            var result = await response.Content.ReadAsStringAsync();

            IList<string> translatedWords = new List<string>();

            switch (response.StatusCode)
            {
                case Windows.Web.Http.HttpStatusCode.Ok:
                    var doc = XDocument.Parse(result);
                    var ns = XNamespace.Get("http://schemas.datacontract.org/2004/07/Microsoft.MT.Web.Service.V2");
                    foreach (XElement xe in doc.Descendants(ns + "TranslateArrayResponse"))
                    {
                        foreach (var node in xe.Elements(ns + "TranslatedText"))
                        {
                            translatedWords.Add(node.Value);
                        }
                    }
                    break;
                default:
                    Console.WriteLine("Request status code is: {0}.", response.StatusCode);
                    Console.WriteLine("Request error message: {0}.", result);
                    break;
            }

            return translatedWords;

        }
    }
}
