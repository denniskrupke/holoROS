using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System;

public class PlanningPick_State : ExperimentState
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
        if(rosbridgeClient.LatestPlanningStatus == RosMessages_old.std_msgs.Int32_old.PLANNING_FAILED){            
            if (ec.PreviousState.GetType() == typeof(PlannedPick_State)){
                nextStateIndex = 1; //PlannedPick
            }
            else nextStateIndex = 0; //Idle
        }
        else if(rosbridgeClient.LatestPlanningStatus == RosMessages_old.std_msgs.Int32_old.SUCCESS){
            nextStateIndex = 1; //PlannedPick
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
        text.text = "planning pick";
        tableTopCollider.enabled = false;
        if (triggerNextState)
        {
            next = true;
            triggerNextState = false;
        }
    }
}
