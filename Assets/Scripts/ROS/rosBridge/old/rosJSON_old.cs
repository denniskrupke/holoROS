using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;

using System;

using RosMessages_old;

namespace RosJSON_old
{
	public abstract class JsonCreationConverter<T> : JsonConverter
	{
	    /// <summary>
	    /// Create an instance of objectType, based properties in the JSON object
	    /// </summary>
	    /// <param name="objectType">type of object expected</param>
	    /// <param name="jObject">
	    /// contents of JSON object that will be deserialized
	    /// </param>
	    /// <returns></returns>
	    protected abstract T Create(Type objectType, JObject jObject);

	    public override bool CanConvert(Type objectType)
	    {
#if WINDOWS_UWP
            return true;
#else
            return typeof(T).IsAssignableFrom(objectType);
#endif
        }

	    public override object ReadJson(JsonReader reader, 
	                                    Type objectType, 
	                                     object existingValue, 
	                                     JsonSerializer serializer)
	    {
            // Load JObject from stream
            JObject jObject = new JObject();
            try
            {
                jObject = JObject.Load(reader);
            }
            catch
            {

            }
            if (jObject == null) { return null; }
	        // Create target object based on JObject
	        T target = Create(objectType, jObject);

            // Populate the object properties
            try
            {
                serializer.Populate(jObject.CreateReader(), target);
            }
            catch
            {

            }

	        return target;
	    }

	    public override void WriteJson(JsonWriter writer, 
	                                   object value,
	                                   JsonSerializer serializer)
	    {
	        throw new NotImplementedException();
	    }
	}


	public class RosMessageConverter : JsonCreationConverter<RosMessage_old>
	{
	    protected override RosMessage_old Create(Type objectType, JObject jObject)
	    {
            if(jObject == null)
            {
                return new RosMessage_old();
            }
	        else if (jObject["op"].ToString() == "advertise")
	        {
	            return new RosAdvertise_old(jObject["topic"].ToString(), jObject["type"].ToString());
	        }
	        else if (jObject["op"].ToString() == "subscribe")
	        {
	            return new RosSubscribe_old(jObject["topic"].ToString(), jObject["type"].ToString());
	        }
	        else if (jObject["op"].ToString() == "unsubscribe")
	        {
	            return new RosUnsubscribe_old(jObject["topic"].ToString());
	        }
	        else if (jObject["op"].ToString() == "publish")
	        {
                MessageData_old msg = new MessageData_old() ;
                try
                {
                    msg = JsonConvert.DeserializeObject<MessageData_old>(jObject["msg"].ToString(), new MessageDataConverter(jObject["topic"].ToString()));
                }
                catch
                {                    
                }
	            return new RosPublish_old(jObject["topic"].ToString(), msg);
	        }
	        else return new RosMessage_old(); // empty dummy
	    }
	}
	

	public class MessageDataConverter : JsonCreationConverter<MessageData_old>
	{
	    string topic = "";
	    public MessageDataConverter(string topic){
	        this.topic = topic;
	    }

	    protected override MessageData_old Create(Type objectType, JObject jObject)
	    {
	        if (topic == "/SModelRobotInput")
	        {
	            return new InputMessageData_old();
	        }
	        else if (topic == "/SModelRobotOutput")
	        {
	            return new OutputMessageData_old();
	        }
	        else if (topic == "/camera/rgb/image_rect_color/compressed")
	        {
	            return new CompressedImage_old(); 
	        }
	        else if (topic == "/joint_states" || topic == "")
	        {
	        	return new JointState_old();
	        }
	        else 
	        {
	            return new MessageData_old(); // no data inside :-(
	        }
	    }
	}
}