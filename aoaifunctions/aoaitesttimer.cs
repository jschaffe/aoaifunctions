using System;
using aoaifunctions.Helpers;
using aoaifunctions.ResponseEntities;
using System.Net.Http;
using System.Net;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;

namespace aoaifunctions
{
    public class aoaitesttimer
    {
        [FunctionName("aoaitesttimer")]
        public void Run([TimerTrigger("0 0 * * * *")]TimerInfo myTimer, ILogger log)
        {
            log.LogInformation("AOAI Timer Function Test is being executed...");

            string inputData = "{\r\n    \"values\": [\r\n      {\r\n        \"recordId\": \"0\",\r\n        \"data\":\r\n           {\r\n             \"SourceName\": \"PARIS\",\r\n             \"Flight\": \"\",\r\n             \"DisplaySummary\": \"On 5/2/2019 at approximately 1038 hours, DELTA #4534 Pre-Check passenger Tony Stark presented his carry-on bag to CVG, Terminal 1, C Checkpoint Lane 4 for security screening.  Transportation Security Officer (TSO) Mickey Mouse was in the X-Ray position and identified what appeared to be a firearm (Smith and Wesson M and P 9mm Serial # JYC1234) in Stark's bag with ammunition.  Supervisory TSO (STSO) Donald Duck reported to the lane and notified Cincinnati Airport Police (CPD).  CPD Officer Goofy responded and escorted Stark from the sterile area to secure the firearm and ammunition.  Stark was allowed to continue travel; photographs and statements were collected.  He was bound for Philadelphia International Airport.\"\r\n           }\r\n      }\r\n    ]\r\n}";

            OpenAIHelper openAIHelper = new OpenAIHelper();
            var response = openAIHelper.GetFlightData(inputData);

            log.LogInformation(JsonHelper.Serializer<SkillResponse>(response));
        }
    }
}
