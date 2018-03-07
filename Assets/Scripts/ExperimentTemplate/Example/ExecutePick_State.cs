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
        nextStateIndex = 0;//picked
        if(rosbridgeClient.latestPlanningStatus == std_msgs.Int32_old.HOLD_OBJECT) next = true;

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
        text.text = "executing";
    }
}
