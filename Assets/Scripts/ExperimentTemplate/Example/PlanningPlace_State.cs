using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System;

public class Idle_State : ExperimentState
{    
    [SerializeField]
    Text text;

    [SerializeField]
    RosBridge_old.RosBridgeClient_old rosbridgeClient;

    [SerializeField]
    Collider tableTopCollider;

    bool next = false;

    public bool Next
    {
        get
        {
            return next;
        }

        set
        {
            next = value;
        }
    }

    public override ExperimentState HandleInput(ExperimentController ec)
    {
        if(rosbridgeClient.latestPlanningStatus == std_msgs.Int32_old.PLANNING_FAILED){
            if(typeof(this.previousState) == typeof(PlannedPlace_State)){
                nextStateIndex = 1; //PlannedPlace
            }
            else nextStateIndex = 0; //Picked
        }
        else if(rosbridgeClient.latestPlanningStatus == std_msgs.Int32_old.SUCCESS){
            nextStateIndex = 1; //PlannedPlace
        }
        next = true;

        if(Next)
        {
            return nextStates[nextStateIndex];
        }
        else
        {
            return this;
        }    
    }

    public override void UpdateState(ExperimentController ec)
    {       
        text.text = "planning place";
        tableTopCollider.SetActive(true);
    }
}
