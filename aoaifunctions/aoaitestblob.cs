using System;
using System.IO;
using System.Net.Http;
using System.Net;
using aoaifunctions.Entities;
using aoaifunctions.Helpers;
using aoaifunctions.ResponseEntities;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace aoaifunctions
{
    public class aoaitestblob
    {
        [FunctionName("aoaitestblob")]
        public void Run([BlobTrigger("aoaitest/{name}", Connection = "AzureWebJobsStorage")]Stream myBlob, string name, ILogger log)
        {
            log.LogInformation($"AOAI Blob Function Test is being executed - blob\n Name:{name} \n Size: {myBlob.Length} Bytes");

            using (var sr = new StreamReader(myBlob))
            {
                // Deserialize the JSON data (adjust the type as needed)
                var data = sr.ReadToEnd();

                log.LogInformation($"{data}");

                OpenAIHelper openAIHelper = new OpenAIHelper();
                var response = openAIHelper.GetFlightData(data);

                log.LogInformation(JsonHelper.Serializer<SkillResponse>(response));
            }

        }
    }
}
