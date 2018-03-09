using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System;

public class PlanningPick_State : ExperimentState
{    
    [SerializeField]
    Text text;

    [SerializeField]
    ros2unityManager rum;    

    [SerializeField]
    Collider tableTopCollider;

    [SerializeField]
    FadingNotification fn;

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
        if (rum.RosBridge.latestPlanningStatus == null) {            
            return this;
        }        

        if (rum.RosBridge.latestPlanningStatus.Count > 0){         
            int status = rum.RosBridge.latestPlanningStatus.Dequeue();
            if(status == RosMessages_old.std_msgs.Int32_old.PLANNING_FAILED){
                // TODO show fail message here
                fn.ShowMessage("FAIL", new Color(1, 0, 0));                
                if (ec.PreviousState.GetType() == typeof(PlannedPick_State)){
                    nextStateIndex = 1; //PlannedPick
                    next = true;                    
                }
                else {
                    nextStateIndex = 0; //Idle
                    next = true;                    
                }
            }
            else if(status == RosMessages_old.std_msgs.Int32_old.SUCCESS){
                // TODO show success message here                
                fn.ShowMessage("SUCCESS", new Color(0, 1, 0));
            }
            else if (status == RosMessages_old.std_msgs.Int32_old.PLANNED_PICK){                
                nextStateIndex = 1; //PlannedPick
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

/*
        if(rosbridgeClient.LatestPlanningStatus == RosMessages_old.std_msgs.Int32_old.PLANNING_FAILED){            
            if (ec.PreviousState.GetType() == typeof(PlannedPick_State)){                
                return nextStates[1]; //PlannedPick
            }
            else return nextStates[0]; //Idle
        }
        else if(rosbridgeClient.LatestPlanningStatus == RosMessages_old.std_msgs.Int32_old.SUCCESS){
            return nextStates[1]; //PlannedPick
        }
        else return this;        
        */
    }

    public override void UpdateState(ExperimentController ec)
    {        
        text.text = "planning pick";
        tableTopCollider.enabled = false;        
    }
}
