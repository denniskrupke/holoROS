#if WINDOWS_UWP
using RosMessages;
using System;
using Windows.Data.Json;
using RosStampCoder;


namespace RosHeaderCoder
{
    public static class RosHeaderCoder_
    {
        public static Header deserializeSingleHeader(JsonValue jheader)
        {
            // baue den Header
            Header header = new Header();

            //Mache den Header wieder zu einem JsonObject, damit Key Value wieder funktioniert
            string geschachteltesHeaderJson = jheader.ToString();
            JsonObject jsonHeader = JsonObject.Parse(geschachteltesHeaderJson);

            // Hole den seq-Wert
            double d = jsonHeader["seq"].GetNumber();

            // Hole den frame_id-Wert
            string jframe_id = jsonHeader["frame_id"].GetString();

            // Hole den Stamp-Wert (wieder geschachtelt, wieder über JsonObject, aber nicht über den Stringumweg, den braucht man hier komischerweise nicht)
            JsonObject jstamp = jsonHeader["stamp"].GetObject();

            // baue den Header zusammen
            header.stamp = RosStampCoder_.deserializeSimple(jstamp);
            header.frame_id = jframe_id;
            header.seq = (int)d;
            return header;
        }

        public static string serializeSingleHeader(Header header)
        {
            string stampString = RosStampCoder_.serializeSimple(header.stamp);
            string frame_id = header.frame_id;
            int seq = header.seq;
            string headerString = baueHeaderString(stampString, frame_id, seq);
            return headerString;
        }

        private static string baueHeaderString(string stampString, string frame_id, int seq)
        {
            return "\"header\": {"+ stampString + ", \"frame_id\": " + "\"" + frame_id + "\"" + ", \"seq\": " + seq + "}, ";
        }
    }
}

#endif