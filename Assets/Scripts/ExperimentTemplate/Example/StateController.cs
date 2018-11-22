using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class StateController : ExperimentController {
    [SerializeField]
    Error_State errorState;

    public String lastCommand = "";
    

    // Use this for initialization
    void Start () {
        //Init();
        //FillTrials();        
	}

    // Update is called once per frame
    protected override void Update()
    {
        base.Update();
    }


    protected override void FillTrials()
    {
         
    }

    public void GoToReset()
    {
        currentState = errorState;
        errorState.SendResetMessage();          
    }
    
}

