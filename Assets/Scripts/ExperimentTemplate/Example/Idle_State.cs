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
        nextStateIndex = 0;        

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
        text.text = "idle";
        if (triggerNextState)
        {
            next = true;
            triggerNextState = false;
        }
    }
}
