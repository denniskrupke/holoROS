
#if WINDOWS_UWP
using System;
using RosMessages;
using Windows.Data.Json;


namespace RosInt32Coder
{
    public static class RosInt32Coder_
    {
        public static RosMessages.Int32 deserializeSimple(JsonObject jint32)
        {
            double doubleint = jint32["data"].GetNumber();
			int integer32 = (int) doubleint;
			
            // Baue den Integer32 zusammen
            RosMessages.Int32 int32Object = new RosMessages.Int32();
            int32Object.data = integer32;
            return int32Object;
        }

        public static string serializeSimple(RosMessages.Int32 integer32)
        {
            return "{\"int32\": {\"data\": " + integer32.data + "}";
        } 
    }
}
#endif