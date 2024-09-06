using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Net.Http;
using System.Net;
using aoaifunctions.Entities;
using aoaifunctions.Helpers;
using aoaifunctions.ResponseEntities;

namespace aoaifunctions
{
    public static class aoaitesthttp
    {
        [FunctionName("aoaitesthttp")]
        public static async Task<HttpResponseMessage> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("AOAI HTTP Function Test is being executed...");

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            log.LogInformation(requestBody);

            OpenAIHelper openAIHelper = new OpenAIHelper();
            var response = openAIHelper.GetAllData(requestBody);

            var httpResponse = new HttpResponseMessage(HttpStatusCode.OK);
            httpResponse.Content = new StringContent(JsonHelper.Serializer<SkillResponse>(response), System.Text.Encoding.UTF8, "application/json");
            return httpResponse;
        }
    }
}
