using UnityEngine;
using UnityEngine.UI;


public class Error_State : ExperimentState
{
    [SerializeField]
    Text text;    

    [SerializeField]
    ros2unityManager rum;

    [SerializeField]
    ManageObjectSelection mos;

    string ms = "error state";

    bool next = false;

    public override bool GetNext()
    {
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
        text.text = ms;
    }

    public void SendResetMessage()
    {
        RosMessages_old.std_msgs.Empty_old reset = new RosMessages_old.std_msgs.Empty_old();
        rum.RosBridge.EnqueRosCommand(new RosMessages_old.RosPublish_old("/hololens/reset", reset));
        mos.ResetPositions();        
    }
}

