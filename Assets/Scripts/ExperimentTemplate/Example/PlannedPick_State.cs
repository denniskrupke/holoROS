using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System;

public class PlannedPick_State : ExperimentState
{    
    [SerializeField]
    Text text;

    string ms = "planned pick";  

    [SerializeField]
    StateController sc;

    [SerializeField]
    ros2unityManager rum;

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
        if (rum.RosBridge.latestPlanningStatus.Count > 0)
        {
            int status = rum.RosBridge.latestPlanningStatus.Dequeue();
            if (status != RosMessages_old.std_msgs.Int32_old.PLANNED_PICK) { ms = "wrong state -> exit all"; }            
        }

        if (next)
        {
            if(sc.lastCommand == "Pick"){
                nextStateIndex = 0; //planningPick
                next = false;
            }
            else if(sc.lastCommand == "Execute"){
                nextStateIndex = 1; //executePick
                next = false;
            }
            
            return nextStates[nextStateIndex];
        }
       
        return this;
    }

    public override void UpdateState(ExperimentController ec)
    {        
        text.text = ms;        
    }
}
