#if WINDOWS_UWP
using System;
using RosMessages;
using Windows.Data.Json;


namespace RosMultiArrayLayoutCoder
{
    public static class RosMultiArrayLayoutCoder_
    {
        public static MultiArrayLayout deserializeSimple(JsonObject jmultiArrayLayout)
        {
 
            return null;
        }

        public static string serializeSimple(MultiArrayLayout mal)
        {
            return null;
         //  return "{\"multiarraydimension\": {\"label\": " + mad.label + ", \"size\": " + mad.size + ", \"stride\": " + mad.stride + "}";
        }
    }
}
#endif