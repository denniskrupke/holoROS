#if WINDOWS_UWP
using System;
using RosMessages;
using Windows.Data.Json;


namespace RosBoolCoder
{
    public static class RosBoolCoder_
    {
        public static Bool deserializeSingleBool(JsonObject jbool)
        {
            bool boolean = jbool["data"].GetBoolean();

            // Baue den Bool zusammen
            Bool boolObject = new Bool();
            boolObject.data = boolean;
            return boolObject;
        }

        public static string serializeSingleBool(Bool boolean)
        {
            return "{\"bool\": {\"data\": " + boolean.data + "}";
        } 
    }
}
#endif
