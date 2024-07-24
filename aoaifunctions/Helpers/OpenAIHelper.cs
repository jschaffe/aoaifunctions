﻿using aoaifunctions.Entities;
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

        string completion = "";

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
