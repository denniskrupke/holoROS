using UnityEngine;
using System.Collections;

public abstract class ExperimentState : MonoBehaviour {

    public ExperimentState[] nextStates;
    public int nextStateIndex = 0;    
    public abstract ExperimentState HandleInput(ExperimentController ec);
    public abstract void UpdateState(ExperimentController ec);
    public abstract bool GetNext();
    public abstract void SetNext(bool val);
}
