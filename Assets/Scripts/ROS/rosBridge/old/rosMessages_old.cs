namespace RosMessages_old{

	/*
		MessageData contains all neccessary information for commands controlling robots
	*/
	public class MessageData_old{}


	// contains information for controlling the robotiq-3finger adaptive gripper
	public class OutputMessageData_old : MessageData_old{
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
	public class InputMessageData_old : MessageData_old
    {
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

	public class Stamp_old
    {
		public int secs;
		public int nsecs;
	}

	public class Header_old
    {
		public Stamp_old stamp;
		public string frame_id; 
		public int seq;
		//public string type;
	}

	public class CompressedImage_old : MessageData_old
    {
		public Header_old header;
		public string data;	
		public string format;
	}

	public class JointState_old : MessageData_old
    {
		public Header_old header;
		public string[] name;
		public double[] position;
		public double[] velocity;
        public double[] effort;
        //public string type = "SModel_robot_output";
    }

    public class Float32MultiArray_old : MessageData_old
    {
        public MultiArrayLayout_old layout;
        public float[] data;
    }

    public class MultiArrayDimension_old
    {
        public string label; //label of given dimension
        public uint size;   //size of given dimension (in type units)
        public uint stride; //stride of given dimension
    }

    public class MultiArrayLayout_old
    {
        public MultiArrayDimension_old[] dim; //Array of dimension properties
        public uint data_offset;    //padding elements at front of data
    }

    namespace std_msgs
    {
        public class Bool_old : MessageData_old
        {
            public bool data;
        }

        public class Empty_old : MessageData_old
        {
            
        }
    }

    namespace geometry_msgs
    {
        public class Vector3_old : MessageData_old
        {
            public double x;
            public double y;
            public double z;
        }

        public class Vector3Stamped_old : MessageData_old
        {
            public Header_old header;
            public Vector3_old vector;
        }

        public class Quaternion_old : MessageData_old
        {
            public double x;
            public double y;
            public double z;
            public double w;
        }

        public class Point_old : MessageData_old
        {
            public double x;
            public double y;
            public double z;
        }

        public class PointStamped_old : MessageData_old
        {
            public Header_old header;
            public Point_old point;
        }

        public class Pose_old : MessageData_old
        {
            public Point_old position;
            public Quaternion_old orientation;
        }

        public class PoseStamped_old : MessageData_old
        {
            public Header_old header;
            public Pose_old pose;
        }

        public class Transform_old : MessageData_old
        {
            public Vector3_old translation;
            public Quaternion_old rotation;
        }

        public class Twist_old : MessageData_old
        {
            public Vector3_old linear;
            public Vector3_old angular;
        }

        public class Wrench_old : MessageData_old
        {
            public Vector3_old force;
            public Vector3_old torque;
        }
    }


    namespace moveit_msgs
    {
        public class GripperTranslation_old : MessageData_old
        {
            public geometry_msgs.Vector3Stamped_old direction;
            public float desired_distance;
            public float min_distance;
        }

        public class Grasp_old : MessageData_old
        {
            public string id;
            public trajectory_msgs.JointTrajectory_old pre_grasp_posture;
            public trajectory_msgs.JointTrajectory_old grasp_posture;
            public geometry_msgs.PoseStamped_old grasp_pose;
            public double grasp_quality;
            public GripperTranslation_old pre_grasp_approach;
            public GripperTranslation_old post_grasp_retreat;
            public GripperTranslation_old post_place_retreat;
            public float max_contact_force;
            public string[] allowed_touch_objects;
        }

        public class PickupAction_old : MessageData_old
        {
            public string target_name;
            public string group_name;
            public string end_effector;
            public Grasp_old[] possible_grasps;
            public string support_surface_name;
            public bool allow_gripper_support_collision;
            public string[] attached_object_touch_links;
            public bool minimize_object_distance;
            public Constraints_old path_constraints;
            public string planner_id;
            public string[] allowed_touch_objects;
            public double allowed_planning_time;
            public PlanningOptions_old planning_options;
            //---
            public int error_code;//public MoveitErrorCodes_old error_code;
            public RobotState_old trajectory_start;
            public RobotTrajectory_old[] trajectory_stages;
            public string[] trajectory_descriptions;
            public Grasp_old grasp;
            //---
            public string state;
        }

        public class RobotTrajectory_old : MessageData_old
        {
            trajectory_msgs.JointTrajectory_old joint_trajectory;
            trajectory_msgs.MultiDOFJointTrajectory_old multi_dof_joint_trajectory;
        }

        public class RobotState_old : MessageData_old
        {
            public JointState_old joint_state;
            public sensor_msgs.MultiDOFJointState_old multi_dof_joint_state;
            public AttachedCollisionObject_old[] attached_collision_objects;
            public bool is_diff;
        }

        public class AttachedCollisionObject_old : MessageData_old
        {
            public string link_name;
            public CollisionObject_old object_collision;
            public string[] touch_links;
            public trajectory_msgs.JointTrajectory_old detach_posture;
            public double weight;
        }

        public class CollisionObject_old : MessageData_old
        {
            public Header_old header;
            public string id;
            public object_recognition_msgs.ObjectType_old type;
            public shape_msgs.SolidPrimitive_old[] primitives;
            public geometry_msgs.Pose_old[] primitive_poses;
            public shape_msgs.Mesh_old[] meshes;
            public geometry_msgs.Pose_old mesh_poses;
            public shape_msgs.Plane_old[] planes;
            public geometry_msgs.Pose_old[] plane_poses;
            public byte operation;
            public const byte ADD=0;
            public const byte REMOVE=1;
            public const byte APPEND=2;
            public const byte MOVE=3;
        }


        public class PlanningOptions_old : MessageData_old
        {
            public PlanningScene_old planning_scene_diff;
            public bool plan_only;
            public bool look_around;
            public int look_around_attempts;
            public double max_safe_execution_cost;
            public bool replan;
            public int replan_attempts;
            public double replan_delay;
        }

        public class PlanningScene_old : MessageData_old
        {
            //todo
        }

        public class Constraints_old : MessageData_old
        {
            public string name;
            public JointConstraint_old[] joint_constraints;
            public PositionConstraint_old[] position_constraints;
            public OrientationConstraint_old[] orientation_constraints;
            public VisibilityConstraint_old[] visibility_constraints;
        }

        public class JointConstraint_old : MessageData_old
        {
            public string joint_name;
            public double position;
            public double tolerance_above;
            public double tolerance_below;
            public double weight;
        }

        public class PositionConstraint_old : MessageData_old
        {
            public Header_old header;
            public string link_name;
            public geometry_msgs.Vector3_old target_point_offset;
            public BoundingVolume_old constraint_region;
            public double weight;
        }

        public class OrientationConstraint_old : MessageData_old
        {
            public Header_old header;
            public geometry_msgs.Quaternion_old orientation;
            public string link_name;
            public double absolute_x_axis_tolerance;
            public double absolute_y_axis_tolerance;
            public double absolute_z_axis_tolerance;
            public double weight;
        }

        public class VisibilityConstraint_old : MessageData_old
        {
            //todo
        }



        public class BoundingVolume_old : MessageData_old
        {
            public shape_msgs.SolidPrimitive_old[] primitives;
            public geometry_msgs.Pose_old[] primitive_poses;
            public shape_msgs.Mesh_old[] meshes;
            public geometry_msgs.Pose_old[] mesh_poses;
        }
    }

    namespace sensor_msgs
    {
        public class MultiDOFJointState_old
        {
            public Header_old header;
            public string[] joint_names;
            public geometry_msgs.Transform_old[] transforms;
            public geometry_msgs.Twist_old[] twist;
            public geometry_msgs.Wrench_old[] wrench;
        }
    }


    namespace shape_msgs
    {
        public class SolidPrimitive_old : MessageData_old
        {
        //todo
        }

        public class Mesh_old : MessageData_old
        {
        //todo
        }

        public class Plane_old : MessageData_old
        {

        }         
    }

    namespace object_recognition_msgs
    {
        public class ObjectType_old : MessageData_old
        {
            // TODO
        }
    }

    namespace trajectory_msgs
    {

        public class JointTrajectoryPoint_old : MessageData_old
        {
            public double[] positions;
            public double[] velocities;
            public double[] accelerations;
            public double[] effort;
            public long time_from_start; 
        }

        public class JointTrajectory_old : MessageData_old
        {
            public Header_old header;
            public string[] joint_names;
            public JointTrajectoryPoint_old[] points;
        }

        public class MultiDOFJointTrajectory_old : MessageData_old
        {
            public Header_old header;
            public string[] joint_names;
            MultiDOFJointTrajectoryPoint_old[] points;
        }

        public class MultiDOFJointTrajectoryPoint_old : MessageData_old
        {
            public geometry_msgs.Transform_old[] transforms;
            public geometry_msgs.Twist_old[] velocities;
            public geometry_msgs.Twist_old[] accelerations;
            public long time_from_start;
        }

    }

    /*
        public class MultiDOFJointState : MessageData{
            public Header header;
            public string[] joint_names;
            public Transform[] transforms;
            public Twist[] twist;
            public Wrench[] wrench;                
        }

        public class Transform : MessageData {
            public Vector3 translation;
            public Quaternion rotation;
        }

        public class Vector3 : MessageData {
            public double x;
            public double y;
            public double z;
        }

        public class Point : MessageData {
            public double x;
            public double y;
            public double z;
        }

        public class Quaternion : MessageData {
            public double x;
            public double y;
            public double z;
            public double w;
        }

        public class Twist : MessageData {
            public Vector3 linear;
            public Vector3 angular;
        }

        public class Wrench : MessageData {
            public Vector3 force;
            public Vector3 torque;
        }

        public class AttachedCollisionObject : MessageData {
            public string link_name;
            public CollisionObject object;
            string[] touch_links;
            JointTrajectory detach_posture;
            double weight;
        }

        public class CollisionObject : MessageData {
            public const byte ADD=0;
            public const byte REMOVE=1;
            public const byte APPEND=2;
            public const byte MOVE=3;
            public Header header;
            public string id;
            public ObjectType type;
            public SolidPrimitive[] primitives;
            public Pose[] primitive_poses;
            public Mesh[] meshes;
            public Pose[] mesh_poses;
            public Plane[] planes;
            public Pose[] plane_poses;
            public byte operation;
        }

        public class ObjectType { //object_recognition_msgs/ObjectType type
            // http://docs.ros.org/api/object_recognition_msgs/html/msg/ObjectType.html}

        public class SolidPrimitive {
            public const byte BOX=1;
            public const byte SPHERE=2;
            public const byte CYLINDER=3;
            public const byte CONE=4;
            public const byte BOX_X=0;
            public const byte BOX_Y=1;
            public const byte BOX_Z=2;
            public const byte SPHERE_RADIUS=0;
            public const byte CYLINDER_HEIGHT=0;
            public const byte CYLINDER_RADIUS=1;
            public const byte CONE_HEIGHT=0;
            public const byte CONE_RADIUS=1;
            public byte type;
            public double[] dimensions;
        }

        public class Pose {
            public Point position;
            public Quaternion orientation;
        }

        public class Mesh {
            public MeshTriangle[] triangles;
            public Point[] vertices;
        }

        public class MeshTriangle{
            public uint[3] vertex_indices;
        }

        public class Plane {

                # Representation of a plane, using the plane equation ax + by + cz + d = 0

                # a := coef[0]
                # b := coef[1]
                # c := coef[2]
                # d := coef[3]

            double[4] coef;
        }

        



        public class JointTrajectory : MessageData {
            public Header header;
            public string[] joint_names;
            public JointTrajectoryPoint[] points;
        }

        public class JointTrajectoryPoint {
            public double[] positions, velocities, accelerations, effort;
            public long time_from_start;
        }

        public class RobotState {
            public JointState joint_state;
            public MultiDOFJointState multi_dof_joint_state;
            public AttachedCollisionObject[] attached_collision_objects;
            public bool is_diff;
        }

    */

        
    /*
		Generates proper ROS message objects which can be serialized to JSON strings 
	*/
    public class RosMessage_old
    {
		public static int _id = 0;
		public string op;
		public string id;
		public string topic;
		public string type;
	}

	public class RosAdvertise_old : RosMessage_old
    {
		//public string type;
		public string queue_size;
		public string latch;

		public RosAdvertise_old(string topic, string messageType){
			++RosMessage_old._id;
			this.op = "advertise";
			this.id = "advertise:"+topic+":"+_id;
			this.type = messageType;
			this.topic = topic;
			this.latch = "false";
			this.queue_size = "100";
		}
	}

	public class RosPublish_old : RosMessage_old
    {
		public MessageData_old msg;
		public bool latch;

		public RosPublish_old(string topic, MessageData_old messageData){
			++RosMessage_old._id;
			this.op = "publish";
			this.id = "publish:"+topic+":"+_id;
			this.topic = topic;
			this.msg = messageData;
			this.latch = false;
		}
	}

	public class RosSubscribe_old : RosMessage_old
    {
		//public string type;
		public string compression;
		public int throttle_rate;
		public int queue_length;

		public RosSubscribe_old(string topic, string messageType){
			++RosMessage_old._id;
			this.op = "subscribe";
			this.id = "subscribe:"+topic+":"+_id;
			this.type = messageType;
			this.topic = topic;
			this.compression = "none";
			this.throttle_rate = 0;
			this.queue_length = 0;
		}

        public RosSubscribe_old(string topic, string messageType, int rate) : this(topic, messageType)
        {            
            this.throttle_rate = rate;           
        }
    }

	public class RosUnsubscribe_old : RosMessage_old
    {
		public RosUnsubscribe_old(string topic){
			++RosMessage_old._id;
			this.op = "unsubscribe";
			this.id = "unsubscribe:"+topic+":"+_id;
			this.topic = topic;
		}
	}

}