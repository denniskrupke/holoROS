using UnityEngine;

public class WorldCursor : MonoBehaviour
{
    public HoloToolkit.Unity.HandsTrackingManager handsTrackingManager;
    private int mode = 0;
    private int numOfModes = 2;
    //private GameObject lastObjectHit = null;
    public ros2unityManager rosManager = null;
    // public Transform origin;
    public ManageObjectSelection selectionManager = null;

    // handmarker-specific variables
    public LineRenderer lineRenderer = null;
    private Vector3 markerPosition;

    RaycastHit hitInfo;
    Vector3 headPosition;
    Vector3 gazeDirection;

    // Use this for initialization
    void Start()
    {        
        markerPosition = new Vector3(0, 0, 0);
    }

    private void Update()
    {        
    }

    // Update is called once per frame
    void LateUpdate()
    {        
        // Do a raycast into the world based on the user's
        // head position and orientation.
        headPosition = Camera.main.transform.position;
        gazeDirection = Camera.main.transform.forward;


        //if (mode == 1) //hand pointing based
        //{
        //    if (Physics.Raycast(headPosition, handsTrackingManager.GetLastDirection(), out hitInfo))
        //    //if (Physics.Raycast(headPosition, gazeDirection, out hitInfo,
        //    //            30.0f, SpatialMapping.PhysicsRaycastMask))
        //    {                
        //        // lastObjectHit = hitInfo.transform.gameObject;
        //        selectionManager.LastSelectedObject = selectionManager.CurrentSelectedObject;
        //        selectionManager.CurrentSelectedObject = hitInfo.transform.gameObject;
        //        // Move the cursor to the point where the raycast hit.
        //        this.transform.position = hitInfo.point;
        //        // Rotate the cursor to hug the surface of the hologram.
        //        this.transform.rotation = Quaternion.FromToRotation(Vector3.up, hitInfo.normal);
        //    }
        //    else
        //    {
        //        // If the raycast did not hit a hologram, place the cursor in neutral center position.
        //        this.transform.position = headPosition + gazeDirection * 1.0f;
        //        this.transform.LookAt(headPosition);
        //        this.transform.localRotation *= Quaternion.Euler(new Vector3(90, 0, 0));

        //        //if (lastObjectHit) lastObjectHit.GetComponent<Renderer>().material.color = Color.green;
        //        if(selectionManager.CurrentSelectedObject) selectionManager.LastSelectedObject = selectionManager.CurrentSelectedObject;
        //        selectionManager.CurrentSelectedObject = null;
        //    }
        //}

        //else 
        if (mode == 0) //gaze based
        {
            if (Physics.Raycast(headPosition, gazeDirection, out hitInfo))
            {                         
                // lastObjectHit = hitInfo.transform.gameObject;                
                if (hitInfo.transform.gameObject.name != "tabletop")
                {                  
                    selectionManager.LastSelectedObject = selectionManager.CurrentSelectedObject;  
                    selectionManager.CurrentSelectedObject = hitInfo.transform.parent.gameObject;
                }
                
                this.transform.position = hitInfo.point;// = hitInfo.point;
                this.transform.rotation = Quaternion.FromToRotation(Vector3.up, hitInfo.normal);                
            }
            else
            {
                // If the raycast did not hit a hologram, place the cursor in neutral center position.
                this.transform.position = headPosition + gazeDirection * 1.0f;
                this.transform.LookAt(headPosition);
                this.transform.localRotation *= Quaternion.Euler(new Vector3(90, 0, 0));

                //if (lastObjectHit) lastObjectHit.GetComponent<Renderer>().material.color = Color.green;
                if (selectionManager.CurrentSelectedObject) selectionManager.LastSelectedObject = selectionManager.CurrentSelectedObject;
                selectionManager.CurrentSelectedObject = null;
            }
        }

        //else if (mode == 2) //april tag based hand tracking (pointing direction)
        //{
        //    //markerTransform.position = origin.transform.TransformPoint(this.rosManager.RosBridge.GetLatestHandPosition());
        //    //markerTransform.rotation = Quaternion.FromToRotation(markerTransform.forward, this.rosManager.RosBridge.GetLatestPointingDirection());//origin.transform.TransformDirection()
        //    //markerTransform
        //    //this.transform.position = markerPosition;
        //    //this.markerPosition = origin.transform.TransformPoint(this.rosManager.RosBridge.GetLatestHandPosition());            
        //    lineRenderer.SetPosition(0, this.markerPosition);
        //    lineRenderer.SetPosition(1, this.markerPosition + this.rosManager.RosBridge.GetLatestPointingDirection()); //1 * origin.transform.TransformPoint(this.rosManager.RosBridge.GetLatestPointingDirection()));
            
        //    if (Physics.Raycast(this.markerPosition, this.rosManager.RosBridge.GetLatestPointingDirection(), out hitInfo)) {        
        //        /*                        
        //        if (lastObjectHit)
        //        {
        //            lastObjectHit.GetComponent<Renderer>().material.color = Color.green;
        //            hitInfo.transform.gameObject.GetComponent<Renderer>().material.color = Color.red;
        //        }
        //        */
        //        // lastObjectHit = hitInfo.transform.gameObject;
        //        selectionManager.LastSelectedObject = selectionManager.CurrentSelectedObject;
        //        selectionManager.CurrentSelectedObject = hitInfo.transform.gameObject;
        //        this.transform.position = hitInfo.point;// = hitInfo.point;
        //        this.transform.rotation = Quaternion.FromToRotation(Vector3.up, hitInfo.normal);
        //    }
        //    else
        //    {
        //        // If the raycast did not hit a hologram, place the cursor in neutral center position.
        //        this.transform.position = headPosition + gazeDirection * 1.0f;
        //        this.transform.LookAt(headPosition);
        //        this.transform.localRotation *= Quaternion.Euler(new Vector3(90, 0, 0));

        //        //if (lastObjectHit) lastObjectHit.GetComponent<Renderer>().material.color = Color.green;
        //        if (selectionManager.CurrentSelectedObject) selectionManager.LastSelectedObject = selectionManager.CurrentSelectedObject;
        //        selectionManager.CurrentSelectedObject = null;
        //    }   
        //}
   }

   public void ChangeMode()
    {
        this.mode = (this.mode + 1) % numOfModes;
    }

    public void SetSelectionMode(int mode)
    {
        this.mode = mode;
    }

}
