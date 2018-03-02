#if WINDOWS_UWP
using Windows.Data.Json;
using RosMessages;
using RosJointStateCoder;
using RosJointTrajectoryCoder;

class RosBridgeClient {

    public RosBridgeClient(){
    }

    // hier wird einfach hart-gecoded eine JointState-Nachricht deserialisiert
    public RosMessage DeserializeJSONstring(string message)
    {

        JsonObject jsonObject = JsonObject.Parse(message);
        string jtopic = jsonObject["topic"].GetString();
        rosComplexCoder coder = null;
        switch (jtopic)
        {
            case "/joint_states":
            coder = new RosJointStateCoder_();
            return coder.startDeserializing(jsonObject);

            case "/preview_trajectory":
            coder = new RosJointTrajectoryCoder_();
            return coder.startDeserializing(jsonObject);

            default:
                return null;
        }
    }

    // die andere Richtung wäre auch schön, damit man auch Nachrichten an ROS senden kann
    public string SerializeROSmessage(RosMessage message){
        //todo
        
        if (message.GetType() == typeof(RosPublish))
        {
            rosComplexCoder coder = null;
            switch (message.topic)
            {
                case "\"/joint_states\"":
                    coder = new RosJointStateCoder_();
                    return coder.startSerializing(message);

                case "\"/preview_trajectory\"":
                coder = new RosJointTrajectoryCoder_();
                    return coder.startSerializing(message);

                default:
                    return null;
            }
        }
        else
        {
            return null;
        }
    }

}
#endif