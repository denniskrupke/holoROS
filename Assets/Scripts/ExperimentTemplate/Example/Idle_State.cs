using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System;


public class Idle_State : ExperimentState
{    
    [SerializeField]
    Text text;   

    [SerializeField]
    Collider tableTopCollider;

    [SerializeField]
    ros2unityManager rum;

    string ms = "idle";

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
            if (status != RosMessages_old.std_msgs.Int32_old.IDLE) { ms = "wrong state -> exit all"; }            
        }
        if (next)
        {
            next = false;
            return nextStates[0];//planning pick
        }
        else
        {
            return this;
        }        
    }

    public override void UpdateState(ExperimentController ec)
    {
        text.text = ms;
        tableTopCollider.enabled = false;        
    }
}
