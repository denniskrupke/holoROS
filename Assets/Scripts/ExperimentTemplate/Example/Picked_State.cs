using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System;

public class Picked_State : ExperimentState
{    
    [SerializeField]
    Text text;    

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
            next = false;
            return nextStates[0]; //planningPlace
        }
        else
        {
            return this;
        }        
    }

    public override void UpdateState(ExperimentController ec)
    {        
        text.text = "picked";        
    }
}
