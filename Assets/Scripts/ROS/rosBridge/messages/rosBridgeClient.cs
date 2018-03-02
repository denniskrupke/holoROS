#if WINDOWS_UWP
using Windows.Data.Json;
using RosMessages;
using RosJointStateCoder;
using RosJointTrajectoryCoder;
using Config;

class RosBridgeClient {

    public RosBridgeClient(){
    }

    // Schnittstelle des Parsers. Hier wird die Deserialisierung aufgerufen.
    public RosMessage DeserializeJSONstring(string message)
    {
        JsonObject jsonObject = JsonObject.Parse(message);
        string jtopic = jsonObject["topic"].GetString();
        rosComplexCoder coder = null;
        if (parserConfig.getJointStateTopics().Contains(jtopic))
        {
            coder = new RosJointStateCoderParallel();
            return coder.startDeserializing(jsonObject);
        }
        else if (parserConfig.getJointTrajectoryTopics().Contains(jtopic))
        {
            coder = new RosJointTrajectoryCoder_();
            return coder.startDeserializing(jsonObject);
        }
        else
        {
            return null;
        }
    }

    // Schnittstelle des Parsers. Hier wird die Serialisierung aufgerufen.
    public string SerializeROSmessage(RosMessage message)
    {

        if (message.GetType() == typeof(RosPublish))
        {
            rosComplexCoder coder = null;
            if (parserConfig.getJointStateTopics().Contains(message.topic))
            {
                coder = new RosJointStateCoderSeriell();
                return coder.startSerializing(message);
            }
            else if (parserConfig.getJointTrajectoryTopics().Contains(message.topic))
            {
                coder = new RosJointTrajectoryCoder_();
                return coder.startSerializing(message);
            }
            else
            {
                return null;
            }
        }
        return null;
    }

}
#endif