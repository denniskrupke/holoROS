using UnityEngine;
using System.Collections;

public abstract class ExperimentState : MonoBehaviour {

    public ExperimentState[] nextStates;
    int nextStateIndex = 0;
    public abstract ExperimentState HandleInput(ExperimentController ec);
    public abstract void UpdateState(ExperimentController ec);
}
