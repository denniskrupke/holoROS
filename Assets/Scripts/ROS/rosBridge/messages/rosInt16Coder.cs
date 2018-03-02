#if WINDOWS_UWP
using System;
using RosMessages;
using Windows.Data.Json;


namespace RosInt16Coder
{
    public static class RosInt16Coder_
    {
        public static RosMessages.Int16 deserializeSingleInt64(JsonObject jint16)
        {
			//evtl ändern in String -> short, da floatingpointfehler
            double doubleshort = jint16["data"].GetNumber();
			short integer16 = (short) doubleshort;
			
            // Baue den Integer16 zusammen
            RosMessages.Int16 int16Object = new RosMessages.Int16();
            int16Object.data = integer16;
            return int16Object;
        }

        public static string serializeSingleInt16(RosMessages.Int16 integer16)
        {
            return "{\"int16\": {\"data\": " + integer16.data + "}";
        } 
    }
}
#endif