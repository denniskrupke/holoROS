using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MarkerManager : MonoBehaviour {

    [SerializeField]
    private List<GameObject> markers;    

    [SerializeField]
    private GameObject objectToTransform;//this is the MainAprilTag

    [SerializeField]
    private GameObject mountPlate;

    [SerializeField]
    private GameObject marker1;

    [SerializeField]
    private GameObject marker2;

    [SerializeField]
    private GameObject marker3;

    [SerializeField]
    private GameObject marker4;

    [SerializeField]
    private Text debugText;

    private Vector3 trackedPosition;
    private Vector3 trackedDirection;
    private Quaternion startRot;
    private Vector3 startForward;

    private Dictionary<string, Vector3> lastPositions;
    private Dictionary<string, Vector3> lastDirections;
    private Dictionary<string, bool> trackingStatus;
    private Dictionary<string, bool> validPose;
    
    private int filterCount = 0;
    
    private Vector3 fromPlateToMain;
    private Vector3 fromM1ToPlate;
    private Vector3 fromM2ToPlate;
    private Vector3 fromM3ToPlate;
    private Vector3 fromM4ToPlate;

    Vector3 scaleXZToZero = new Vector3(1, 1, 0);

    void Start () {
        //raw positions from vuforia marker detection
        lastPositions = new Dictionary<string, Vector3>();
        lastPositions.Add("000", new Vector3());
        lastPositions.Add("128", new Vector3());
        lastPositions.Add("154", new Vector3());
        lastPositions.Add("162", new Vector3());
        lastPositions.Add("164", new Vector3());

        //raw forward directions from vuforia marker detection
        lastDirections = new Dictionary<string, Vector3>();
        lastDirections.Add("000", new Vector3());
        lastDirections.Add("128", new Vector3());
        lastDirections.Add("154", new Vector3());
        lastDirections.Add("162", new Vector3());
        lastDirections.Add("164", new Vector3());

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
    }
	
	// updates the lab model frame pose according to the last marker detections by calculating mean rotation and translation
	void Update () {
        updatePoses();        

        int count = 0;        
        trackedPosition = new Vector3();
        trackedDirection = new Vector3();

        if (trackingStatus["000"])
        {
            trackedPosition += lastPositions["000"];
            trackedDirection += lastDirections["000"];
            count++;
        }
        if (trackingStatus["128"])
        {
            trackedPosition += lastPositions["128"];
            trackedDirection += lastDirections["128"];
            count++;
        }
        if (trackingStatus["154"])
        {
            trackedPosition += lastPositions["154"];
            trackedDirection += lastDirections["154"];
            count++;
        }
        if (trackingStatus["162"])
        {
            trackedPosition += lastPositions["162"];
            trackedDirection += lastDirections["162"];
            count++;
        }
        if (trackingStatus["164"])
        {
            trackedPosition += lastPositions["164"];
            trackedDirection += lastDirections["164"];
            count++;
        }

        if (count > 0)
        {
            trackedPosition /= (float)(count * 1.0f);
            trackedDirection /= (float)(count * 1.0f);
        }
        
        //debugText.text = "" + "x:" + trackedPosition.x + " y:" + trackedPosition.y + " z:" + trackedPosition.z;

        // pose updates are applied once per second
        if (filterCount++ > 60) {            
            objectToTransform.transform.position = trackedPosition;
            //trackedDirection.Scale(scaleXZToZero);
            objectToTransform.transform.rotation = startRot * Quaternion.FromToRotation(new Vector3(0,0,1), trackedDirection) * Quaternion.Euler(0,-90,0);//startRot * Quaternion.FromToRotation(startForward, trackedDirection);
            filterCount = 0;
        }
    }        

    //updates poses if markers are currently recognized
    private void updatePoses()
    {        
        foreach (GameObject marker in markers)
        {         
            // the big marker
            if (marker.transform.parent.name.Contains("000") && trackingStatus["000"])
            {
                lastPositions["000"] = marker.transform.position;                
                lastDirections["000"] = marker.transform.forward;                
                validPose["000"] = true;                
            }            
            if (marker.transform.parent.name.Contains("128") && trackingStatus["128"])
            {                   
                lastPositions["128"] = marker.transform.position - fromM1ToPlate - fromPlateToMain;
                lastDirections["128"] = marker.transform.forward;                
                validPose["128"] = true;
            }
            if (marker.transform.parent.name.Contains("154") && trackingStatus["154"])
            {
                lastPositions["154"] = marker.transform.position - fromM2ToPlate - fromPlateToMain;
                lastDirections["154"] = marker.transform.forward;                
                validPose["154"] = true;
            }
            if (marker.transform.parent.name.Contains("162") && trackingStatus["162"])
            {
                lastPositions["162"] = marker.transform.position - fromM3ToPlate - fromPlateToMain;
                lastDirections["162"] = marker.transform.forward;                
                validPose["162"] = true;
            }
            if (marker.transform.parent.name.Contains("164") && trackingStatus["164"])
            {
                lastPositions["164"] = marker.transform.position - fromM4ToPlate - fromPlateToMain;
                lastDirections["164"] = marker.transform.forward;                
                validPose["164"] = true;
            }
            // xyz="0.2255 -0.025 0.032" mount plate            
        }
    }    

    public void SetStatus(string markerid, bool status)
    {
        trackingStatus[markerid] = status;
    }
    
}

