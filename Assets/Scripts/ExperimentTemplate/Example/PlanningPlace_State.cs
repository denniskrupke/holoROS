using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System;

public class PlanningPlace_State : ExperimentState
{    
    [SerializeField]
    Text text;

    [SerializeField]
    RosBridge_old.RosBridgeClient_old rosbridgeClient;

    [SerializeField]
    Collider tableTopCollider;

    bool next = false;

    public override bool GetNext(){
        return next;
    }
    
    public override void SetNext(bool val)
    {        
        next = val;
    }

    public override ExperimentState HandleInput(ExperimentController ec)
    {
        if(rosbridgeClient.LatestPlanningStatus == RosMessages_old.std_msgs.Int32_old.PLANNING_FAILED){
            if(ec.PreviousState.GetType() == typeof(PlannedPlace_State)){
                nextStateIndex = 1; //PlannedPlace
            }
            else nextStateIndex = 0; //Picked
        }
        else if(rosbridgeClient.LatestPlanningStatus == RosMessages_old.std_msgs.Int32_old.SUCCESS){
            nextStateIndex = 1; //PlannedPlace
        }
        next = true;

        if(next)
        {
            next = false;
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
        tableTopCollider.enabled = true;        
    }
}
