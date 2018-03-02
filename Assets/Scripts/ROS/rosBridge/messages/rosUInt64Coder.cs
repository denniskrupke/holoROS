#if WINDOWS_UWP
using System;
using RosMessages;
using Windows.Data.Json;


namespace RosUInt64Coder
{
    public static class RosUInt64Coder_
    {
        public static RosMessages.UInt64 deserializeSimple(JsonObject juint64)
        {
			//evtl ändern in String -> long, da floatingpointfehler
            double doubleulong = juint64["data"].GetNumber();
			ulong uinteger64 = (ulong) doubleulong;

            // Baue den Integer64 zusammen
            RosMessages.UInt64 uint64Object = new RosMessages.UInt64();
            uint64Object.data = uinteger64;
            return uint64Object;
        }

        public static string serializeSimple(RosMessages.UInt64 uinteger64)
        {
            return "{\"uint64\": {\"data\": " + uinteger64.data + "}";
        } 
    }
}
#endif