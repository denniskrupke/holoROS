using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;

public abstract class ExperimentController
    : Singleton<ExperimentController>
    {


    [SerializeField]
    protected ExperimentState currentState;
    protected ExperimentState previousState;

    public string resultsDirectoryPath = @"results/";
    private int currentTrialIndex;
    private int currentSubjectID;
    protected StreamWriter outputStream;

    protected List<ExperimentTrial> currentTrials;

    public ExperimentState CurrentState
    {
        get
        {
            return currentState;
        }
    }

    public ExperimentState PreviousState
    {
        get
        {
            return previousState;
        }
    }

    public List<ExperimentTrial> CurrentTrials
    {
        get
        {
            return currentTrials;
        }
    }

    public int CurrentTrialIndex
    {
        get
        {
            return currentTrialIndex;
        }

        set
        {
            currentTrialIndex = value;
        }
    }

    // Use this for initialization
    void Start () {
        
    }
	protected virtual void Init()
    {
        createFile();
    }
	// Update is called once per frame
	protected virtual void Update () {
        previousState = currentState;
        currentState = currentState.HandleInput(this);
        currentState.UpdateState(this);
    }

    protected abstract void FillTrials();
    /// <summary>
    /// checks how many files are in the resultsDirectoryPath and generates an according new index out of that information
    /// </summary>
    /// <returns></returns>
    protected int getNextFileName()
    {
        string path = resultsDirectoryPath;

        if (!Directory.Exists(path))
        {
            Directory.CreateDirectory(path);
        }

        int fileNum = Directory.GetFiles(path).Length;

        return fileNum;
    }
    /// <summary>
    /// Starts the outputstream and creates the output file
    /// </summary>
    /// <returns></returns>
    protected bool createFile()
    {
        currentSubjectID = getNextFileName();

        string path = resultsDirectoryPath + currentSubjectID + ".txt";

        if (!File.Exists(path))
        {
            //outputStream = new StreamWriter(path, true);
        }
        else
        {
            Debug.LogError("file already exists");
            Application.Quit();
        }
        return true;
    }
    /// <summary>
    /// Writes a line into the output file using the currentSubjectID;currentTrialIndex + parameter s
    /// </summary>
    /// <param name="s">string to additionally write into the file</param>
    public void WriteLog(string s)
    {
        outputStream.WriteLine(currentSubjectID +";"+ currentTrialIndex + s);
        outputStream.Flush();
    }
    /// <summary>
    /// Cleanup process
    /// </summary>
    void OnDestroy()
    {
        outputStream.Flush();
        //outputStream.Close();
    }

}
