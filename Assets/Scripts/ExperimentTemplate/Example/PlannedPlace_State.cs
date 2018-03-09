using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System;

public class PlannedPlace_State : ExperimentState
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
        if(next)
        {
            if(sm.lastCommand == "Place"){
                nextStateIndex = 0; //planningPlace
            }
            else if(sm.lastCommand == "Execute"){
                nextStateIndex = 1; //executePlace
            }
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
