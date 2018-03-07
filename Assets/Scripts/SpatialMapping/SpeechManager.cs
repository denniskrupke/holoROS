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
    public ConfigurationManager configurationManager = null;

    public bool demoMode = false;
    public string lastCommand = "";

    [SerializeField]
    StateController sc;
 
    void Start()
    {
        

        //keywords.Add("Mode", () =>
        //{            
        //    this.BroadcastMessage("OnChangeMethod");
        //});


        keywords.Add("Calibrate", () =>
        {           
            this.configurationManager.registration_calibration = true;
            lastCommand = "Calibrate"; 
        });


        keywords.Add("Pick", () =>
        {
            // Triggers pick-point extraction, message creation and enqueueing
            //this.BroadcastMessage("OnPickUp()");
            this.setTransform.OnPickUp();
            lastCommand = "Pick";
            sc.CurrentState.triggerNextState = true;
        });

        keywords.Add("Execute", () =>
        {
            // Triggers execution of previously planned actions
            //this.BroadcastMessage("OnConfirmPick()");
            
            if(lastCommand == "Pick") this.setTransform.OnConfirmPick();
            else if(lastCommand == "Place") this.setTransform.OnConfirmPlace();
            lastCommand = "Execute";
            sc.CurrentState.triggerNextState = true;
        });
       

        keywords.Add("Place", () =>
        {
            // Triggers place-point extraction, message creation and enqueueing
            //this.BroadcastMessage("OnPlace()");
            this.setTransform.OnPlace();
            lastCommand = "Place";
            sc.CurrentState.triggerNextState = true;
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

    void OnQuit()
    {
        Application.Quit();
    }
}
