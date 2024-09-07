using aoaifunctions.Entities;
using aoaifunctions.ResponseEntities;
using Azure;
using Azure.AI.OpenAI;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.Text.Json;
using System.Text.Json.Nodes;
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
        private const string consolidatedPrompt = "Extract flight information from the Summary below as JSON, returned as a node name FlightInfo: Include departure airport code as DepartureAirportCode, arrival airport code as ArrivalAirportCode, flight path as Path, Airline as Carrier, flight number as Number, departure airport as DepartureAirport, arrival airport as ArrivalAirport, seat number as Seat, concatenate DepartureAirportCode and DepartureAirport with \\\"::\\\" as a separator as DepartureCodeName, concatenate ArrivalAirportCode and ArrivalAirport with \\\"::\\\" as a separator as ArrivalCodeName   Format flight path as XXX-XXX; Provide full departure airport name; Provide full arrival airport name; If seat number is not found return null.  If no flight information is found, return null.  In addition, extract the threat and threat level (none, low, medium, high) as a separate JSON node named ThreatInfo: Include threat as ThreatType and threat level as ThreatLevel. If threat level is not explicit, infer as best as possible.  Do no include an explanation of your findings.";
        private const string contentDelimiter = "###";

        public OpenAIHelper()
        {

        }

        string completion = "";

        public SkillResponse GetAllData(string data)
        {
            var response = new SkillResponse();
            var returnValue = String.Empty;

            var skillData = JsonHelper.Deserialize<SkillData>(data);
            foreach (var val in skillData.values)
            {
                try
                {
                    OpenAIHelper openAI = new OpenAIHelper();
                    returnValue = openAI.GetChatPromptResponse($"{consolidatedPrompt} {contentDelimiter} {val.data.DisplaySummary} {contentDelimiter}");
                    if (IsJson(returnValue))
                    {
                        if (val.data.SourceName.Equals("PARIS", StringComparison.InvariantCultureIgnoreCase))
                        {
                            var jsonNode = JsonNode.Parse(returnValue);
                            var flightInfo = jsonNode["FlightInfo"];
                            var threatInfo = jsonNode["ThreatInfo"];
                            response.values.Add(new ResponseEntities.Values
                            {
                                recordId = val.recordId,
                                data = new ResponseEntities.Data
                                {
                                    FlightData = "[" + flightInfo.ToJsonString() + "]",
                                    ThreatData = threatInfo == null ? null : threatInfo.ToJsonString()
                                }
                            });
                        }
                        else
                        {
                            var jsonNode = JsonNode.Parse(returnValue);
                            var threatInfo = jsonNode["ThreatInfo"];
                            response.values.Add(new ResponseEntities.Values
                            {
                                recordId = val.recordId,
                                data = new ResponseEntities.Data
                                {
                                    FlightData = val.data.Flight,
                                    ThreatData = threatInfo == null ? null : threatInfo.ToJsonString()
                                }
                            });
                        }
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
            var openAiUrl = ConfigHelper.GetConfigSetting("OpenAiUrl");
            var openAiKey = ConfigHelper.GetConfigSetting("OpenAiKey");
            var openAiModelName = ConfigHelper.GetConfigSetting("OpenAiModelName");

            AzureOpenAIClient aiClient = new AzureOpenAIClient(new Uri(openAiUrl), new AzureKeyCredential(openAiKey));
            ChatClient client = aiClient.GetChatClient(openAiModelName);
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
