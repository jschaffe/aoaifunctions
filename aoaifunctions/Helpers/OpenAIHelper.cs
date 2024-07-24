using aoaifunctions.Entities;
using aoaifunctions.ResponseEntities;
using Azure;
using Azure.AI.OpenAI;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using OpenAI;
using OpenAI.Chat;
using System;
using System.ClientModel;
using System.Collections.Generic;
using System.Text;

namespace aoaifunctions.Helpers
{
    public class OpenAIHelper
    {
        private const string prompt = "Extract flight information from the Summary below as JSON: Include departure airport code as DepartureAirportCode, arrival airport code as ArrivalAirportCode, flight path as Path, Airline as Carrier, flight number as Number, departure airport as DepartureAirport, arrival airport as ArrivalAirport, seat number as Seat, concatenate DepartureAirportCode and DepartureAirport with \"::\" as a separator as DepartureCodeName, concatenate ArrivalAirportCode and ArrivalAirport with \"::\" as a separator as ArrivalCodeName   Format flight path as XXX-XXX; Provide full departure airport name; Provide full arrival airport name; If seat number is not found return null";
        private const string contentDelimiter = "###";

        public OpenAIHelper()
        {

        }
        //static string key = "9f4d0240980a416c9ac4996342e7bc43";
        //static string endpoint = "https://jschaffeopenai.openai.azure.com/";
        //static string key = "bced092b215a4768bcabe21028e00719";
        //static string endpoint = "https://openaikesteph.openai.azure.com/";
        static string key = "8917f9dbe60b4d709ba1ff12aad7b6ad";
        static string endpoint = "https://rapidopenai.openai.azure.us/";
        string completion = "";

        //public string GetPromptResponse(string prompt)
        //{
        //    OpenAIClient client = new OpenAIClient(new Uri(endpoint), new AzureKeyCredential(key));
        //    if (prompt != "")
        //    {
        //        try
        //        {
        //            ChatCompletionOptions completionsOptions = new ChatCompletionOptions
        //            {

        //                //DeploymentName = "gpt-4-1106preview",
        //                DeploymentName = "davinci-summary",
        //                //DeploymentName = "gpt35turbo",
        //                Prompts = { prompt },
        //                MaxTokens = 1000
        //            };
        //            Response<Completions> completionsResponse = client.GetCompletions(completionsOptions);
        //            completion = completionsResponse.Value.Choices[0].Text;
        //        }
        //        catch (Exception ex)
        //        {
        //            completion = ex.Message;
        //        }
        //    }
        //    return completion;
        //}

        public SkillResponse GetFlightData(string data)
        {
            var response = new SkillResponse();
            var returnValue = String.Empty;

            var skillData = JsonHelper.Deserialize<SkillData>(data);
            foreach (var val in skillData.values)
            {
                try
                {
                    if (val.data.SourceName.Equals("PARIS", StringComparison.InvariantCultureIgnoreCase))
                    {
                        OpenAIHelper openAI = new OpenAIHelper();
                        //returnValue = openAI.GetPromptResponse($"{prompt} {contentDelimiter} {val.data.DisplaySummary} {contentDelimiter}");
                        returnValue = openAI.GetChatPromptResponse($"{prompt} {contentDelimiter} {val.data.DisplaySummary} {contentDelimiter}");
                        if (IsJson(returnValue))
                        {
                            var aoaiResponse = JsonHelper.Deserialize<AOAIResponse>(returnValue);
                            response.values.Add(new ResponseEntities.Values
                            {
                                recordId = val.recordId,
                                data = new ResponseEntities.Data
                                {
                                    FlightData = "[" + returnValue + "]"
                                }
                            });
                        }
                        else
                        {
                            var temp = new ResponseEntities.Values
                            {
                                recordId = val.recordId,
                                data = new ResponseEntities.Data
                                {
                                    FlightData = null
                                }
                            };
                            temp.warnings.Add(returnValue);
                            response.values.Add(temp);
                        }
                    }
                    else
                    {
                        response.values.Add(new ResponseEntities.Values
                        {
                            recordId = val.recordId,
                            data = new ResponseEntities.Data
                            {
                                FlightData = val.data.Flight
                            }
                        });
                    }
                }
                catch (Exception ex)
                {
                    var temp = new ResponseEntities.Values
                    {
                        recordId = val.recordId,
                        data = new ResponseEntities.Data
                        {
                            FlightData = "[" + returnValue + "]"
                        }
                    };
                    temp.errors.Add(ex.Message);
                    response.values.Add(temp);
                }
            }

            return response;
        }

        public string GetChatPromptResponse(string prompt)
        {
            AzureOpenAIClient aiClient = new AzureOpenAIClient(new Uri(endpoint), new AzureKeyCredential(key));
            ChatClient client = aiClient.GetChatClient("gpt-4");
            if (prompt != "")
            {
                try
                {
                    ChatCompletion response = client.CompleteChat(prompt);

                    completion = response.Content[0].Text.Replace("```json", "").Replace("```", "");
                }
                catch (Exception ex)
                {
                    completion = ex.Message;
                }
            }
            return completion;
        }

        private static bool IsJson(string jsonValue)
        {
            try
            {
                var obj = JToken.Parse(jsonValue);
                return true;
            }
            catch (JsonReaderException jex)
            {
                // Exception in parsing JSON
                return false;
            }
        }
    }
}
