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

        if(rosbridgeClient.LatestPlanningStatus.Count > 0){
            int status = rosbridgeClient.LatestPlanningStatus.Dequeue();
            if(status == RosMessages_old.std_msgs.Int32_old.PLANNING_FAILED){
                // TODO show fail message here
                if(ec.PreviousState.GetType() == typeof(PlannedPlace_State)){
                    nextStateIndex = 1; //PlannedPlace
                    next = true;
                }
                else {
                    nextStateIndex = 0; //Picked
                    next = true;
                }
            }
            else if(status == RosMessages_old.std_msgs.Int32_old.SUCCESS){
                // TODO show success message here
            }
            else if (status == RosMessages_old.std_msgs.Int32_old.PLANNED_PLACE){
                nextStateIndex = 1; //PlannedPlace
                next = true;
            }
        }

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
