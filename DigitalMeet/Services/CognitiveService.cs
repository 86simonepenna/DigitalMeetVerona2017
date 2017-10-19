
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Net.Http;
using Newtonsoft.Json;
using System.Net.Http.Headers;
using DigitalMeet.Models;
using System.Collections.Generic;

namespace DigitalMeet.Services
{
    public class CognitiveService
    {
        private HttpClient httpClient;

        public CognitiveService()
        {
            httpClient = new HttpClient();
        }

        public string ReadText(string text)
        {
            return "ok";
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

    }
}
