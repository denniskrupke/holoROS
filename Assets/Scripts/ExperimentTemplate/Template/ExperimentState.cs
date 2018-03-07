using UnityEngine;
using System.Collections;

public abstract class ExperimentState : MonoBehaviour {

    public ExperimentState[] nextStates;
    public int nextStateIndex = 0;
    public bool triggerNextState = false;
    public abstract ExperimentState HandleInput(ExperimentController ec);
    public abstract void UpdateState(ExperimentController ec);
}
