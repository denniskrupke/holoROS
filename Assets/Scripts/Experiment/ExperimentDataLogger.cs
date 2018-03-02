using UnityEngine;
using UnityEngine.UI;

public class ExperimentDataFrame{
	public byte id_participant;
	public byte method; // 0=finger-ray / 1=gaze-ray
	public byte trial; // [0...71]
	public float currentAngle; //[0 60 120] each two times for each cylinder
	public byte cylinderIndex; //[0...5]
	public long timeStamp_start; //when the target appears in system time expressed in milliseconds
	public long timeStamp_stop; //when the goal position was selected by the user in system time expressed in milliseconds
	public Vector3 position_cylinder; //position of the current cylinder in world coordinates
	public Vector3 position_target; //position of the current target in world coordinated
	public Vector3 position_user; //position of the user when the goal position was selected
	public Quaternion rotation_user; //rotation of the user when the goal position was selected
	public float distance_euklid_user2cylinder_before; //how far is the user away from the cylinder in meter when spawning
	public float distance_euklid_user2cylinder_after; //how far is the user away from the cylinder in meter at selection time
	public float distance_euklid_target2selection; //like the error of the selection in meter
	public bool error; //true, if the wrong cylinder / or the table were selected
    public float time;
    

/*	create your own constructor if you like ;-)
	public ExperimentDataFrame(Vector3 pos, Quaternion rot){
		UpdateCurrentTimestampInMilliseconds ();
		position = pos;
		rotation = rot;
	}
	*/

	private long CalculateCurrentTimeStamp(){
		return System.DateTime.Now.Millisecond + System.DateTime.Now.Second*1000 + System.DateTime.Now.Minute*60*1000 + System.DateTime.Now.Hour*60*60*1000;
	}

	public void UpdateStartTime(){
		timeStamp_start = CalculateCurrentTimeStamp();
	}

	public void UpdateStopTime(){
		timeStamp_stop = CalculateCurrentTimeStamp();	
	}
}
		
/*! \brief can be attached to an object to capture position and rotation in world space
 * 
 */
public class ExperimentDataLogger : MonoBehaviour {
    public ExperimentFileWriter experimentFileWriter = null;
    public ParticipantTargetPositioner participantTargetPositioner = null;
    public Randomizer randomizer = null;
    public Text debugHUD = null;
    public Transform tcp = null;
    public SetTransform goalPoseSetter = null;
    public Transform greenTarget = null;
    public WorldCursor cursor = null;

	private ExperimentDataFrame experimentData;
    private bool waitForCompletion = false;
    private bool firstTime = true;

    public bool fakeROSExecution = true;

	// Use this for initialization
	void Start () {
		experimentData = new ExperimentDataFrame();
	}
	
	// Update is called once per frame
	void Update () {        
        if (this.waitForCompletion) {           
            if (Vector3.Distance(greenTarget.position, this.tcp.position) < 0.01)
            {
                this.waitForCompletion = false;
                participantTargetPositioner.PrepareNextTarget();
            }
        }
    }

	public ExperimentDataFrame GetExperimentData(){
		return experimentData;
	}

	public void StartTrial(){
        // TODO apply actual values
        experimentData.UpdateStartTime();
        if (this.firstTime)
        {
            experimentData.id_participant = (byte)(experimentFileWriter.RetieveLastParticipantID());
            if ((experimentData.id_participant) % 4 == 0)
            { // second method even
                randomizer.methods[0] = 1;
                this.cursor.SetSelectionMode(1);
                experimentData.id_participant += 1;
            }
            else if ((experimentData.id_participant) % 4 == 1)
            { //next participant odd
                randomizer.methods[0] = 1;
                this.cursor.SetSelectionMode(1);
                experimentData.id_participant += 1;
            }
            else if ((experimentData.id_participant) % 4 == 2) //second method odd
            {
                randomizer.methods[0] = 0;
                this.cursor.SetSelectionMode(0);
                experimentData.id_participant += 1;
            }
            else
            {  //first method even
                randomizer.methods[0] = 0;
                this.cursor.SetSelectionMode(0);
                experimentData.id_participant += 1;
            }
            firstTime = false;
        }
             
        Trial currentTrial = participantTargetPositioner.GetCurrentTrial();
        experimentData.method = (byte)randomizer.methods[0];// currentTrial.method; // 0=finger-ray / 1=gaze-ray
		experimentData.trial = (byte) participantTargetPositioner.GetTrialCount(); // [0...71]
		experimentData.currentAngle = currentTrial.angle; //[0 60 120] each two times for each cylinder
		experimentData.cylinderIndex = (byte) currentTrial.cylinder; //[0...5]        
        //experimentData.timeStamp_start = CalculateCurrentTimeStamp(); //when the target appears in system time expressed in milliseconds

        experimentData.position_cylinder = participantTargetPositioner.targetObjects[experimentData.cylinderIndex].transform.position; //position of the current cylinder in world coordinates
		experimentData.position_target = participantTargetPositioner.targetMarker.transform.position; //position of the current target in world coordinated
		experimentData.position_user = participantTargetPositioner.participantPose.position; //position of the user when the goal position was selected
		experimentData.rotation_user = participantTargetPositioner.participantPose.rotation; //rotation of the user when the goal position was selected
		experimentData.distance_euklid_user2cylinder_before = Vector3.Distance(experimentData.position_cylinder, experimentData.position_user); //how far is the user away from the cylinder in meter at selection time				        
    }

	public void StopTrial(){
        //debugHUD.text = "\n " + "stop trial" + debugHUD.text;
        experimentData.UpdateStopTime();                

        experimentData.distance_euklid_target2selection = participantTargetPositioner.errorDistance;// Vector3.Distance(experimentData.position_target, cursor.transform.position); //like the error of the selection in meter	
		experimentData.distance_euklid_user2cylinder_after = Vector3.Distance(participantTargetPositioner.participantPose.position, experimentData.position_cylinder);//how far is the user away from the cylinder in meter at selection time 
        
        // THINK ABOUT VEC2 EUKLIDEAN        
        Vector2 target, selection;
        target.x = participantTargetPositioner.targetObjects[experimentData.cylinderIndex].transform.position.x;
        target.y = participantTargetPositioner.targetObjects[experimentData.cylinderIndex].transform.position.z;
        selection.x = cursor.transform.position.x;
        selection.y = cursor.transform.position.z;

        experimentData.error = Vector2.Distance(target, selection) > 0.04f; //true, if the wrong cylinder / or the table were selected	-> distance to center of cylinder (x,z) to selection (x,z) larger than radius of cylinder plus mu
        experimentData.time = (experimentData.timeStamp_stop - experimentData.timeStamp_start)/1000.0f;//time in seconds

        //write data
        experimentFileWriter.AppendLineToFile(experimentFileWriter.ExperimentDataFrame2String(experimentData));        

        // only for development without the robot
        if (!fakeROSExecution)  this.waitForCompletion = true;        
        else participantTargetPositioner.PrepareNextTarget();
    }

}
