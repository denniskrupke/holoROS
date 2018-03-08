using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System;

public class PlannedPick_State : ExperimentState
{    
    [SerializeField]
    Text text;

    [SerializeField]
    RosBridge_old.RosBridgeClient_old rosbridgeClient;

    [SerializeField]
    SpeechManager sm;

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
        /*
        if(sm.lastCommand == "Pick"){
            nextStateIndex = 0; //planningPick
        }
        else if(sm.lastCommand == "Execute"){
            nextStateIndex = 1; //executePick
        }
        
        if(next)
        {
            next = false;
            return nextStates[nextStateIndex];
        }
       
        */
        return this;
    }

    public override void UpdateState(ExperimentController ec)
    {        
        text.text = "";        
    }
}
