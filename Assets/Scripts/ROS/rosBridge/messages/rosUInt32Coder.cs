#if WINDOWS_UWP
using System;
using RosMessages;
using Windows.Data.Json;


namespace RosUInt32Coder
{
    public static class RosUInt32Coder_
    {
        public static RosMessages.UInt32 deserializeSingleUInt32(JsonObject juint32)
        {
            double doubleuint = juint32["data"].GetNumber();
			uint uinteger32 = (uint) doubleuint;

            // Baue den Integer32 zusammen
            RosMessages.UInt32 uint32Object = new RosMessages.UInt32();
            uint32Object.data = uinteger32;
            return uint32Object;
        }

        public static string serializeSingleUInt32(RosMessages.UInt32 uinteger32)
        {
            return "{\"uint32\": {\"data\": " + uinteger32.data + "}";
        } 
    }
}
#endif