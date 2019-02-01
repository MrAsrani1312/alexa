using System;
using System.Collections.Generic;
using System.Text;
using Alexa.NET.Request;
using Alexa.NET.Response;
using Alexa.NET.Response.APL;
using Alexa.NET.Response.Directive;
using Alexa.NET.APL;
using Alexa.NET.APL.Commands;
using Alexa.NET.APL.Components;
using System.Linq;
using Newtonsoft.Json;
using AWSLambda2;

namespace Alexa.NET
{
    public class Dependencies
    {
        public static SkillResponse response = ResponseBuilders.BuildResponse(null, false, null, null, null);

        public Container Container { get; private set; }
        public Image Image { get; private set; }
        public Text Text { get; private set; }


        #region Data

            public static string[] imgUrls = {
            "https://s3.amazonaws.com/asmr-media/images/10triggers.PNG",
            "https://s3.amazonaws.com/asmr-media/images/20triggers.PNG",
            "https://s3.amazonaws.com/asmr-media/images/100triggers.PNG"
            };

            public static string[] names =
            {
            "10 triggers to help you sleep",
            "20 triggers to help you sleep",
            "100 triggers to help you sleep",
            };

            public static string[] ID = {
            "1",
            "2",
            "3"
            };
        #endregion


        #region Helper functions
        public Image createImage(string url, string height, string width)
        {
            return new Image(url) {
                Height = height,
                Width = width
            };
        }
        #endregion


        #region Video Response
        public static SkillResponse BuildVideoResonse(string url)  //Video Response
        {
            VideoSource videoSrc = new VideoSource(url);
            List<VideoSource> vid = new List<VideoSource>();
            vid.Add(videoSrc);

            // Using APL Video
            Video videoPlay = new Video() {
                Source = new APLValue<List<VideoSource>>(vid),
                Autoplay = new APLValue<bool>(true),
                //Scale = new APLValue<VideoScale>(VideoScale.BestFill)
                // TRY: Added Height and width in video for making it Fullscreen/Almost fullscreen
                Height = "100vh",
                Width = "100vw"
            };

            Container mainContainer = new Container(videoPlay);
            mainContainer.Height = "100%";
            mainContainer.Width = "100%";

            Container outerContainer = new Container(mainContainer);
            outerContainer.Direction = "row";

            response.Response.Directives.Add(new RenderDocumentDirective
            {
                Token = "Video Player",
                Document = new Response.APL.APLDocument
                {
                    MainTemplate = new Response.APL.Layout(mainContainer)
                }
            });
            Log.Output("APL Response Started Building");
            // Reprompt reprompt = new Reprompt("");

            response.Response.OutputSpeech = new PlainTextOutputSpeech { Text = "Playing Video." };
            response.Response.ShouldEndSession = false;
            Log.Output("----APL RESPONSE---");
            Log.Output(JsonConvert.SerializeObject(response));

            return response;
        }
        #endregion

        #region APL Builder
        public static SkillResponse CreateAPL()
        {
            
            Container upperTextContainer = new Container(new Text("Welcome to ASMR Video"));
            upperTextContainer.Height = "10vh";
            upperTextContainer.Width = "10vw";

            Container footerContainer = new Container(new Text("Try: \"Alexa, play Video 1.\""));
            footerContainer.Height = "10vh";
            footerContainer.Width = "10vw";

            TouchWrapper[] tr = new TouchWrapper[names.Length];

                for (int i = 0; i < names.Length; i++)
                {
                    tr[i] = new TouchWrapper(  //Touchwrapper for each list item
                                     new Container(  // Containner for each list item with Video Thumbnail and Video name
                                     new APLComponent[]{
                                         new Image(imgUrls[i]){ Height = "50vh", Width = "50vh" },
                                         new Text(names[i]){ MaxLines = 2, Spacing = 12 }
                                      })
                                     {
                                         MaxWidth = 528,
                                         MinWidth = 312,
                                         PaddingLeft = 16,
                                         PaddingRight = 16,
                                         Height = "100%"
                                     })
                                    {
                                        Id= ID[i],
                                        OnPress = new SendEvent()
                                    };               
                     
                }

            Sequence seq = new Sequence(tr);
            seq.ScrollDirection = "horizontal";
            seq.Height = "70vh";
            seq.Width = "100%";
            seq.Numbered = true;

            Container mainContainer = new Container(seq);
            mainContainer.Height = "80vh";
            mainContainer.Width = "80vw";

            Container outerContainer = new Container(upperTextContainer, mainContainer, footerContainer);
            outerContainer.Direction = "row";

            response.Response.Directives.Add(new RenderDocumentDirective {
                Token = "MainDocument",
                Document = new Response.APL.APLDocument
                {
                    MainTemplate = new Response.APL.Layout(outerContainer)
                }
            });
            Log.Output("APL Response Started Building");
            Reprompt reprompt = new Reprompt("Which video do you want to play? ");

            response.Response.OutputSpeech = new PlainTextOutputSpeech { Text = "Welcome to ASMR Video. Select a video to start playing or ask for help." };

            Log.Output(JsonConvert.SerializeObject(response));

            return response;

        }
        #endregion

        #region Slot Converter

        public static int SlotConverter(string slot)
        {
            int slotValueInteger = -1;


            switch(slot)
            {
                case "one":
                case "One":
                    slotValueInteger = 0;
                    break;

                case "two":
                case "Two":
                    slotValueInteger = 1;
                    break;

                case "three":
                case "Three":
                    slotValueInteger = 2;
                    break;
            }

            return slotValueInteger;
        }

        #endregion

    }
}
