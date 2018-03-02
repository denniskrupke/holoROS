#if WINDOWS_UWP
using System;
using RosMessages;
using Windows.Data.Json;


namespace RosStampCoder
{
    public static class RosStampCoder_
    {
        public static Stamp deserializeSimple(JsonObject jstamp)
        {
            double stampSecs = jstamp["secs"].GetNumber();
            double stampNsecs = jstamp["nsecs"].GetNumber();

            // Baue den Stamp zusammen
            Stamp stamp = new Stamp();
            stamp.secs = (int)stampSecs;
            stamp.nsecs = (int)stampNsecs;
            return stamp;
        }

        public static string serializeSimple(Stamp stamp)
        {
            return "\"stamp\": {\"secs\": " + stamp.secs + ", \"nsecs\": " + stamp.nsecs + "}";
        } 
    }
}
#endif