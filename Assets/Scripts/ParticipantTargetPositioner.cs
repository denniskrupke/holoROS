using System;
using UnityEngine;
using UnityEngine.UI;

public class ParticipantTargetPositioner : MonoBehaviour {
    private Vector2 cPos, pPos, tPos;
    private int currentObjectIndex = 0;
    //private int currentObjectIndexCount = 0;
    private float h; //length from cPos to Tpos
    private float b; //length from pPos to Tpos (hypothenuse)  

    public Transform participantPose; //headset pose    
    public GameObject[] targetObjects;
    public GameObject targetMarker;

    public float minHeight;
    public float maxHeight;
    private float userMotivationAngle = 0.0f;//120.0f;

    public Text text;
    public AudioClip clip;

    public ExperimentFileWriter experimentFileWriter = null;
    public Randomizer randomizer = null;
    private Trial currentTrial;
    private int trialCount = -1;

    public ExperimentDataLogger experimentDataLogger = null;
    public WorldCursor worldCursor = null;
    public SetTransform armControl = null;
    public GameObject debugHUD = null;

    public Transform greenTarget = null;
    public Transform tcp = null;

    private bool waitForCompletion = false;

    //int countFrame = 0;

    public float errorDistance = 0.0f;
    public bool fakeROSExecution = true;

    private AudioSource audioSource;


    // Use this for initialization
    void Start () {
        audioSource = gameObject.GetComponent<AudioSource>(); //TODO audio not working
        SetDemoTargetPosition();
    }

    // Update is called once per frame
    void Update()
    {
        if (waitForCompletion && !fakeROSExecution)
        {
            if (Vector3.Distance(greenTarget.position, tcp.position) < 0.01)
            {
                //this.text.text = "BEdingung erfüllt";
                waitForCompletion = false;
                this.debugHUD.SetActive(false);
                PrepareNextTarget();
            }
        }
    }

    private void FixedUpdate()
    {
        errorDistance = Vector3.Distance(worldCursor.transform.position, targetMarker.transform.position); // TODO: check if this hack works
        /*
        if (countFrame++ >= 10)
        {
            this.text.text = "distance: " + Vector3.Distance(worldCursor.transform.position, targetMarker.transform.position) +
                "\t cursor: " + worldCursor.transform.position.x + "\t" + worldCursor.transform.position.y + "\t" + worldCursor.transform.position.y + "\t" +
                " target: " + targetMarker.transform.position.x + "\t" + targetMarker.transform.position.y + "\t" + targetMarker.transform.position.y;
               // text.text;
            countFrame = 0;
        }
        */
    }

    public int GetTrialCount() { return this.trialCount; }
    public Trial GetCurrentTrial() { return this.currentTrial; }

    public int CurrentObjectIndex
    {
        get
        {
            return currentObjectIndex;
        }

        set
        {
            currentObjectIndex = value;
        }
    }

    //private byte[] cylindersIndex = {4, 0, 3, 2, 1,
    //                            2, 3, 0, 4, 1,
    //                            4, 3, 0, 1, 2};

    //private bool done = false;




    //calculates angle between user and targetCylinder relative to cylinder's zero angle
    private float CalculateCurrentAngleFromUserToTarget() {
        float angle = 0.0f;

        h = Vector2.Distance(cPos, tPos);
        b = Vector2.Distance(pPos, tPos);
        angle = (float) Math.Asin(h / b);

        float halfPI = (float)Math.PI / 2.0f;

        if ((pPos.x-tPos.x) >= 0.0f) { // right 
            if ((pPos.y - tPos.y) >= 0.0f) {// upper right                                
                angle = -halfPI + (halfPI - angle);
            }
            else {//lower right                
            }
        }
        else { // left
            if ((pPos.y - tPos.y) >= 0.0f) {// upper left
                angle = halfPI + angle + halfPI;               
            }
            else {//lower left
                angle = -angle + (float)Math.PI ;                
            }
        }        

        return (float) (angle*180.0f/Math.PI);
    }



