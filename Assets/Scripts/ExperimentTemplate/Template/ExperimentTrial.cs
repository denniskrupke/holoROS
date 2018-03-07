﻿using UnityEngine;
using System.Collections;

public abstract class ExperimentTrial {
    private int trialNum;

    public int TrialNum
    {
        get
        {
            return trialNum;
        }
    }
    public ExperimentTrial(int _trialNum)
    {
        trialNum = _trialNum;
    }
    public override string ToString()
    {
        string ret = trialNum.ToString();
        return ret;
    }
}
