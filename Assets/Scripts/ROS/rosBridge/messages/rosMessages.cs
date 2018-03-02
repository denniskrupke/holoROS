#if WINDOWS_UWP
using Windows.Data.Json;

namespace RosMessages{

	/*
		MessageData contains all neccessary information for commands controlling robots
	*/
	public class MessageData{}

	
	/*
		Alle Klassen, die einen komplexen Datentyp, also einen, der als Nachricht verschickt werden kann,
		müssen das "rosComplexCoder" Interface implementieren.
	*/
    public interface rosComplexCoder
    {
        RosPublish startDeserializing(JsonObject jsonobject);
        string startSerializing(RosMessage message);
    }


	// contains information for controlling the robotiq-3finger adaptive gripper
	public class OutputMessageData : MessageData{
		public uint rACT = 0;
		public uint rMOD = 0;
		public uint rGTO = 0;
		public uint rATR = 0;
		public uint rGLV = 0;
		public uint rICF = 0;
		public uint rICS = 0;
		public uint rPRA = 0;
		public uint rSPA = 0;
		public uint rFRA = 0;
		public uint rPRB = 0;
		public uint rSPB = 0;
		public uint rFRB = 0;
		public uint rPRC = 0;
		public uint rSPC = 0;
		public uint rFRC = 0;
		public uint rPRS = 0;
		public uint rSPS = 0;
		public uint rFRS = 0;
	}

	// contains information about the current state of the robotiq-3finger adaptive gripper
	public class InputMessageData : MessageData{
		public uint gACT = 0; 
		public uint gMOD = 0; 
		public uint gGTO = 0; 
		public uint gIMC = 0; 
		public uint gSTA = 0; 
		public uint gDTA = 0; 
		public uint gDTB = 0; 
		public uint gDTC = 0; 
		public uint gDTS = 0; 
		public uint gFLT = 0; 
		public uint gPRA = 0; 
		public uint gPOA = 0; 
		public uint gCUA = 0; 
		public uint gPRB = 0; 
		public uint gPOB = 0; 
		public uint gCUB = 0; 
		public uint gPRC = 0; 
		public uint gPOC = 0; 
		public uint gCUC = 0; 
		public uint gPRS = 0; 
		public uint gPOS = 0; 
		public uint gCUS = 0;
	}

// Anfang std_msg
	
	public class Bool{
		public bool data;
	}
	
	public class Byte{
		public byte data;
	}
	
	public class ByteMultiArray{
		public MultiArrayLayout layout;
		public byte[] data;
	}
	
	public class Char{
		public char data;
	}
	
	public class ColorRGBA{
		public float r;
		public float g;
		public float b;
		public float a;
	}
	
	public class Duration{
		public int secs;
        public int nsecs;
	}
	
	public class Empty{
		
	}
	
	public class Float32{
		public float data;
	}
	
	public class Float32MultiArray{
		public MultiArrayLayout layout;
		public float[] data;
	}
	
	public class Float64{
		public double data;
	}
	
	public class Float64MultiArray{
		public MultiArrayLayout layout;
		public double[] data;
	}
	
	public class Header{
		public Stamp stamp;
		public string frame_id; 
		public int seq;
		//public string type;
	}
	
	public class Int16{
		public short data;
	}
	
	public class Int16MultiArray{
		public MultiArrayLayout layout;
		public short[] data;
	}
	
	public class Int32{
		public int data;
	}
	
	public class Int32MultiArray{
		public MultiArrayLayout layout;
		public int[] data;
	}
	
	public class Int64{
		public long data;
	}
	
	public class Int64MultiArray{
		public MultiArrayLayout layout;
		public long[] data;
	}
	
	public class Int8{
		public sbyte data;
	}
	
	public class Int8MultiArray{
		public MultiArrayLayout layout;
		public sbyte[] data;
	}
	
	public class MultiArrayDimension{
		public string label;
		public uint size;
		public uint stride;
	}
	
	public class MultiArrayLayout{
		public MultiArrayDimension[] dim;
		public uint data_offset;
	}
	
	public class String{
		public string data;
	}
	
	public class Time{
		public string data;
	}
	
	public class UInt16{
		public ushort data;
	}
	
	public class UInt16MultiArray{
		public MultiArrayLayout layout;
		public ushort[] data;
	}
	
	public class UInt32{
		public uint data;
	}
	
	public class UInt32MultiArray{
		public MultiArrayLayout layout;
		public uint[] data;
	}
	
	public class UInt64{
		public ulong data;
	}
	
	public class UInt64MultiArray{
		public MultiArrayLayout layout;
		public ulong[] data;
	}
	
	public class UInt8{
		public byte data;
	}
	
	public class UInt8MultiArray{
		public MultiArrayLayout layout;
		public byte[] data;
	}
    

// Ende std_msg
// Anfang trajectory_msgs

    public class JointTrajectoryPoint
    {
        public double[] positions;
        public double[] velocities;
        public double[] accelerations;
        public double[] effort;
        public Duration time_from_start;
    }

    public class JointTrajectory : MessageData{
        public Header header;
        public string[] joint_names;
        public JointTrajectoryPoint[] points;
    }

	public class CompressedImage : MessageData{
		public Header header;
		public string data;	
		public string format;
	}

	public class JointState : MessageData{
		public Header header;
		public string[] name;
		public double[] position;
		public double[] velocity;
		public double[] effort;
		//public string type = "SModel_robot_output";
	}
	
	public class Stamp{
		public int secs;
		public int nsecs;
	}

	/*
		Generates proper ROS message objects which can be serialized to JSON strings 
	*/
	public class RosMessage{
		public static int _id = 0;
		public string op;
		public string id;
		public string topic;
		public string type;
	}

	public class RosAdvertise : RosMessage{
		//public string type;
		public string queue_size;
		public string latch;

		public RosAdvertise(string topic, string messageType){
			++RosMessage._id;
			this.op = "advertise";
			this.id = "advertise:"+topic+":"+_id;
			this.type = messageType;
			this.topic = topic;
			this.latch = "false";
			this.queue_size = "100";
		}
	}

	public class RosPublish : RosMessage{
		public MessageData msg;
		public bool latch;

		public RosPublish(string topic, MessageData messageData){
			++RosMessage._id;
			this.op = "publish";
			this.id = "publish:"+topic+":"+_id;
			this.topic = topic;
			this.msg = messageData;
			this.latch = false;
		}
	}

	public class RosSubscribe : RosMessage{
		//public string type;
		public string compression;
		public int throttle_rate;
		public int queue_length;

		public RosSubscribe(string topic, string messageType){
			++RosMessage._id;
			this.op = "subscribe";
			this.id = "subscribe:"+topic+":"+_id;
			this.type = messageType;
			this.topic = topic;
			this.compression = "none";
			this.throttle_rate = 0;
			this.queue_length = 0;
		}
	}

	public class RosUnsubscribe : RosMessage{
		public RosUnsubscribe(string topic){
			++RosMessage._id;
			this.op = "unsubscribe";
			this.id = "unsubscribe:"+topic+":"+_id;
			this.topic = topic;
		}
	}

}
#endif