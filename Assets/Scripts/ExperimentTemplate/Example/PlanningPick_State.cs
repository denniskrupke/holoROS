using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System;

public class PlanningPick_State : ExperimentState
{    
    [SerializeField]
    Text text;

    [SerializeField]
    ros2unityManager rum;    

    [SerializeField]
    Collider tableTopCollider;

    [SerializeField]
    FadingNotification fn;

    string ms = "planning pick";

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
            if (status == RosMessages_old.std_msgs.Int32_old.PLANNING_FAILED)
            {
                fn.ShowMessage("FAIL", new Color(1, 0, 0));
                if (ec.PreviousState.GetType() == typeof(PlannedPick_State))
                    return nextStates[1]; //PlannedPick                   
                return nextStates[0]; //Idle
            }
            else if (status == RosMessages_old.std_msgs.Int32_old.SUCCESS)
            {                               
                fn.ShowMessage("SUCCESS", new Color(0, 1, 0));
                return nextStates[1];
            }
            else ms = "wrong state -> exit all";
        }
        return this;                  
    }

    public override void UpdateState(ExperimentController ec)
    {        
        text.text = ms;
        tableTopCollider.enabled = false;        
    }
}
