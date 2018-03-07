using UnityEngine;
using System.Collections;

public class MoveToExperimentTrial : ExperimentTrial
{
    private Vector3 targetPosition;

    public Vector3 TargetPosition
    {
        get
        {
            return targetPosition;
        }
    }
    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="_targetPosition"></param>
    public MoveToExperimentTrial(int trialnum, Vector3 _targetPosition) : base(trialnum)
    {
        targetPosition = _targetPosition;
    }
    public override string ToString()
    {
        string ret = base.ToString() +";"+ targetPosition.x.ToString("F5")+";"+ targetPosition.y.ToString("F5")+";"+ targetPosition.z.ToString("F5");
        return ret;
    }
}
