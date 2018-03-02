#if WINDOWS_UWP
using System;
using RosMessages;
using Windows.Data.Json;


namespace RosUInt16Coder
{
    public static class RosUInt16Coder_
    {
        public static RosMessages.UInt16 deserializeSingleUInt64(JsonObject juint16)
        {
			//evtl ändern in String -> short, da floatingpointfehler
            double doubleushort = juint16["data"].GetNumber();
			ushort uinteger16 = (ushort) doubleushort;

            // Baue den Integer16 zusammen
            RosMessages.UInt16 uint16Object = new RosMessages.UInt16();
            uint16Object.data = uinteger16;
            return uint16Object;
        }

        public static string serializeSingleUInt16(RosMessages.UInt16 uinteger16)
        {
            return "{\"uint16\": {\"data\": " + uinteger16.data + "}";
        } 
    }
}
#endif