#if WINDOWS_UWP
using RosMessages;
using Windows.Data.Json;

namespace RosDurationCoder
{
    public static class RosDurationCoder_
    {
        public static Duration deserializeSingleDuration(JsonObject jduration)
        {
            double durationSecs = jduration["secs"].GetNumber();
            double durationNsecs = jduration["nsecs"].GetNumber();

            // Baue den Stamp zusammen
            Duration duration = new Duration();
            duration.secs = (int)durationSecs;
            duration.nsecs = (int)durationNsecs;
            return duration;
        }

        public static string serializeSingleDuration(Duration duration)
        {
            return "{\"secs\": " + duration.secs + ", \"nsecs\": " + duration.nsecs + "}";
        }
    }
}
#endif