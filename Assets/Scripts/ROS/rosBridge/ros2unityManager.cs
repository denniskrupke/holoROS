using UnityEngine;
using UnityEngine.UI;
using RosBridge_old;


public class ros2unityManager : MonoBehaviour {
	public bool autoConnect = false;
	public bool verbose = true;
    public Text debugHUD = null;
    public Text statusHUD = null;

    public bool imageStreaming = false;
    public drawImage canvas;

    public bool jointStates = true;
    public actuateManipulator manipulatorControl;
    public ActuateRobot robotControl = null;
    public actuateGripper gripperControl = null;

    public bool handTrackingAprilTags = true;
	//private bool useLeap = false;
	private bool testLatency = false;

	
	
	//public processLeapFrames leapController;

	private RosBridgeClient_old rosBridge;
	//private HandControlMessageGenerator gripperMsgGen;
	//private JointStateGenerator jointStateMsgGen;

	//private int millisSinceLastGripperCommand = Environment.TickCount;
	//private static int messageSeq = 0;
        
    

    private int count = 0;
    private int countMax = 20;//480;

    private bool subscribedToTopics = false;

    

    internal RosBridgeClient_old RosBridge
    {
        get
        {
            return rosBridge;
        }

        set
        {
            rosBridge = value;
        }
    }

    void Start () {
		//gripperMsgGen = new HandControlMessageGenerator ();
		rosBridge = new RosBridgeClient_old (this.verbose, this.imageStreaming, this.jointStates, this.testLatency, this.debugHUD, this.handTrackingAprilTags, this.statusHUD);
        
        rosBridge.MaybeLog("Try to connect");        
        if (autoConnect) {
            //debugHUD.text = "\n Try to connect..." + debugHUD.text;
            this.Connect ();
		}

		//leapController.Activate (useLeap);

		//if (testLatency) {
		//	jointStateMsgGen = new JointStateGenerator ();
		//	rosBridge.EnqueRosCommand (new RosAdvertise ("/SModelRobotOutput", "SModel_robot_output"));
		//}

        /*
        if (autoConnect)
        {
            if (!rosBridge.IsConnected())
            {
                Connect();
            }
        }
        */
	}

	// starts the connection to the ROSbridge
	private void Connect(){
        //millisSinceLastGripperCommand = Environment.TickCount;
        rosBridge.MaybeLog("Try to connect with ROSbridge via websockets.");        
        //debugHUD.text = "\n Try to connect with ROSbridge via websockets." + debugHUD.text;
        rosBridge.Start ();
	}


	/* 
	 * Here commands to the robot-side can be send
	 */
	void Update () {
        //this.debugHUD.text = "messageCount";// + rosBridge.messageCount;
        
        /*		
		if (Input.GetKeyDown (KeyCode.C)) {
			rosBridge.EnqueRosCommand (new RosPublish ("/SModelRobotOutput", HandControlMessageGenerator.closeHand (1.0f)));
		} else if (Input.GetKeyDown (KeyCode.O)) {
			rosBridge.EnqueRosCommand (new RosPublish ("/SModelRobotOutput", HandControlMessageGenerator.openHand (1.0f)));
		} else if (Input.GetKey (KeyCode.M)) {	//sends a perormance-test ping message	
			JointState jointState = JointStateGenerator.emptyJointState();
			Stamp stamp = new Stamp ();
			DateTime currentTime = DateTime.Now;
			stamp.secs = currentTime.Second;
			stamp.nsecs = currentTime.Millisecond;
			Header header = new Header ();
			header.stamp = stamp;
			header.seq = messageSeq++;
			header.frame_id = "Latency Test";
			jointState.header = header;
			rosBridge.EnqueRosCommand (new RosPublish ("/joint_states", jointState));
			//rosBridge.EnqueRosCommand (new RosPublish ("/SModelRobotOutput", HandControlMessageGenerator.openHand (1.0f)));
		}
		else if (Input.GetKeyDown(KeyCode.W)){
			//rosBridge.WriteLatencyDataFile();
		}
        */
    }


