using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System;

public class PlanningPlace_State : ExperimentState
{    
    [SerializeField]
    Text text;

    [SerializeField]
    ros2unityManager rum;
    //RosBridge_old.RosBridgeClient_old rosbridgeClient;

    [SerializeField]
    Collider tableTopCollider;

    [SerializeField]
    FadingNotification fn;

    bool next = false;

    string ms = "planning place";

    public override bool GetNext(){
        return next;
    }
    
    public override void SetNext(bool val)
    {        
        next = val;
    }

    public override ExperimentState HandleInput(ExperimentController ec)
    {

        if(rum.RosBridge.latestPlanningStatus.Count > 0){
            int status = rum.RosBridge.latestPlanningStatus.Dequeue();
            if(status == RosMessages_old.std_msgs.Int32_old.PLANNING_FAILED){                
                fn.ShowMessage("FAIL", new Color(1, 0, 0));
                if (ec.PreviousState.GetType() == typeof(PlannedPlace_State))
                    return nextStates[1]; //PlannedPlace                                    
                return nextStates[0]; //Picked                                    
            }
            else if(status == RosMessages_old.std_msgs.Int32_old.SUCCESS){                
                fn.ShowMessage("SUCCESS", new Color(0,1,0));
                return nextStates[1]; //PlannedPlace                
            }
            else  ms = "wrong state -> exit all"; 
        }
        return this; 
    }

    public override void UpdateState(ExperimentController ec)
    {       
        text.text = ms;
        tableTopCollider.enabled = true;        
    }
}
