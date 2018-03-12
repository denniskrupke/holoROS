using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System;

public class ExecutePlace_State : ExperimentState
{    
    [SerializeField]
    Text text;

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
            if (status == RosMessages_old.std_msgs.Int32_old.IDLE)
                return nextStates[0]; //idle                       
        }

        return this;           
    }

    public override void UpdateState(ExperimentController ec)
    {     
        text.text = "executing place";        
    }
}
