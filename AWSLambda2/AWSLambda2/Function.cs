using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Alexa.NET.Request;
using Alexa.NET.Request.Type;
using Alexa.NET.Response;
using Alexa.NET.Response.Directive;
using Alexa.NET;
using Alexa.NET.APL; // APL
// using Alexa.NET.APL.Components;
// using Alexa.NET.APL.Commands;
using Amazon.Lambda.Core;
using Amazon.Lambda.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.Json.JsonSerializer))]

namespace AWSLambda2
{
    public class Function
    { 
        public const string INVOCATION_NAME = "Test Player";


            #region Data
        public string[] audioUrls =
            {
            "https://s3.amazonaws.com/asmr-media/audio/ASMR+10+Triggers+to+Help+You+Sleep+%E2%99%A5.m4a",
            "https://s3.amazonaws.com/asmr-media/audio/ASMR+20+Triggers+To+Help+You+Sleep+%E2%99%A5.m4a",
            "https://s3.amazonaws.com/asmr-media/audio/ASMR+100+Triggers+To+Help+You+Sleep+%E2%99%A5+4+HOURS.m4a",

            };

        public string[] names =
        {
            "10 triggers to help you sleep",
            "20 triggers to help you sleep",
            "100 triggers to help you sleep",
            };

        public string[] videoUrls = {
            "https://s3.amazonaws.com/asmr-media/videos/ASMR+10+Triggers+to+Help+You+Sleep+%E2%99%A5.mp4",
            "https://s3.amazonaws.com/asmr-media/videos/ASMR+20+Triggers+To+Help+You+Sleep+%E2%99%A5.mp4",
            "https://s3.amazonaws.com/asmr-media/videos/ASMR+100+Triggers+To+Help+You+Sleep+%E2%99%A5+4+HOURS.mp4"
        };


        string whatIsASMRvideo = "https://s3.amazonaws.com/asmr-media/videos/What+is+ASMR.mp4";
        string whatIsASMRaudio = "https://s3.amazonaws.com/asmr-media/audio/What+is+ASMR.m4a";

        #endregion

