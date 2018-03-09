using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System;

public class ExecutePick_State : ExperimentState
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
            if(status == RosMessages_old.std_msgs.Int32_old.HOLD_OBJECT) next = true; //picked
        }

        if(next)
        {
            return nextStates[0];//picked
        }
        else
        {
            return this;
        }        
    }

    public override void UpdateState(ExperimentController ec)
    {        
        text.text = "executing";
        tableTopCollider.enabled = true;         
    }
}
