#if WINDOWS_UWP
using System;
using RosMessages;
using Windows.Data.Json;


namespace RosUInt8Coder
{
    public static class RosInt8Coder_
    {
        public static UInt8 deserializeSimple(JsonObject juint8)
        {
			//evtl ändern in String -> sbyte, da floatingpointfehler
            double doublebyte = juint8["data"].GetNumber();
			byte uinteger8 = (byte) doublebyte;
			
            // Baue den Integer8 zusammen
            UInt8 uint8Object = new UInt8();
            uint8Object.data = uinteger8;
            return uint8Object;
        }

        public static string serializeSimple(UInt8 uinteger8)
        {
            return "{\"uint8\": {\"data\": " + uinteger8.data + "}";
        } 
    }
}
#endif