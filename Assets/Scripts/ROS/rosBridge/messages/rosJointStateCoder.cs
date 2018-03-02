#if WINDOWS_UWP
using Windows.Data.Json;
using RosMessages;
using RosArrayService;
using RosHeaderCoder;
using System;

namespace RosJointStateCoder
{

    class RosJointStateCoder_ : rosComplexCoder
    {
        private JointState latestJointState;

        public RosJointStateCoder_()
        {
            latestJointState = new JointState();
        }

        // hier wird einfach hart-gecoded eine JointState-Nachricht deserialisiert
        public RosPublish startDeserializing(JsonObject jsonObject)
        {
            return deserializeJointState(jsonObject);
        }

        // die andere Richtung wäre auch schön, damit man auch Nachrichten an ROS senden kann
        public string startSerializing(RosMessage message)
        {
            //todo
            JointState js = new JointState();

            RosPublish publishMessage = (RosPublish)message;

            MessageData msg = publishMessage.msg;
            if(msg.GetType() == typeof(JointState))
            {
                js = (JointState)msg;
            }

            return serializeJointState(js);
            
        }

        public JointState getJointState()
        {
            return latestJointState;
        }

        private bool isJointState(string message)
        {
            return message.Equals("/joint_states");
        }

        private RosPublish deserializeJointState(JsonObject jsonObject)
        {

            JsonArray jnarray = jsonObject["msg"].GetObject()["name"].GetArray();
            JsonArray jparray = jsonObject["msg"].GetObject()["position"].GetArray();
            JsonArray jvarray = jsonObject["msg"].GetObject()["velocity"].GetArray();
            JsonArray jearray = jsonObject["msg"].GetObject()["effort"].GetArray();

            // baue den Header
            Header header = RosHeaderCoder_.deserializeSingleHeader((JsonValue)jsonObject["msg"].GetObject()["header"]);

            //Restliche Werte des JointState rausfinden

            JointState messageData = new JointState();
            messageData.header = header;
            // Nutze den ArrayService, um die JSONArrays in "normale" Arrays umzuwandeln
            messageData.name = RosArrayService_.stringArrayAusJSonArray(jnarray);
            messageData.position = RosArrayService_.doubleArrayAusJSonArray(jparray);
            messageData.velocity = RosArrayService_.doubleArrayAusJSonArray(jvarray);
            messageData.effort = RosArrayService_.doubleArrayAusJSonArray(jearray);

            return new RosPublish("\"/joint_states\"",messageData);

        }

        private string serializeJointState(JointState js)
        {
            string topic = "\"topic\": \"/joint_states\", "; // platzhalter, topic vom RosMessage- objekt später holen
            // Baue Header
 
            string headerString = RosHeaderCoder_.serializeSingleHeader(js.header);

            // Baue Velocityarray
            double[] velocity = js.velocity;
            string velocityString = RosArrayService_.baueDoubleWertArrayUm(velocity, "\"velocity\": ", false);

            // Baue Effortarray
            double[] effort = js.effort;
            string effortString = RosArrayService_.baueDoubleWertArrayUm(effort, "\"effort\": ", false);

            // Baue Namearray
            string[] name = js.name;
            string nameString = RosArrayService_.baueStringWertArrayUm(name, "\"name\": ", false);

            // Baue Positionarray
            double[] position = js.position;
            string positionString = RosArrayService_.baueDoubleWertArrayUm(position, "\"position\": ", true);

            // Einzelteile zusammenkleben und formatiert zurückgeben
            return "\"{" + topic + "\"msg\": {" + headerString + velocityString + effortString + nameString + positionString + "}, \"op\": \"publish\"}\"";
        }
    }
}
#endif
