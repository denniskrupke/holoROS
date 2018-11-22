using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MarkerManager : MonoBehaviour {

    //The Vuforia markers
    [SerializeField]
    private List<GameObject> markers;    

    //The master marker used as an anker for the world
    [SerializeField]
    private GameObject objectToTransform;//this is the MainAprilTag

    //The center position of the mounting plate of the robot (the following markers are attached to the corners of this plate)
    [SerializeField]
    private GameObject mountPlate;

    //----------- The markers in the world --------------
    [SerializeField]
    private GameObject marker1;

    [SerializeField]
    private GameObject marker2;

    [SerializeField]
    private GameObject marker3;

    [SerializeField]
    private GameObject marker4;
    //---------------------------------------------------

    [SerializeField]
    private Text debugText; //this can be used to print some debug information as part of the HUD

    private Vector3 trackedPosition;
    private Vector3 trackedForwardDirection;
    private Vector3 trackedUpDirection;
    private Quaternion startRot;
    private Vector3 startForward;

    private Dictionary<string, Vector3> lastPositions;  //positions of the detected markers
    private Dictionary<string, Vector3> lastForwardDirections; //forward directions of the detected markers
    private Dictionary<string, Vector3> lastUpDirections; //up directions of the detected markers
    private Dictionary<string, bool> trackingStatus;
    private Dictionary<string, bool> validPose;
    
    private int filterCount = 0;
    
    private Vector3 fromPlateToMain;
    private Vector3 fromM1ToPlate;
    private Vector3 fromM2ToPlate;
    private Vector3 fromM3ToPlate;
    private Vector3 fromM4ToPlate;

    private Vector3 fromPlateToMainForward;
    private Vector3 fromM1ToPlateForward;
    private Vector3 fromM2ToPlateForward;
    private Vector3 fromM3ToPlateForward;
    private Vector3 fromM4ToPlateForward;

    private Vector3 fromPlateToMainUp;
    private Vector3 fromM1ToPlateUp;
    private Vector3 fromM2ToPlateUp;
    private Vector3 fromM3ToPlateUp;
    private Vector3 fromM4ToPlateUp;

    //Vector3 scaleXZToZero = new Vector3(1, 1, 0);

    //updates poses if markers are currently recognized
    private void updatePoses()
    {
        foreach (GameObject marker in markers)
        {
            // the big marker
            if (marker.transform.parent.name.Contains("000") && trackingStatus["000"])
            {
                lastPositions["000"] = marker.transform.position;
                lastForwardDirections["000"] = marker.transform.forward;
                lastUpDirections["000"] = marker.transform.up;
                validPose["000"] = true;
            }
            if (marker.transform.parent.name.Contains("128") && trackingStatus["128"])
            {
                lastPositions["128"] = marker.transform.position - fromM1ToPlate - fromPlateToMain;
                lastForwardDirections["128"] = marker.transform.forward - fromM1ToPlateForward - fromPlateToMainForward;
                lastUpDirections["128"] = marker.transform.up - fromM1ToPlateUp - fromPlateToMainUp;
                validPose["128"] = true;
            }
            if (marker.transform.parent.name.Contains("154") && trackingStatus["154"])
            {
                lastPositions["154"] = marker.transform.position - fromM2ToPlate - fromPlateToMain;
                lastForwardDirections["154"] = marker.transform.forward - fromM1ToPlateForward - fromPlateToMainForward;
                lastUpDirections["154"] = marker.transform.up - fromM1ToPlateUp - fromPlateToMainUp;
                validPose["154"] = true;
            }
            if (marker.transform.parent.name.Contains("162") && trackingStatus["162"])
            {
                lastPositions["162"] = marker.transform.position - fromM3ToPlate - fromPlateToMain;
                lastForwardDirections["162"] = marker.transform.forward - fromM1ToPlateForward - fromPlateToMainForward;
                lastUpDirections["162"] = marker.transform.up - fromM1ToPlateUp - fromPlateToMainUp;
                validPose["162"] = true;
            }
            if (marker.transform.parent.name.Contains("164") && trackingStatus["164"])
            {
                lastPositions["164"] = marker.transform.position - fromM4ToPlate - fromPlateToMain;
                lastForwardDirections["164"] = marker.transform.forward - fromM1ToPlateForward - fromPlateToMainForward;
                lastUpDirections["164"] = lastUpDirections["164"] = marker.transform.up;
                validPose["164"] = true;
            }
            // xyz="0.2255 -0.025 0.032" mount plate            
        }
    }

    void Start () {
        //raw positions from vuforia marker detection
        lastPositions = new Dictionary<string, Vector3>();
        lastPositions.Add("000", new Vector3());
        lastPositions.Add("128", new Vector3());
        lastPositions.Add("154", new Vector3());
        lastPositions.Add("162", new Vector3());
        lastPositions.Add("164", new Vector3());

        //raw forward directions from vuforia marker detection
        lastForwardDirections = new Dictionary<string, Vector3>();
        lastForwardDirections.Add("000", new Vector3());
        lastForwardDirections.Add("128", new Vector3());
        lastForwardDirections.Add("154", new Vector3());
        lastForwardDirections.Add("162", new Vector3());
        lastForwardDirections.Add("164", new Vector3());

        //raw up directions from vuforia marker detection
        lastUpDirections = new Dictionary<string, Vector3>();
        lastUpDirections.Add("000", new Vector3());
        lastUpDirections.Add("128", new Vector3());
        lastUpDirections.Add("154", new Vector3());
        lastUpDirections.Add("162", new Vector3());
        lastUpDirections.Add("164", new Vector3());

        //is a named marker currently tracked
        trackingStatus = new Dictionary<string, bool>();
        trackingStatus.Add("000", false);
        trackingStatus.Add("128", false);
        trackingStatus.Add("154", false);
        trackingStatus.Add("162", false);
        trackingStatus.Add("164", false);

        //has a named marker successfully tracked
        validPose = new Dictionary<string, bool>();
        validPose.Add("000", false);
        validPose.Add("128", false);
        validPose.Add("154", false);
        validPose.Add("162", false);
        validPose.Add("164", false);

        //original pose
        startRot = objectToTransform.transform.rotation;
        startForward = objectToTransform.transform.forward;
        //startForward.Scale(scaleXZToZero);

        //fixed translations between frames, known by static scene geometry
        fromPlateToMain = objectToTransform.transform.InverseTransformPoint(mountPlate.transform.position);
        fromM1ToPlate = mountPlate.transform.InverseTransformPoint(marker1.transform.position);
        fromM2ToPlate = mountPlate.transform.InverseTransformPoint(marker2.transform.position);
        fromM3ToPlate = mountPlate.transform.InverseTransformPoint(marker3.transform.position);
        fromM4ToPlate = mountPlate.transform.InverseTransformPoint(marker4.transform.position);

        //fixed forward rotations between frames, known by static scene geometry
        fromPlateToMainForward = objectToTransform.transform.InverseTransformDirection(mountPlate.transform.forward);
        fromM1ToPlateForward = mountPlate.transform.InverseTransformDirection(marker1.transform.forward);
        fromM2ToPlateForward = mountPlate.transform.InverseTransformDirection(marker2.transform.forward);
        fromM3ToPlateForward = mountPlate.transform.InverseTransformDirection(marker3.transform.forward);
        fromM4ToPlateForward = mountPlate.transform.InverseTransformDirection(marker4.transform.forward);

        //fixed up rotations between frames, known by static scene geometry
        fromPlateToMainUp = objectToTransform.transform.InverseTransformDirection(mountPlate.transform.up);
        fromM1ToPlateUp = mountPlate.transform.InverseTransformDirection(marker1.transform.up);
        fromM2ToPlateUp = mountPlate.transform.InverseTransformDirection(marker2.transform.up);
        fromM3ToPlateUp = mountPlate.transform.InverseTransformDirection(marker3.transform.up);
        fromM4ToPlateUp = mountPlate.transform.InverseTransformDirection(marker4.transform.up);
    }
	
	// updates the lab model frame pose according to the last marker detections by calculating mean rotation and translation
	void Update () {
        updatePoses();        

        int count = 0;        
        trackedPosition = new Vector3(); //position
        trackedForwardDirection = new Vector3();//forward
        trackedUpDirection = new Vector3();//up

        if (trackingStatus["000"])
        {
            trackedPosition += lastPositions["000"];
            trackedForwardDirection += lastForwardDirections["000"];
            trackedUpDirection += lastUpDirections["000"];
            count++;
        }
        if (trackingStatus["128"])
        {
            trackedPosition += lastPositions["128"];
            trackedForwardDirection += lastForwardDirections["128"];
            trackedUpDirection += lastUpDirections["128"];
            count++;
        }
        if (trackingStatus["154"])
        {
            trackedPosition += lastPositions["154"];
            trackedForwardDirection += lastForwardDirections["154"];
            trackedUpDirection += lastUpDirections["154"];
            count++;
        }
        if (trackingStatus["162"])
        {
            trackedPosition += lastPositions["162"];
            trackedForwardDirection += lastForwardDirections["162"];
            trackedUpDirection += lastUpDirections["162"];
            count++;
        }
        if (trackingStatus["164"])
        {
            trackedPosition += lastPositions["164"];
            trackedForwardDirection += lastForwardDirections["164"];
            trackedUpDirection += lastUpDirections["164"];
            count++;
        }

        if (count > 0) //if there is currently at least one detected marker we normalize the vectors
        {
            trackedPosition /= (float)(count * 1.0f);
            trackedForwardDirection /= (float)(count * 1.0f);
            trackedUpDirection /= (float)(count * 1.0f);
        }
        
        //debugText.text = "" + "x:" + trackedPosition.x + " y:" + trackedPosition.y + " z:" + trackedPosition.z;

        // pose updates are applied once per second
        if (filterCount++ > 60) {            
            objectToTransform.transform.position = trackedPosition;
            //trackedDirection.Scale(scaleXZToZero);
            objectToTransform.transform.rotation = startRot * Quaternion.FromToRotation(new Vector3(0, 1, 0), trackedUpDirection)
                                                            //* Quaternion.FromToRotation(new Vector3(0, 0, 1), trackedForwardDirection)                                                             
                                                            * Quaternion.Euler(0, -90, 0)
                                                            
                                                            ;//startRot * Quaternion.FromToRotation(startForward, trackedDirection);
            //objectToTransform.transform.up = trackedUpDirection;
            filterCount = 0;
        }
    }        

    

    public void SetStatus(string markerid, bool status)
    {
        trackingStatus[markerid] = status;
    }
    
}

