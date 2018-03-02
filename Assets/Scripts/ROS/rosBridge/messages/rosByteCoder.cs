#if WINDOWS_UWP
using System;
using RosMessages;
using Windows.Data.Json;


namespace RosByteCoder
{
    public static class RosByteCoder_
    {
        public static RosMessages.Byte deserializeSingleByte(JsonObject jbyte)
        {
            double doubleby = jbyte["data"].GetNumber();
			byte by = (byte) doubleby;
			
            // Baue den Byte zusammen
            RosMessages.Byte byteObject = new RosMessages.Byte();
            byteObject.data = by;
            return byteObject;
        }

        public static string serializeSingleByte(RosMessages.Byte by)
        {
            return "{\"byte\": {\"data\": " + by.data + "}";
        } 
    }
}
#endif