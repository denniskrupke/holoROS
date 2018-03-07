using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class MoveToExperimentController : ExperimentController {

    [SerializeField]
    Vector3 targetPosition;
    [SerializeField]
    int trialCount = 10;

    // Use this for initialization
    void Start () {
        Init();
        FillTrials();        
	}

    // Update is called once per frame
    protected override void Update()
    {
        base.Update();

    }

    protected override void FillTrials()
    {
        currentTrials = new List<ExperimentTrial>();
        for(int i  = 0; i < trialCount; i++)
        {
            CurrentTrials.Add(new MoveToExperimentTrial(i, targetPosition + new Vector3(UnityEngine.Random.value, UnityEngine.Random.value)));
        }
        ExtensionMethods.Shuffle(currentTrials);    
    }
}

