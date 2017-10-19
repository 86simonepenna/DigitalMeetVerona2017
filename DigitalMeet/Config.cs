using Newtonsoft.Json;
using System.IO;
using System.Threading.Tasks;

public class Config
{
    public string CognitiveComputeVisionKey { get; set; }
    public string CognitiveOcrEndpoint { get; set; }
     public int ScanTimeDeley { get; set; }
   
    private static Config instance;

    public static Config Instance
    {
        get
        {
            if (instance == null)
            {
                instance = Read();

            }
            return instance;
        }
    }


    private static Config Read()
    {

       var data = File.ReadAllText("config.json");
        return JsonConvert.DeserializeObject<Config>(data);
    }

}