using UnityEngine;
using UnityEngine.UI;


/*
 * This class takes a transform from a e.g. tracked/recognized object and based on this it calculates the new pose of the gameobject it is attached to.
 */
public class MarkerPoseDetectionAdapter : MonoBehaviour {
    public Transform markerAnker = null;
    public Text text = null;
    public ConfigurationManager configurationManager = null;     
       
    private int count = 0;
    Quaternion startRot;
    Quaternion startRotMarker;
    Vector3 startMarkerForward;    
    Vector3 scaleYToZero = new Vector3(1, 0, 1);

    [SerializeField]
    ManageObjectSelection mos;

    // Use this for initialization
    void Start () {
        startRot = transform.rotation;
        startRotMarker = markerAnker.rotation;

        startMarkerForward = markerAnker.forward;
        //text.text = "\n x:" + startMarkerForward.x + "y:" + startMarkerForward.y + "z:" + startMarkerForward.z + text.text;

        startMarkerForward.Scale(scaleYToZero);    
    }

    // Update is called once per frame
    void Update () {
        if (configurationManager.registration_calibration)
        {
            this.transform.position = markerAnker.position;
            //this.transform.rotation = markerAnker.rotation;                        

            Vector3 newMarkerForward = markerAnker.forward;
            newMarkerForward.Scale(scaleYToZero);

            this.transform.rotation = startRot * Quaternion.FromToRotation(startMarkerForward, newMarkerForward);
            if (count++ > 200)
            {
                configurationManager.registration_calibration = false;
                count = 0;
                mos.RememberPositions();
            }
        }

        /*
        if (count++ > 100)
        {
            //text.text = "\n x:" + markerAnker.up.x + "y:" + markerAnker.up.y + "z:" + markerAnker.up.z + ";\n x:" + euler.x + "y:" + euler.y + "z:" + euler.z + text.text;
            //text.text = "\n x:" + newMarkerForward.x + "y:" + newMarkerForward.y + "z:" + newMarkerForward.z + ";\n x:" + startMarkerForward.x + "y:" + startMarkerForward.y + "z:" + startMarkerForward.z + text.text;
            count = 0;
        }
       */
    }
 
}
