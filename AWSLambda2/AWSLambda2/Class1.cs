using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using Newtonsoft.Json;
using Alexa.NET.Request.Type;
using Alexa.NET;
using Alexa.NET.APL;
using Newtonsoft.Json.Linq;

namespace AWSLambda2
{
    public class ExceptionEncountered : Request
    {
        public const string RequestType = "System.ExceptionEncountered";

        [JsonProperty("error", NullValueHandling = NullValueHandling.Ignore)]
        public Error Error { get; set; }

        [JsonProperty("cause", NullValueHandling = NullValueHandling.Ignore)]
        public ErrorCause ErrorCause { get; set; }
    }

    public class SystemRequestTypeConverter : IRequestTypeConverter
    {
        public bool CanConvert(string requestType)
        {
            return requestType == "SystemExceptionEncountered";
        }

        public Request Convert(string requestType)
        {
            return new ExceptionEncountered();
        }

        public void AddToRequestConverter()
        {
            if(RequestConverter.RequestConverters.Where(rc => rc != null).All(rc => rc.GetType() != typeof(SystemRequestTypeConverter)))
            {
                RequestConverter.RequestConverters.Add(this); //Add Request to list
            }
        }
    }

    //APL Requests
    public class APLEncountered : Request
    {
        public const string RequestType = "Alexa.Presentation.APL.UserEvent";

        // ADD Json Property - Token
        [JsonProperty("token", NullValueHandling = NullValueHandling.Ignore)]
        public string Token { get; set; }

        // Add Json Property - Args
        [JsonProperty("arguments", NullValueHandling = NullValueHandling.Ignore)]
        public string[] Arguments { get; set; }

        //Add Json Property - Source
        [JsonProperty("source", NullValueHandling = NullValueHandling.Ignore)]
        public APLCommandSource Source { get; set; }

        //Add Json Property - Component
        [JsonProperty("component", NullValueHandling = NullValueHandling.Ignore)]
        public JObject Component{ get; set;}
    }

    public class APLRequestTypeConverter : IRequestTypeConverter
    {
        public bool CanConvert(string requestType)
        {
            return requestType == "Alexa.Presentation.APL.UserEvent";
        }

        public Request Convert(string requestType)
        {
            return new APLEncountered();
        }

        public void AddToRequestConverter()
        {
            if (RequestConverter.RequestConverters.Where(rc => rc != null).All(rc => rc.GetType() != typeof(APLRequestTypeConverter)))
            {
                RequestConverter.RequestConverters.Add(this); //Add Request to list
            }
        }
    }

}
