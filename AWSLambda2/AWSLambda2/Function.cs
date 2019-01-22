using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Alexa.NET.Request;
using Alexa.NET.Request.Type;
using Alexa.NET.Response;
using Alexa.NET;
using Amazon.Lambda.Core;
using Amazon.Lambda.Serialization;
using Newtonsoft.Json;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.Json.JsonSerializer))]

namespace LambdaAlexa
{
    public class Function
    {

        /// <summary>
        /// A simple function that takes a string and does a ToUpper
        /// </summary>
        /// <param name="input"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        private static HttpClient _httpClient;
        public const string INVOCATION_NAME = "Test Player";

        // Array for song data 
        string[] audioUrls =
        {
            "https://s3.amazonaws.com/asmr-media/audio/ASMR+10+Triggers+to+Help+You+Sleep+%E2%99%A5.m4a",
            "https://s3.amazonaws.com/asmr-media/audio/ASMR+20+Triggers+To+Help+You+Sleep+%E2%99%A5.m4a",
            "https://s3.amazonaws.com/asmr-media/audio/ASMR+100+Triggers+To+Help+You+Sleep+%E2%99%A5+4+HOURS.m4a"
        };
        string[] audioNames =
        {
            "10 triggers to help you sleep",
            "20 triggers to help you sleep",
            "100 triggers to help you sleep"
        };

        public Function()
        {
            _httpClient = new HttpClient();
        }

        public SkillResponse FunctionHandler(SkillRequest input, ILambdaContext context)
        {
            // Initialise data 
            var requestType = input.GetRequestType();  // Get type of request
                                                       //var 

            // ***REQUESTS***

            if (input.Request is LaunchRequest) // Launch Request 
            {
                Reprompt reprompt = new Reprompt("How can I help you today?");
                return ResponseBuilder.Ask("Welcome to ASMR video. Please ask for the list of songs or ask me to play a song", reprompt);
            }
            else if (input.Request is SessionEndedRequest)
            {
                // End Session by playing message
                return ResponseBuilder.Tell("Thank you for using this skill. Goodbye.");
            }
            // ***INTENTS***
            if (requestType == typeof(IntentRequest)) // INTENTS
            {
                var intentRequest = input.Request as IntentRequest; // Get intent request
                var intentName = intentRequest.Intent.Name;

                //var countryRequested = intentRequest.Intent.Slots["Country"].Value; // Get slots

                //   return MakeSkillResponse(
                //           $"You'd like more information about {countryRequested}",
                //           true); //Response

                //Check request 
                switch (intentName)
                {
                    // Play Song Intent
                    case "PlaySongIntent":
                        var songSlot = intentRequest.Intent.Slots["songName"].Value;
                        int index = Convert.ToInt32(songSlot);
                        var audioResponse = ResponseBuilder.AudioPlayerPlay(Alexa.NET.Response.Directive.PlayBehavior.Enqueue, audioUrls[index], audioNames[index]);

                        return audioResponse;

                    // ListSongsIntent
                    case "ListSongsIntent":
                        string text = "The songs are: ";
                        for (int i = 0; i < audioNames.Length; i++)
                        {
                            string ch = " , ";
                            if (i == (audioNames.Length - 1))
                            {
                                ch = ".";
                            }
                            text += (i + " " + audioNames[i] + ch);
                        }

                        return ResponseBuilder.Tell(text);

                    // Help Intent
                    case "AMAZON.HelpIntent":
                        return ResponseBuilder.Tell("You can ask me 'What is ASMR' or ask me too play one of ASMR Darling's top ten videos or ask for a list of ASMR's top ten videos");

                    //AMAZON StpIntent
                    case "AMAZON.StopIntent":
                        return ResponseBuilder.AudioPlayerStop();

                    default:
                        return ResponseBuilder.Tell("I dont understand. Please ask me to list all songs or ask for help");
                }
            }
            else
            {
                //Reprompt reprompt = new Reprompt("Please say something like play a song or list all songs.");
                return ResponseBuilder.Tell("I dont understand. Please ask me to list all songs or ask for help");
            }
        }

       /* private SkillResponse MakeSongResponse(int index, string url, int offset)
        {
            var response = new ResponseBody
            {
                type = "",
                ShouldEndSession = true,
                OutputSpeech = new PlainTextOutputSpeech { Text = ("Playing song " + audioNames[index]) }
            };
        }
        */
        private SkillResponse MakeSkillResponse(string outputSpeech,
            bool shouldEndSession,
            string repromptText = "Just say, play a song. To exit, say, exit.")
        {
            var response = new ResponseBody
            {
                ShouldEndSession = shouldEndSession,
                OutputSpeech = new PlainTextOutputSpeech { Text = outputSpeech }
            };

            if (repromptText != null)
            {
                response.Reprompt = new Reprompt() { OutputSpeech = new PlainTextOutputSpeech() { Text = repromptText } };
            }

            var skillResponse = new SkillResponse
            {
                Response = response,
                Version = "1.0"
            };
            return skillResponse;
        }

    }
}