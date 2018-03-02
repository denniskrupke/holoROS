#if WINDOWS_UWP
using System;
using RosMessages;
using Windows.Data.Json;


namespace RosInt8Coder
{
    public static class RosInt8Coder_
    {
        public static Int8 deserializeSingleInt8(JsonObject jint8)
        {
			//evtl ändern in String -> sbyte, da floatingpointfehler
            double doublesbyte = jint8["data"].GetNumber();
			sbyte integer8 = (sbyte) doublesbyte;
			
            // Baue den Integer8 zusammen
            Int8 int8Object = new Int8();
            int8Object.data = integer8;
            return int8Object;
        }

        public static string serializeSingleInt8(Int8 integer8)
        {
            return "{\"int8\": {\"data\": " + integer8.data + "}";
        } 
    }
}
#endif