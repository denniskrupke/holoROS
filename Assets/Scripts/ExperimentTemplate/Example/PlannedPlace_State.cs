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
        if(sm.lastCommand == "Place"){
            nextStateIndex = 0; //planningPlace
        }
        else if(sm.lastCommand == "Execute"){
            nextStateIndex = 1; //executePlace
        }


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
        text.text = "";
    }
}
