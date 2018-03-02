#if WINDOWS_UWP
using RosArrayService;
using RosHeaderCoder;
using RosMessages;
using System.Diagnostics;
using System.Threading.Tasks;
using Windows.Data.Json;

namespace RosJointStateCoder
{

    class RosJointStateCoderParallel : rosComplexCoder
    {
        private JointState _messageData;
        // private bool semaphorMessageData = false;

        public RosJointStateCoderParallel()
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
            if (msg.GetType() == typeof(JointState))
            {
                js = (JointState)msg;
            }

            return serializeJointState(js, topic);
        }

        private RosPublish deserializeJointState(JsonObject jsonObject)
        {
            _messageData = new JointState();

            // Threading
            Task[] tasks = new Task[2]
            {
                 Task.Run(() => baueArrayTeilA(jsonObject["msg"].GetObject()["name"].GetArray(),jsonObject["msg"].GetObject()["position"].GetArray())),
                 Task.Run(() => baueArrayTeilB(jsonObject["msg"].GetObject()["velocity"].GetArray(),jsonObject["msg"].GetObject()["effort"].GetArray()))
            };

            // baue den Header
            Header header = RosHeaderCoder_.deserializeSingleHeader((JsonValue)jsonObject["msg"].GetObject()["header"]);
            _messageData.header = header;

            Task.WaitAll(tasks);
            return new RosPublish("\"/joint_states\"", _messageData);

        }
        // Nutze den ArrayService, um die JSONArrays in "normale" Arrays umzuwandeln

        // Threadingmethoden
        private void baueNameArray(JsonArray array)
        {
            _messageData.name = RosArrayService_.stringArrayAusJsonArray(array);
        }
        private void bauePositionArray(JsonArray array)
        {
            _messageData.position = RosArrayService_.doubleArrayAusJsonArray(array);
        }
        private void baueVelocityArray(JsonArray array)
        {
            _messageData.velocity = RosArrayService_.doubleArrayAusJsonArray(array);
        }
        private void baueEffortArray(JsonArray array)
        {
            _messageData.effort = RosArrayService_.doubleArrayAusJsonArray(array);
        }
        private void baueArrayTeilA(JsonArray array1, JsonArray array2)
        {
            _messageData.name = RosArrayService_.stringArrayAusJsonArray(array1);
            _messageData.position = RosArrayService_.doubleArrayAusJsonArray(array2);
        }
        private void baueArrayTeilB(JsonArray array1, JsonArray array2)
        {
            _messageData.velocity = RosArrayService_.doubleArrayAusJsonArray(array1);
            _messageData.effort = RosArrayService_.doubleArrayAusJsonArray(array2);
        }


        private string serializeJointState(JointState js, string topic)
        {
            //string topic = "\"topic\": \"/joint_states\", "; // platzhalter, topic vom RosMessage- objekt später holen
            string ausgabeTopic = "\"topic\": " + topic + ", ";

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
            return "\"{" + ausgabeTopic + "\"msg\": {" + headerString + velocityString + effortString + nameString + positionString + "}, \"op\": \"publish\"}\"";
        }
    }
}
#endif