        public SkillResponse FunctionHandler(JObject inputObj, ILambdaContext context)
        {

            Log.logger = context.Logger;
            APLSkillRequest input = new APLSkillRequest();
            SkillResponse respond = ResponseBuilders.BuildResponse(null, false, null, null, null);

            try
            {
                new SystemRequestTypeConverter().AddToRequestConverter();
                new UserEventRequestHandler().AddToRequestConverter();
                //new APLRequestTypeConverter().AddToRequestConverter();

                //Getting input 
                string inputString = JsonConvert.SerializeObject(inputObj);
                input = JsonConvert.DeserializeObject<APLSkillRequest>(inputString);
                //Logging input
                Log.Output("---INPUT---");
                context.Logger.LogLine(JsonConvert.SerializeObject(input));
                
                // Initialise data 
                var requestType = input.GetRequestType();  // Get type of request
                bool VideoSupport = input.Context.System.Device.IsInterfaceSupported("VideoApp");
                bool APLSupport = input.Context.System.Device.IsInterfaceSupported("Alexa.Presentation.APL");
                Log.Output("Video Support - APL - is: " + APLSupport);

                // ***REQUESTS*** //

                if (input.Request is LaunchRequest && APLSupport) // Launch Request for Video
                {
                    // Launch request for echo spot/show -> Return APL + Ask 
                    Log.Output("Video App Launch Request");
                    respond = Dependencies.CreateAPL();
                }
                else if (input.Request is LaunchRequest && !APLSupport) // Launch Request for speakers
                {
                    Log.Output("Launch Request for smart speaker");
                    Reprompt reprompt = new Reprompt("How can I help you today?");
                    respond = ResponseBuilder.Ask("Welcome to ASMR video. Please ask for the list of songs or ask me to play a song", reprompt);
                }
                else if (input.Request is SessionEndedRequest) // SessionEndedRequest
                {
                    // End Session by playing message
                    Log.Output("Session Ended Request Called");
                    respond =  ResponseBuilder.Tell("Thank you for using this skill. Goodbye.");
                    respond.Response.ShouldEndSession = true;
                }
                else if (input.Request is UserEventRequest usrEvent && APLSupport) // User Event Request for TouchWrappers
                {
                    Log.Output("User Event Launch requst");
                    var Id = Convert.ToInt32(usrEvent.Source.ComponentId); // Take ID as integer
                    Id = Id - 1;
                    Log.Output("ID of touchwrapper is : "+ (Id+1)+" , Index of number is: "+ Id);
                    respond = Dependencies.BuildVideoResonse(videoUrls[Id]);
                    context.Logger.LogLine(JsonConvert.SerializeObject(respond));
                } 
                else if (input.Request is PlaybackControllerRequest) // Playback controller request
                {
                    Log.Output("Playback Controller Request Called");
                    var playbackReq = input.Request as PlaybackControllerRequest;
                    switch (playbackReq.PlaybackRequestType)
                    {
                        case PlaybackControllerRequestType.Next:
                            break;
                        case PlaybackControllerRequestType.Pause:
                            break;
                        case PlaybackControllerRequestType.Play:
                            break;
                        case PlaybackControllerRequestType.Previous:
                            break;
                    }
                    respond = ResponseBuilder.AudioPlayerStop();
                }
                // ***INTENTS***
                else if (requestType == typeof(IntentRequest)) // INTENTS
                {
                    var intentRequest = input.Request as IntentRequest; // Get intent request
                    var intentName = intentRequest.Intent.Name;
                    Log.Output("Intent Requests");

                    //Check request 
                    switch (intentName)
                    {
                        // Play Song Intent
                        case "PlaySongIntent":
                            Log.Output("Play a song Intent");
                            var songSlot = intentRequest.Intent.Slots["songName"].Value; // get slot

                            //int songNumIndex= Dependencies.SlotConverter(songSlot);
                            int songNumIndex = Convert.ToInt32(songSlot);
                            songNumIndex -= 1;
                            Log.Output("Song Slot is: " + songSlot+ " , song Number index is : "+ songNumIndex);

                            if (songNumIndex != -1)  // -1 = NOT FOUND
                            {
                                var audioRes = ResponseBuilders.AudioPlayerPlay(Alexa.NET.Response.Directive.PlayBehavior.ReplaceAll, audioUrls[songNumIndex], names[songNumIndex], null, 0);
                                respond = audioRes;
                                respond.Response.OutputSpeech = new PlainTextOutputSpeech { Text = "Playing the song." };
                            }
                            else //Found
                            {
                                
                                respond.Response.OutputSpeech = new PlainTextOutputSpeech { Text = "I did not understand which song you asked me to pplay. Could you please repeat?" };
                            }
                            break;

                        // ListSongsIntent
                        case "ListSongsIntent":
                            Log.Output("List Song Intent Request Called");
                            string text = "The ASMR songs are: ";
                            for (int i = 0; i < names.Length; i++)
                            {
                                string ch = " , ";
                                if (i == (names.Length - 1))
                                {
                                    ch = ".";
                                }
                                text += ((i + 1) + ". " + names[i] + ch);
                            }
                            text += " Which song do you want me to play? Say \"Alexa, play song 1 \".";
                            Reprompt reprompt = new Reprompt("Which song should I play?");
                            respond = ResponseBuilder.Ask(text, reprompt);
                            break;

                        // Help Intent
                        case "AMAZON.HelpIntent":
                            Log.Output("Help Intent Request Called");
                            respond = ResponseBuilder.Tell("You can ask me 'What is ASMR' or ask me to play one of ASMR Darling's top ten videos or ask for a list of ASMR's top ten videos");
                            break;

                        //AMAZON StopIntent
                        case "AMAZON.StopIntent":
                            Log.Output("Stop Intent Request Called");
                            if (APLSupport)
                            {
                                // Stop when Video Present
                                respond = Dependencies.CreateAPL();
                            }
                            else
                            {
                                // Stop when Audio Present
                                Reprompt re = new Reprompt("How can I help you today?");
                                respond = ResponseBuilder.Ask("Welcome to ASMR video. Please ask for the list of songs or ask me to play a song", re);
                                respond.Response.Directives.Add(new StopDirective());

                            }
                            break;

                        case "AMAZON.CancelIntent":
                            if (APLSupport)
                            {
                                Log.Output("---CancelIntent with video Support---");
                                respond = Dependencies.CreateAPL(); ;
                            }
                            else
                            {
                                Log.Output("Cancel Intent Request(Audio Player) Called");
                                Reprompt re = new Reprompt("How can I help you today?");
                                respond = ResponseBuilder.Ask("Welcome to ASMR video. Please ask for the list of songs or ask me to play a song", re);
                                respond.Response.Directives.Add(new StopDirective());
                            }
                            break;

                        case "AMAZON.PauseIntent":
                            Log.Output("Pause Intent Request Called");
                            respond =  ResponseBuilder.AudioPlayerStop();
                            break;

                        case "WhatIsASMRIntent":
                            // What is ASMR?
                            if (APLSupport)
                            {
                                Log.Output("What is ASMR Intent - VideoApp played");
                                respond = Dependencies.BuildVideoResonse(whatIsASMRvideo); // Return response to play Video
                            }
                            else
                            {
                                Log.Output("What is ASMR- Audio played");
                                respond = ResponseBuilders.AudioPlayerPlay(Alexa.NET.Response.Directive.PlayBehavior.ReplaceAll, whatIsASMRaudio, "What is ASMR?", null, 0); ;
                            }
                            break;

                        case "PlayVideoIntent":
                            Log.Output("Play a Video Intent - \"Alexa play video\"");
                            var videoSlot = intentRequest.Intent.Slots["songName"].Value; // get slot
                            int videoNumIndex = Dependencies.SlotConverter(videoSlot);

                            respond = Dependencies.BuildVideoResonse(videoUrls[videoNumIndex]);
                            context.Logger.LogLine(JsonConvert.SerializeObject(respond));

                            break;
                        default:
                            Log.Output("Did not understand the intent request / Unexpected intent request");
                            respond = ResponseBuilder.Tell("I dont understand. Please ask me to list all songs or you can ask for help");
                            break;
                    }
                }
                else
                {
                    Log.Output("Unknown Request or Intent.");
                    Log.Output(JsonConvert.SerializeObject(input));
                    respond = ResponseBuilder.Tell("I dont understand. Please ask me to list all songs or ask for help");
                }

                return respond;
            }
            catch (Exception e)
            {
                Log.Output("Error while serializing input.. ");
                Log.Output(e.StackTrace);

                return ResponseBuilder.Tell("I did not understand that. Unknown request. ");
            }

        }

        // Main Response Builder
        private static SkillResponse BuildResponses(IOutputSpeech outputSpeech, bool shouldEndSession, Session sessionAttributes, Reprompt reprompt, ICard card)
        {
            SkillResponse response = new SkillResponse { Version = "1.0" };
            if (sessionAttributes != null) response.SessionAttributes = sessionAttributes.Attributes;

            ResponseBody body = new ResponseBody
            {
                ShouldEndSession = shouldEndSession,
                OutputSpeech = outputSpeech
            };

            if (reprompt != null) body.Reprompt = reprompt;
            if (card != null) body.Card = card;

            response.Response = body;

            return response;
        }

    }
}