	// for efficiency reasons, motion of the robot joints and updates of the streamed video are done with 30 FPS
	void FixedUpdate(){
        this.statusHUD.text = ""+rosBridge.messageCount;
        if (robotControl != null && rosBridge.GetLatestJoinState() != null && rosBridge.GetLatestJoinState().name != null)
        {
            //debugHUD.text = "\n Try to update robot control values." + debugHUD.text;
            robotControl.Names = rosBridge.GetLatestJoinState().name;
            robotControl.Angles = rosBridge.GetLatestJoinState().position;
            // gripperControl.Names = rosBridge.GetLatestJoinState().name;
            // gripperControl.Angles = rosBridge.GetLatestJoinState().position;
        }
        // if (count == 0) debugHUD.text = "";
        /* if (robotControl != null && rosBridge.GetLatestJoinState() != null && rosBridge.GetLatestJoinState().name != null)
         {
             //debugHUD.text = "\n Try to update robot control values." + debugHUD.text;
             robotControl.Names = rosBridge.GetLatestJoinState().name;
             robotControl.Angles = rosBridge.GetLatestJoinState().position;
            // gripperControl.Names = rosBridge.GetLatestJoinState().name;
            // gripperControl.Angles = rosBridge.GetLatestJoinState().position;
         }
         */
        count += 1;
        if (count > countMax)
        {
            count = 0;
            //debugHUD.text = "\n April Position: " + rosBridge.GetLatestHandPosition().x + " " + rosBridge.GetLatestHandPosition().y + " " + rosBridge.GetLatestHandPosition().z + " " + debugHUD.text;
            //debugHUD.text = "\n status connected: " +rosBridge.IsConnected() + debugHUD.text;
            //debugHUD.text = "\n message count: " + rosBridge.messageCount + debugHUD.text;            
            //debugHUD.text = "" + "\n " + rosBridge.IncomingMessageString + debugHUD.text;

            //if (rosBridge.GetLatestJoinState() != null && rosBridge.GetLatestJoinState().name != null) debugHUD.text = "" + "\n " + rosBridge.GetLatestJoinState().name[1] + ": " + rosBridge.GetLatestJoinState().position[1] + debugHUD.text;            
            //debugHUD.text = "" + "\n " + rosBridge.RosMessageStrings.Count + debugHUD.text;          

            /*
            if (robotControl != null && rosBridge.GetLatestJoinState() != null && rosBridge.GetLatestJoinState().name != null) {
                robotControl.Names = rosBridge.GetLatestJoinState().name;
                robotControl.Angles = rosBridge.GetLatestJoinState().position;
            }
            */

            if (!rosBridge.IsConnected())
            {
                rosBridge.Communicate();
            }
            else
            {
                if (!subscribedToTopics)
                {
#if WINDOWS_UWP
                    //debugHUD.text = "\n subscribing to topics..." + debugHUD.text;
                    rosBridge.SubscribeToTopics();
                    subscribedToTopics = true;
#endif
                }
            }         
        }
        
        /*
        for (int i = 0; i < 20; i++)
        {
            if (millisSinceLastGripperCommand + 3000 > Environment.TickCount)
            {
                return;
            }
            Connect();            
        }
        */

        // processing is only reasonable if connected to the ROSbridge
        //if (rosBridge.IsConnected()) {
        /*
        if (imageStreaming) {
            // displaying the streamed images from the openni2 node on a canvas
            canvas.showImage (rosBridge.GetLatestImage ().data);
        } */
        //if (jointStates) {
        // synchronizes the virtual robot with the joint state of the real one       

        //manipulatorControl.UpdateJointStates (rosBridge.GetLatestJoinState ().name, rosBridge.GetLatestJoinState ().position);

			//}
            /*
			if (useLeap) {
				// processes data from tracked hands with reduced rate of 10Hz and sends control commands to the real gripper with the same rate
				if (millisSinceLastGripperCommand + 100 > Environment.TickCount) {
					return;
				}
				millisSinceLastGripperCommand = Environment.TickCount;
				//TODO normalizing to a reasonable range
				//rosBridge.EnqueRosCommand (new RosPublish ("/SModelRobotOutput", HandControlMessageGenerator.closeHand (leapController.GetHandClosingState ()))); //direct mapping between PinchStrength and ClosingState of the gripper
			}*/
	//	} 
    /*else {
			canvas.showTestImage ();
		}*/
        
    }


	// Closes the connection to the ROSbridge
	void OnDestroy(){
        //debugHUD.text = "Shutting down...";
        rosBridge.Disconnect ();
        rosBridge.Stop();
	}
}
