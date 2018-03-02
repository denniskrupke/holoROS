#if WINDOWS_UWP
using RosArrayService;
using RosHeaderCoder;
using RosMessages;
using System.Diagnostics;
using System.Threading.Tasks;
using Windows.Data.Json;

namespace RosJointStateCoder
{

    class RosJointStateCoderSeriell : rosComplexCoder
    {

        public RosJointStateCoderSeriell()
        {
        }

        // Startet das Deserialisieren eines JointStates
        public RosPublish startDeserializing(JsonObject jsonObject)
        {
            return deserializeJointState(jsonObject);
        }

        // Startet das Serialisieren eines JointStates
        public string startSerializing(RosMessage message)
        {
            JointState js = new JointState();

            string topic = message.topic;

            RosPublish publishMessage = (RosPublish)message;

            MessageData msg = publishMessage.msg;
            if(msg.GetType() == typeof(JointState))
            {
                js = (JointState)msg;
            }

            return serializeJointState(js,topic);      
        }

        private RosPublish deserializeJointState(JsonObject jsonObject)
        {
            JointState messageData = new JointState();
     
            // baue den Header
            Header header = RosHeaderCoder_.deserializeSingleHeader((JsonValue)jsonObject["msg"].GetObject()["header"]);
            messageData.header = header;

            // Nutze den ArrayService, um die JSONArrays in "normale" Arrays umzuwandeln
            messageData.name = RosArrayService_.stringArrayAusJsonArray(jsonObject["msg"].GetObject()["name"].GetArray());
            messageData.position = RosArrayService_.doubleArrayAusJsonArray(jsonObject["msg"].GetObject()["position"].GetArray());
            messageData.velocity = RosArrayService_.doubleArrayAusJsonArray(jsonObject["msg"].GetObject()["velocity"].GetArray());
            messageData.effort = RosArrayService_.doubleArrayAusJsonArray(jsonObject["msg"].GetObject()["effort"].GetArray());
           
            // Fertiges Objekt zurückgeben
            return new RosPublish("\"/joint_states\"", messageData);
        }

        private string serializeJointState(JointState js, string topic)
        {
            //string defaultttopic = "\"topic\": \"/joint_states\", "; // platzhalter, topic vom RosMessage- objekt später holen
            string ausgabeTopic = "\"topic\": " + topic + ", ";
            // Baue Header
 
            string headerString = RosHeaderCoder_.serializeSingleHeader(js.header);

            // Baue Velocityarray
            string velocityString = RosArrayService_.baueDoubleWertArrayUm(js.velocity, "\"velocity\": ", false);

            // Baue Effortarray
            string effortString = RosArrayService_.baueDoubleWertArrayUm(js.effort, "\"effort\": ", false);

            // Baue Namearray
            string nameString = RosArrayService_.baueStringWertArrayUm(js.name, "\"name\": ", false);

            // Baue Positionarray
            string positionString = RosArrayService_.baueDoubleWertArrayUm(js.position, "\"position\": ", true);

            // Einzelteile zusammenkleben und formatiert zurückgeben
            return "\"{" + ausgabeTopic + "\"msg\": {" + headerString + velocityString + effortString + nameString + positionString + "}, \"op\": \"publish\"}\"";
        }
    }
}
#endif
