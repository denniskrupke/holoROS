using System.Collections;
using System.Collections.Generic;

using RosBridge_old;
using RosMessages_old;

using UnityEngine;
using UnityEngine.UI;

#if UWP
using RosBridge;
using RosMessages;
#endif



public class SetTransform : MonoBehaviour {
    public GameObject cursor;
    public Transform origin;
    public ros2unityManager rosManager;
    public ManageObjectSelection objectSelectionManager = null;
    public Transform startingPosition;
    public ExperimentDataLogger experimentDataLogger = null;
    public Text debugHUD = null;
    public SpeechManager speechManager = null;
    public ParticipantTargetPositioner targetPositioner = null;
    public Transform targetMarker = null;
    public Transform tableTop = null;

    private Vector3 lastPlacePos;
    
	// Use this for initialization
	void Start () {
        //ResetArm();
    }
	
	// Update is called once per frame
	void Update () {
		
	}

    
    public void OnPickUp(){        
        //object1, object2, object3
        //this.transform.position = new Vector3(objectSelectionManager.CurrentSelectedObject.transform.position.x, cursor.transform.position.y, objectSelectionManager.CurrentSelectedObject.transform.position.z);
        Vector3 cursor_pos = cursor.transform.position;
        targetMarker.position = cursor_pos;
        Vector3 target_pos = objectSelectionManager.CurrentSelectedObject.transform.position;
        //Vector3 invPos = objectSelectionManager.CurrentSelectedObject.transform.InverseTransformPoint(cursor.transform.position);
        Vector3 invPos = cursor_pos - target_pos;        
        //float[] data = { invPos.x, -invPos.z, invPos.y };
        RosMessages_old.geometry_msgs.PointStamped_old ps = new RosMessages_old.geometry_msgs.PointStamped_old();        
        RosMessages_old.geometry_msgs.Point_old p = new RosMessages_old.geometry_msgs.Point_old();        
        ps.point = p;        
        ps.point.x = invPos.x; 
        ps.point.y = invPos.z; 
        ps.point.z = invPos.y;        
        Header_old h = new Header_old();        
        ps.header = h;        
        ps.header.frame_id = objectSelectionManager.CurrentSelectedObject.name;        
        rosManager.RosBridge.EnqueRosCommand(new RosPublish_old("/hololens/plan_pick", ps));        
    }

    public void OnPlace()
    {
        //targetMarker.position = cursor.transform.position;
        // TODO
        this.lastPlacePos = cursor.transform.position;
        lastPlacePos.y += 0.129f;
        objectSelectionManager.CurrentSelectedObject.transform.position = lastPlacePos;

        Vector3 invPos = cursor.transform.position - tableTop.position;
        RosMessages_old.geometry_msgs.PointStamped_old ps = new RosMessages_old.geometry_msgs.PointStamped_old();
        RosMessages_old.geometry_msgs.Point_old p = new RosMessages_old.geometry_msgs.Point_old();
        ps.point = p;
        ps.point.x = -invPos.z;
        ps.point.y = invPos.x;
        ps.point.z = invPos.y;
        Header_old h = new Header_old();
        ps.header = h;        
        rosManager.RosBridge.EnqueRosCommand(new RosPublish_old("/hololens/plan_place", ps));
    }

    public void OnConfirmPick(){
        RosMessages_old.std_msgs.Empty_old confirm = new RosMessages_old.std_msgs.Empty_old();        
        rosManager.RosBridge.EnqueRosCommand(new RosPublish_old("/hololens/execute_pick", confirm));
    }

    public void OnConfirmPlace()
    {
        RosMessages_old.std_msgs.Empty_old confirm = new RosMessages_old.std_msgs.Empty_old();
        rosManager.RosBridge.EnqueRosCommand(new RosPublish_old("/hololens/execute_place", confirm));
    }

    public void OnOpenGripper(){
        RosMessages_old.std_msgs.Empty_old confirm = new RosMessages_old.std_msgs.Empty_old();        
        rosManager.RosBridge.EnqueRosCommand(new RosPublish_old("/hololens_open_gripper", confirm));
    }



    



    void OnSetTarget()
    {
        this.transform.position = new Vector3(objectSelectionManager.CurrentSelectedObject.transform.position.x, cursor.transform.position.y, objectSelectionManager.CurrentSelectedObject.transform.position.z);
        Vector3 invPos = origin.transform.InverseTransformPoint(this.transform.position);
        float[] data = { invPos.x, invPos.z, invPos.y };
        Float32MultiArray_old pose = new Float32MultiArray_old();
        pose.data = data;

        if (!speechManager.demoMode) experimentDataLogger.StopTrial();
        //if (!speechManager.demoMode) experimentDataLogger.StopTrial(cursor.transform.position);
        if (speechManager.demoMode) targetPositioner.SetDemoTargetPosition();       
        rosManager.RosBridge.EnqueRosCommand(new RosPublish_old("/unity_arm_control", pose));        
    }


    void OnMove()
    {
        this.transform.position = new Vector3(objectSelectionManager.CurrentSelectedObject.transform.position.x, cursor.transform.position.y, objectSelectionManager.CurrentSelectedObject.transform.position.z);
        Vector3 invPos = origin.transform.InverseTransformPoint(this.transform.position);
        float[] data = { invPos.x, invPos.z, invPos.y };
        Float32MultiArray_old pose = new Float32MultiArray_old();
        pose.data = data;
        
        rosManager.RosBridge.EnqueRosCommand(new RosPublish_old("/unity_arm_control", pose));
    }


    void OnConfirm()
    {
        float[] data = { 23.0f, 23.0f , 23.0f };
        Float32MultiArray_old confirmCode = new Float32MultiArray_old();
        confirmCode.data = data;

        rosManager.RosBridge.EnqueRosCommand(new RosPublish_old("/unity_control_confirm", confirmCode));
    }


    void OnResetArm()
    {
        ResetArm();
    } 


    public void ResetArm()
    {
        this.transform.position = startingPosition.position;
        Vector3 invPos = origin.transform.InverseTransformPoint(this.transform.position);
        float[] data = { invPos.x, invPos.z, invPos.y };
        Float32MultiArray_old pose = new Float32MultiArray_old();
        pose.data = data;
        rosManager.RosBridge.EnqueRosCommand(new RosPublish_old("/unity_arm_control", pose));
    }
}
