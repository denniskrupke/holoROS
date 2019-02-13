using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Windows.Speech;

public class SpeechManager : MonoBehaviour
{
    KeywordRecognizer keywordRecognizer = null;
    Dictionary<string, System.Action> keywords = new Dictionary<string, System.Action>();
    public GameObject HUD = null;
    public SetTransform setTransform = null;
    //public ConfigurationManager configurationManager = null;

    public bool demoMode = false;

    [SerializeField]
    StateController sc;

    //[SerializeField]
    //ManageObjectSelection mos;

    void Start()
    {                
        //keywords.Add("Calibrate", () =>
        //{           
        //    this.configurationManager.registration_calibration = true;
        //    lastCommand = "Calibrate";            
        //});

        //keywords.Add("Detect", () =>
        //{
        //    this.configurationManager.registration_calibration = true;
        //    lastCommand = "Calibrate";            
        //});

        keywords.Add("Pick", () =>
        {
            sc.PreviousState = sc.CurrentState;
            // Triggers pick-point extraction, message creation and enqueueing            
            this.setTransform.OnPickUp();
            sc.lastCommand = "Pick";
            if((sc.CurrentState.GetType() == typeof(PlannedPick_State)) || (sc.CurrentState.GetType() == typeof(Idle_State))) sc.CurrentState.SetNext(true);
        });


        keywords.Add("Place", () =>
        {
            sc.PreviousState = sc.CurrentState;
            // Triggers place-point extraction, message creation and enqueueing            
            this.setTransform.OnPlace();            
            if((sc.CurrentState.GetType() == typeof(PlannedPlace_State)) || (sc.CurrentState.GetType() == typeof(Picked_State))) sc.CurrentState.SetNext(true);
            sc.lastCommand = "Place";
        });


        keywords.Add("Confirm", () =>
        {
            _execute();
        });

        keywords.Add("Execute", () =>
        {
            _execute();
        });

        keywords.Add("Reset", () =>
        {
            sc.GoToReset();
        });

        keywords.Add("Quit", () =>
        {
            Application.Quit();
        });

        keywords.Add("Exit", () =>
        {
            Application.Quit();
        });


        // Tell the KeywordRecognizer about our keywords.
        keywordRecognizer = new KeywordRecognizer(keywords.Keys.ToArray());

        // Register a callback for the KeywordRecognizer and start recognizing!
        keywordRecognizer.OnPhraseRecognized += KeywordRecognizer_OnPhraseRecognized;
        keywordRecognizer.Start();
    }

    private void KeywordRecognizer_OnPhraseRecognized(PhraseRecognizedEventArgs args)
    {
        System.Action keywordAction;
        if (keywords.TryGetValue(args.text, out keywordAction))
        {
            keywordAction.Invoke();
        }
    }

    void _execute()
    {
        // Triggers execution of previously planned actions            
        if (sc.CurrentState.GetType() == typeof(PlannedPick_State))
        {
            this.setTransform.OnConfirmPick();
            sc.CurrentState.SetNext(true);
        }
        else if (sc.CurrentState.GetType() == typeof(PlannedPlace_State))
        {
            this.setTransform.OnConfirmPlace();
            sc.CurrentState.SetNext(true);
        }

        sc.lastCommand = "Execute";
    }

    void OnQuit()
    {
        Application.Quit();
    }
}
