#if WINDOWS_UWP
using System;
using RosMessages;
using Windows.Data.Json;


namespace RosMultiArrayDimensionCoder
{
    public static class RosMultiArrayDimensionCoder_
    {
        public static MultiArrayDimension deserializeSingleMultiArrayDimension(JsonObject jmultiArrayDimension)
        {
            string multiArrayDimensionLabel = jmultiArrayDimension["label"].GetString();
            double multiArrayDimensionSize = jmultiArrayDimension["size"].GetNumber();
			double multiArrayDimensionStride = jmultiArrayDimension["stride"].GetNumber();
			

            // Baue die MultiArrayDimension zusammen
            MultiArrayDimension mad = new MultiArrayDimension();
            mad.label = multiArrayDimensionLabel;
            mad.size = (uint)multiArrayDimensionSize;
			mad.stride = (uint)multiArrayDimensionStride;
            return mad;
        }

        public static string serializeSingleMultiArrayDimension(MultiArrayDimension mad)
        {
            return "{\"multiarraydimension\": {\"label\": " + mad.label + ", \"size\": " + mad.size + ", \"stride\": " + mad.stride + "}";
        } 
    }
}
#endif