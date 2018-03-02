using UnityEngine;

public class WorldCursor : MonoBehaviour
{
    public HoloToolkit.Unity.HandsTrackingManager handsTrackingManager;
    private int mode = 0;
    private int numOfModes = 2;
    private GameObject lastObjectHit = null;
    public LineRenderer lineRenderer = null;
    public ros2unityManager rosManager = null;
    public Transform origin;
    //private Transform markerTransform;
    private Vector3 markerPosition;
    public ManageObjectSelection selectionManager = null;
    // private MeshRenderer meshRenderer;    

    // Use this for initialization
    void Start()
    {
        // Grab the mesh renderer that's on the same object as this script.
        //meshRenderer = this.gameObject.GetComponentInChildren<MeshRenderer>();
        markerPosition = new Vector3(0, 0, 0);
    }

    private void Update()
    {
        /*
        if (Input.GetKey(KeyCode.A)) 
        {
            this.mode = 0;
        }
        else if (Input.GetKey(KeyCode.B))
        {
            this.mode = 1;
        }
        */
    }

    // Update is called once per frame
    void LateUpdate()
    {
        
        // Do a raycast into the world based on the user's
        // head position and orientation.
        var headPosition = Camera.main.transform.position;
        var gazeDirection = Camera.main.transform.forward;


        RaycastHit hitInfo;

        if (mode == 1) //hand pointing based
        {
            if (Physics.Raycast(headPosition, handsTrackingManager.GetLastDirection(), out hitInfo))
            //if (Physics.Raycast(headPosition, gazeDirection, out hitInfo,
            //            30.0f, SpatialMapping.PhysicsRaycastMask))
            {
                /*
                // If the raycast hit a hologram...                
                if (lastObjectHit)
                {
                    lastObjectHit.GetComponent<Renderer>().material.color = Color.green;
                    hitInfo.transform.gameObject.GetComponent<Renderer>().material.color = Color.red;
                }
                */
                lastObjectHit = hitInfo.transform.gameObject;
                selectionManager.LastSelectedObject = selectionManager.CurrentSelectedObject;
                selectionManager.CurrentSelectedObject = hitInfo.transform.gameObject;
                // Move the cursor to the point where the raycast hit.
                this.transform.position = hitInfo.point;
                // Rotate the cursor to hug the surface of the hologram.
                this.transform.rotation = Quaternion.FromToRotation(Vector3.up, hitInfo.normal);
            }
            else
            {
                // If the raycast did not hit a hologram, place the cursor in neutral center position.
                this.transform.position = headPosition + gazeDirection * 1.0f;
                this.transform.LookAt(headPosition);
                this.transform.localRotation *= Quaternion.Euler(new Vector3(90, 0, 0));

                //if (lastObjectHit) lastObjectHit.GetComponent<Renderer>().material.color = Color.green;
                if(selectionManager.CurrentSelectedObject) selectionManager.LastSelectedObject = selectionManager.CurrentSelectedObject;
                selectionManager.CurrentSelectedObject = null;
            }
        }
        else if (mode == 0) //gaze based
        {
            if (Physics.Raycast(headPosition, gazeDirection, out hitInfo))
            {         
                /*      
                if (lastObjectHit)
                {
                    lastObjectHit.GetComponent<Renderer>().material.color = Color.green;
                    hitInfo.transform.gameObject.GetComponent<Renderer>().material.color = Color.red;
                }
                */
                
                lastObjectHit = hitInfo.transform.gameObject;
                selectionManager.LastSelectedObject = selectionManager.CurrentSelectedObject;

                if (hitInfo.transform.gameObject.name != "tabletop")
                {                    
                    selectionManager.CurrentSelectedObject = hitInfo.transform.gameObject;
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
        else if (mode == 2) //april tag based hand tracking (pointing direction)
        {
            //markerTransform.position = origin.transform.TransformPoint(this.rosManager.RosBridge.GetLatestHandPosition());
            //markerTransform.rotation = Quaternion.FromToRotation(markerTransform.forward, this.rosManager.RosBridge.GetLatestPointingDirection());//origin.transform.TransformDirection()
            //markerTransform
            //this.transform.position = markerPosition;
            //this.markerPosition = origin.transform.TransformPoint(this.rosManager.RosBridge.GetLatestHandPosition());            
            lineRenderer.SetPosition(0, this.markerPosition);
            lineRenderer.SetPosition(1, this.markerPosition + this.rosManager.RosBridge.GetLatestPointingDirection()); //1 * origin.transform.TransformPoint(this.rosManager.RosBridge.GetLatestPointingDirection()));
            
            
            if (Physics.Raycast(this.markerPosition, this.rosManager.RosBridge.GetLatestPointingDirection(), out hitInfo)) {        
                /*                        
                if (lastObjectHit)
                {
                    lastObjectHit.GetComponent<Renderer>().material.color = Color.green;
                    hitInfo.transform.gameObject.GetComponent<Renderer>().material.color = Color.red;
                }
                */
                lastObjectHit = hitInfo.transform.gameObject;
                selectionManager.LastSelectedObject = selectionManager.CurrentSelectedObject;
                selectionManager.CurrentSelectedObject = hitInfo.transform.gameObject;
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
