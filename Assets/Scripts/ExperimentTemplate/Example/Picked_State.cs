using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System;

public class Picked_State : ExperimentState
{    
    [SerializeField]
    Text text;

    [SerializeField]
    RosBridge_old.RosBridgeClient_old rosbridgeClient;

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
        nextStateIndex = 0;//picked
        if(rosbridgeClient.LatestPlanningStatus == RosMessages_old.std_msgs.Int32_old.HOLD_OBJECT) next = true;

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
        text.text = "";        
    }
}
