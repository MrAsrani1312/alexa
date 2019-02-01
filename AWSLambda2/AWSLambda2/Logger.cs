using Amazon.Lambda.Core;
using System;
using System.Collections.Generic;
using System.Text;
//using Amazon.Lambda.Core

namespace AWSLambda2
{
    public static class Log
    {
        public static ILambdaLogger logger;

        public static void Output(object Obj)
        {
            logger.LogLine(Obj.ToString());
        }
        // Add STring 

    }
}