    void OnNext()
    {
        //text.text = "\n " + "try next target" + text.text;
               
        if(!fakeROSExecution) waitForCompletion = true;   //WaitForReset
        if (fakeROSExecution) PrepareNextTarget();        
    }
   
    
    public void PrepareNextTarget() {        
        if (!randomizer.IsDone())
        {            
            //text.text = "\n " + "randomizer is not done" + text.text;
            this.currentTrial = randomizer.NextTrial();
            trialCount++;
            //this.worldCursor.SetSelectionMode(this.currentTrial.method);
            //text.text = "\n " + "received random trial" + text.text;
            experimentDataLogger.StartTrial();
            //text.text = "\n " + "started logger" + text.text;

            currentObjectIndex = currentTrial.cylinder;
            userMotivationAngle = currentTrial.angle;
            SetNextTargetPosition();

            if (audioSource != null)
            {
                audioSource.clip = clip;
                audioSource.Play();
            }
        }
        else {
            text.text = "\n " + "randomizer is done" + text.text;
            this.armControl.ResetArm();
            this.text.text = "FINISHED";
            this.debugHUD.SetActive(true);
            
            //Application.Quit();
            /*
            if (!randomizer.secondRoundRunning) {
                text.text = "\n " + "2nd round" + text.text;
                randomizer.NextMethod();
                PrepareNextTarget();
            }
            else { 
                text.text = "\n " + "FINISHED" + text.text;
                // TODO EXIT PROGRAM
            }
            */
        }           
    }

    public void SetDemoTargetPosition()
    {
        this.currentObjectIndex = UnityEngine.Random.Range(0, 4);
        SetCurrentPositions();
        float height = targetObjects[currentObjectIndex].transform.position.y + UnityEngine.Random.Range(this.minHeight, this.maxHeight);
        targetMarker.transform.position = new Vector3(tPos.x + targetObjects[currentObjectIndex].transform.localScale.x / 2.0f,
                                                        height,
                                                        tPos.y);
        float angle = 0.0f;
        float userAngle = CalculateCurrentAngleFromUserToTarget();
        angle = (angle > 0) ? -this.userMotivationAngle : this.userMotivationAngle;
        targetMarker.transform.RotateAround(targetObjects[currentObjectIndex].transform.position, new Vector3(0, 1, 0), angle + userAngle);
    }

    public void SetNextTargetPosition() {
        //text.text = "\n " + "update positions" + text.text;
        SetCurrentPositions(); //updates head and object positions and calculates c

        float height = targetObjects[currentObjectIndex].transform.position.y + UnityEngine.Random.Range(this.minHeight, this.maxHeight);

        targetMarker.transform.position = new Vector3(tPos.x + targetObjects[currentObjectIndex].transform.localScale.x / 2.0f,
                                                        height,
                                                        tPos.y);
        //this.userMotivationAngle = 30.0f;
        float angle = 0.0f;
        float userAngle = CalculateCurrentAngleFromUserToTarget();
        angle = (angle > 0) ? -this.userMotivationAngle : this.userMotivationAngle;
        //text.text = "\n " + "count:" + trialCount++ + " method:" + currentTrial.method + " cylinder:" + currentTrial.cylinder + " angle:" + angle + text.text;

        targetMarker.transform.RotateAround(targetObjects[currentObjectIndex].transform.position, new Vector3(0, 1, 0), angle + userAngle);
    }


    // updates targetCylinder and userPose and determines 3rd position in the right triangle
    private void SetCurrentPositions() {
        pPos.x = participantPose.position.x;
        pPos.y = participantPose.position.z;

        tPos.x = targetObjects[currentObjectIndex].transform.position.x;
        tPos.y = targetObjects[currentObjectIndex].transform.position.z;
        //text.text = "\n " + "set target to x:" + tPos.x + " y:" + tPos.y + text.text;

        cPos.x = targetObjects[currentObjectIndex].transform.position.x;
        cPos.y = participantPose.position.z;
    }    

}