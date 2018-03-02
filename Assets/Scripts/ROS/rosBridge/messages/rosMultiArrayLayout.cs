#if WINDOWS_UWP
using System;
using RosMessages;
using Windows.Data.Json;


namespace RosMultiArrayLayoutCoder
{
    public static class RosMultiArrayLayoutCoder_
    {
        public static MultiArrayLayout deserializeSingleMultiArrayLayout(JsonObject jmultiArrayLayout)
        {
 
            return null;
        }

        public static string serializeSingleMultiArrayLayout(MultiArrayLayout mal)
        {
            return null;
         //  return "{\"multiarraydimension\": {\"label\": " + mad.label + ", \"size\": " + mad.size + ", \"stride\": " + mad.stride + "}";
        }
    }
}
#endif