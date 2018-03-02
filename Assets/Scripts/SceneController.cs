using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class SceneController : MonoBehaviour {

    public List<GameObject> enableList;
    public List<GameObject> disableList;
    public Text debugHUD = null;
    public WorldCursor cursor = null;
    public GameObject hud = null;
    public actuateGripper gripperControl = null;

    // Use this for initialization
    //TODO
    void Start () {
        UpdateSceneObjects();
        gripperControl.MoveGripper(Mathf.Lerp(0.0f, 0.3f, Time.deltaTime * 0.025f));
    }
	
	// Update is called once per frame
    // TODO
	void Update () {
        if (RobotAR.StateMachine.Changed())
        {
            UpdateSceneObjects();
        }
	}


    void UpdateSceneObjects()
    {
        switch (RobotAR.StateMachine.GetCurrentState())
        {
            case RobotAR.State.Pick:
                {
                    foreach (GameObject go in disableList)
                    {
                        go.SetActive(false);
                    }
                }
                break;
            case RobotAR.State.Place:
                {
                    foreach (GameObject go in disableList)
                    {
                        go.SetActive(true);
                    }
                }
                break;
        }
        RobotAR.StateMachine.Reset();
    }


    void OnChangeMode()
    {
        //debugHUD.text = "\n Try to go to next state..." + debugHUD.text;
        RobotAR.StateMachine.NextState();
    }

    void OnChangeMethod()
    {
        cursor.ChangeMode();
    }

    void OnDebug()
    {
        hud.SetActive(!hud.activeSelf);
    }

    void OnGripperOpen()
    {
        gripperControl.MoveGripper(Mathf.Lerp(0.0f, 0.3f, Time.deltaTime * 0.025f));
    }

    void OnGripperClose()
    {
        gripperControl.MoveGripper(Mathf.Lerp(0.3f, 0.0f, Time.deltaTime *0.025f));

        //void OnChangeComputer 
    }
}
