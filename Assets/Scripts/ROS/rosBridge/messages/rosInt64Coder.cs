#if WINDOWS_UWP
using System;
using RosMessages;
using Windows.Data.Json;


namespace RosInt64Coder
{
    public static class RosInt64Coder_
    {
        public static RosMessages.Int64 deserializeSimple(JsonObject jint64)
        {
			//evtl ändern in String -> long, da floatingpointfehler
            double doublelong = jint64["data"].GetNumber();
			long integer64 = (long) doublelong;

            // Baue den Integer64 zusammen
            RosMessages.Int64 int64Object = new RosMessages.Int64();
            int64Object.data = integer64;
            return int64Object;
        }

        public static string serializeSimple(RosMessages.Int64 integer64)
        {
            return "{\"int64\": {\"data\": " + integer64.data + "}";
        } 
    }
}
#endif