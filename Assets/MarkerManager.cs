using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MarkerManager : MonoBehaviour {

    [SerializeField]
    private List<GameObject> markers;    

    [SerializeField]
    private Vector3 trackedPosition;

    [SerializeField]
    private Vector3 trackedDirection;

    [SerializeField]
    private GameObject objectToTransform;

    private Quaternion startRot;
    private Vector3 startForward;

    private Dictionary<string, Vector3> lastPositions;
    private Dictionary<string, Vector3> lastDirections;
    private Dictionary<string, bool> trackingStatus;
    //private int countDetectedMarkers = 0;

    private bool firstPos = true;
    private bool firstRot = true;

    public Text text;

    private int filter = 0;    

    void Start () {
        lastPositions = new Dictionary<string, Vector3>();
        lastPositions.Add("000", new Vector3());
        lastPositions.Add("128", new Vector3());
        lastPositions.Add("154", new Vector3());
        lastPositions.Add("162", new Vector3());
        lastPositions.Add("164", new Vector3());

        lastDirections = new Dictionary<string, Vector3>();
        lastDirections.Add("000", new Vector3());
        lastDirections.Add("128", new Vector3());
        lastDirections.Add("154", new Vector3());
        lastDirections.Add("162", new Vector3());
        lastDirections.Add("164", new Vector3());

        trackingStatus = new Dictionary<string, bool>();
        trackingStatus.Add("000", false);
        trackingStatus.Add("128", false);
        trackingStatus.Add("154", false);
        trackingStatus.Add("162", false);
        trackingStatus.Add("164", false);

        startRot = objectToTransform.transform.rotation;
        startForward = objectToTransform.transform.forward;
    }
	
	// Update is called once per frame
	void Update () {
        updatePositions();
        updateDirections();
       
        trackedPosition = (lastPositions["000"] +
                    lastPositions["128"] +
                    lastPositions["154"] +
                    lastPositions["162"] +
                    lastPositions["164"]) / (float)(5 * 1.0f);

        trackedDirection = (lastDirections["000"] +
                    lastDirections["128"] +
                    lastDirections["154"] +
                    lastDirections["162"] +
                    lastDirections["164"]) / (float)(5 * 1.0f);

        //text.text = "" + "x:" + trackedDirection.x + " y:" + trackedDirection.y + " z:" + trackedDirection.z;

        if (filter++ > 60) { 
            objectToTransform.transform.position = trackedPosition;            
            objectToTransform.transform.rotation = startRot * Quaternion.FromToRotation(new Vector3(0,0,1), trackedDirection) * Quaternion.Euler(0,-90,0);//startRot * Quaternion.FromToRotation(startForward, trackedDirection);
            filter = 0;
        }
    }

    // deprecated
    private int countValidPositions()
    {
        int c = 0;

        if (!(Mathf.Approximately(lastPositions["000"].x, 0.0f)
            && Mathf.Approximately(lastPositions["000"].y, 0.0f)
            && Mathf.Approximately(lastPositions["000"].z, 0.0f)))
            c++;
        if (!(Mathf.Approximately(lastPositions["128"].x, 0.0f)
            && Mathf.Approximately(lastPositions["128"].y, 0.0f)
            && Mathf.Approximately(lastPositions["128"].z, 0.0f)))
            c++;
        if (!(Mathf.Approximately(lastPositions["154"].x, 0.0f)
            && Mathf.Approximately(lastPositions["154"].y, 0.0f)
            && Mathf.Approximately(lastPositions["154"].z, 0.0f)))
            c++;
        if (!(Mathf.Approximately(lastPositions["162"].x, 0.0f)
            && Mathf.Approximately(lastPositions["162"].y, 0.0f)
            && Mathf.Approximately(lastPositions["162"].z, 0.0f)))
            c++;
        if (!(Mathf.Approximately(lastPositions["162"].x, 0.0f)
            && Mathf.Approximately(lastPositions["162"].y, 0.0f)
            && Mathf.Approximately(lastPositions["162"].z, 0.0f)))
            c++;

        return c;
    }

    private void updatePositions()
    {
        foreach (GameObject marker in markers)
        {         
            if (marker.transform.parent.name.Contains("000") && trackingStatus["000"])
            {
                lastPositions["000"] = marker.transform.position;
                if (firstPos)
                {
                    lastPositions["128"] = marker.transform.position;
                    lastPositions["154"] = marker.transform.position;
                    lastPositions["162"] = marker.transform.position;
                    lastPositions["164"] = marker.transform.position;
                    firstPos = false;
                }
            }
            else if (marker.transform.parent.name.Contains("128") && trackingStatus["128"])
            {
                lastPositions["128"] = marker.transform.position - new Vector3(.2f,-.15f,0);
            }
            else if (marker.transform.parent.name.Contains("154") && trackingStatus["154"])
            {
                lastPositions["154"] = marker.transform.position - new Vector3(-.2f, .15f, 0);
            }
            else if (marker.transform.parent.name.Contains("162") && trackingStatus["162"])
            {
                lastPositions["162"] = marker.transform.position - new Vector3(.2f, -.75f, 0);
            }
            else if (marker.transform.parent.name.Contains("164") && trackingStatus["164"])
            {
                lastPositions["164"] = marker.transform.position - new Vector3(-.2f, -.75f, 0);
            }
        }
    }

    private void updateDirections()
    {        
        foreach (GameObject marker in markers)
        {
            if (!marker.activeSelf) continue;

            if (marker.transform.parent.name.Contains("000") && trackingStatus["000"])
            {
                lastDirections["000"] = marker.transform.forward;
                if (firstRot)
                {
                    lastDirections["128"] = marker.transform.forward;
                    lastDirections["154"] = marker.transform.forward;
                    lastDirections["162"] = marker.transform.forward;
                    lastDirections["164"] = marker.transform.forward;
                    firstRot = false;
                }
            }
            else if (marker.transform.parent.name.Contains("128") && trackingStatus["128"])
            {
                lastDirections["128"] = marker.transform.forward;
            }
            else if (marker.transform.parent.name.Contains("154") && trackingStatus["154"])
            {
                lastDirections["154"] = marker.transform.forward;
            }
            else if (marker.transform.parent.name.Contains("162") && trackingStatus["162"])
            {
                lastDirections["162"] = marker.transform.forward;
            }
            else if (marker.transform.parent.name.Contains("164") && trackingStatus["164"])
            {
                lastDirections["164"] = marker.transform.forward;
            }
        }
    }

    public void SetStatus(string markerid, bool status)
    {
        trackingStatus[markerid] = status;
    }

    /*
    private void LateUpdate()
    {
              
    }
    */
}

