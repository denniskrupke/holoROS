using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MarkerManager : MonoBehaviour {

    //The Vuforia markers
    [SerializeField]
    List<GameObject> markers;    

    //The master marker used as an anker for the world
    [SerializeField]
    GameObject objectToTransform;//this is the MainAprilTag

    //The center position of the mounting plate of the robot (the following markers are attached to the corners of this plate)
    [SerializeField]
    GameObject mountPlate;

    //----------- The markers in the world --------------
    [SerializeField]
    GameObject marker1;

    [SerializeField]
    GameObject marker2;

    [SerializeField]
    GameObject marker3;

    [SerializeField]
    GameObject marker4;
    //---------------------------------------------------

    [SerializeField]
    Text debugText; //this can be used to print some debug information as part of the HUD

    Vector3 trackedPosition, trackedForwardDirection, trackedUpDirection;    
    Quaternion inverseStartRot, inverseStartRot1, inverseStartRot2, inverseStartRot3, inverseStartRot4;    

    Dictionary<string, Vector3> lastPositions;  //positions of the detected markers
    Dictionary<string, Vector3> lastForwardDirections; //forward directions of the detected markers
    Dictionary<string, Vector3> lastUpDirections; //up directions of the detected markers
    Dictionary<string, bool> trackingStatus;
    Dictionary<string, bool> validPose;
    
    // we don't want to allign the world on every frame, once per second seems enough
    int filterCount = 0;
    [SerializeField] int filterCountMax; //60 works well

    Vector3 fromPlateToMain;
    Vector3 fromM1ToPlate;
    Vector3 fromM2ToPlate;
    Vector3 fromM3ToPlate;
    Vector3 fromM4ToPlate;


    void Start()
    {
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
        //startRot = objectToTransform.transform.rotation;
        inverseStartRot = Quaternion.Inverse(markers[0].transform.rotation);//Quaternion.Inverse(objectToTransform.transform.rotation);        
        inverseStartRot1 = Quaternion.Inverse(markers[1].transform.rotation);
        inverseStartRot2 = Quaternion.Inverse(markers[2].transform.rotation);
        inverseStartRot3 = Quaternion.Inverse(markers[3].transform.rotation);
        inverseStartRot4 = Quaternion.Inverse(markers[4].transform.rotation);

        
        //fixed translations between frames, known by static scene geometry
        fromPlateToMain = objectToTransform.transform.InverseTransformPoint(mountPlate.transform.position);
        fromM1ToPlate = mountPlate.transform.InverseTransformPoint(marker1.transform.position);
        fromM2ToPlate = mountPlate.transform.InverseTransformPoint(marker2.transform.position);
        fromM3ToPlate = mountPlate.transform.InverseTransformPoint(marker3.transform.position);
        fromM4ToPlate = mountPlate.transform.InverseTransformPoint(marker4.transform.position);
    }


    //updates poses if markers are currently recognized
    private void updatePoses()
    {
        foreach (GameObject currentMarker in markers)
        {
            // the big marker
            if (currentMarker.transform.parent.name.Contains("000") && trackingStatus["000"])
            {
                lastPositions["000"] = currentMarker.transform.position;
                lastForwardDirections["000"] = inverseStartRot * currentMarker.transform.forward;
                lastUpDirections["000"] = inverseStartRot * currentMarker.transform.up;
                validPose["000"] = true;
            }
            if (currentMarker.transform.parent.name.Contains("128") && trackingStatus["128"])
            {
                lastPositions["128"] = currentMarker.transform.position - fromM1ToPlate - fromPlateToMain;
                lastForwardDirections["128"] = inverseStartRot1 * currentMarker.transform.forward;// - fromM1ToPlateForward - fromPlateToMainForward;
                lastUpDirections["128"] = inverseStartRot1 * currentMarker.transform.up;// - fromM1ToPlateUp - fromPlateToMainUp;
                validPose["128"] = true;
            }
            if (currentMarker.transform.parent.name.Contains("154") && trackingStatus["154"])
            {
                lastPositions["154"] = currentMarker.transform.position - fromM2ToPlate - fromPlateToMain;
                lastForwardDirections["154"] = inverseStartRot2 * currentMarker.transform.forward;// - fromM1ToPlateForward - fromPlateToMainForward;
                lastUpDirections["154"] = inverseStartRot2 * currentMarker.transform.up;// - fromM1ToPlateUp - fromPlateToMainUp;
                validPose["154"] = true;
            }
            if (currentMarker.transform.parent.name.Contains("162") && trackingStatus["162"])
            {
                lastPositions["162"] = currentMarker.transform.position - fromM3ToPlate - fromPlateToMain;
                lastForwardDirections["162"] = inverseStartRot3 * currentMarker.transform.forward;// - fromM1ToPlateForward - fromPlateToMainForward;
                lastUpDirections["162"] = inverseStartRot3 * currentMarker.transform.up;// - fromM1ToPlateUp - fromPlateToMainUp;
                validPose["162"] = true;
            }
            if (currentMarker.transform.parent.name.Contains("164") && trackingStatus["164"])
            {
                lastPositions["164"] = currentMarker.transform.position - fromM4ToPlate - fromPlateToMain;
                lastForwardDirections["164"] = inverseStartRot4 * currentMarker.transform.forward;// - fromM1ToPlateForward - fromPlateToMainForward;
                lastUpDirections["164"] = inverseStartRot4 * currentMarker.transform.up;
                validPose["164"] = true;
            }
            // xyz="0.2255 -0.025 0.032" mount plate            
        }
    }

    
	
	// updates the lab model frame pose according to the last marker detections by calculating mean rotation and translation
	void Update () {
        updatePoses();        

        int count = 0;        
        trackedPosition = new Vector3(); //position for calculation of the current average
        trackedForwardDirection = new Vector3();//forward direction for calculation of the current average
        trackedUpDirection = new Vector3();//up direction for calculation of the current average

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
        if (filterCount++ > filterCountMax) {            
            objectToTransform.transform.position = trackedPosition;
            objectToTransform.transform.rotation = Quaternion.LookRotation(trackedForwardDirection.normalized, trackedUpDirection.normalized);
            filterCount = 0;
        }
    }        

    

    public void SetStatus(string markerid, bool status)
    {
        trackingStatus[markerid] = status;
    }
    
}

