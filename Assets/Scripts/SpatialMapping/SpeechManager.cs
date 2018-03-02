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

    // Use this for initialization
    void Start()
    {
        //keywords.Add("Start", () =>
        //{
        //    // Call the OnNext method on every descendant object.
        //    HUD.SetActive(false);
        //    setTransform.ResetArm();
        //    this.BroadcastMessage("OnNext");
        //    this.demoMode = false;
        //});
      
        /*
        // keywords.Add("Set Target", () =>
        keywords.Add("Move", () =>
        {
            // Call the OnReset method on every descendant object.
            this.BroadcastMessage("OnMove");
        });
        */

        //keywords.Add("Set", () =>
        //{            
        //    this.BroadcastMessage("OnSetTarget");
        //});


        //keywords.Add("Confirm", () =>
        //{            
        //    this.BroadcastMessage("OnConfirm");
        //});
        

        //keywords.Add("Reset", () =>
        //{            
        //    this.BroadcastMessage("OnResetArm");
        //});


        //keywords.Add("Quit", () =>
        //{            
        //    this.BroadcastMessage("OnQuit");
        //});


        //keywords.Add("Mode", () =>
        //{            
        //    this.BroadcastMessage("OnChangeMethod");
        //});


        keywords.Add("Calibrate", () =>
        {            
            this.configurationManager.registration_calibration = true;
        });


        keywords.Add("Pick", () =>
        {
            // Triggers pick-point extraction, message creation and enqueueing
            //this.BroadcastMessage("OnPickUp()");
            this.setTransform.OnPickUp();
        });

        keywords.Add("Execute Pick", () =>
        {
            // Triggers execution of previously planned actions
            //this.BroadcastMessage("OnConfirmPick()");
            this.setTransform.OnConfirmPick();
        });

        keywords.Add("Open Gripper", () =>
        {
            // Triggers execution of previously planned actions
            //this.BroadcastMessage("OnOpenGripper()");
            this.setTransform.OnOpenGripper();
        });        

        keywords.Add("Place", () =>
        {
            // Triggers place-point extraction, message creation and enqueueing
            //this.BroadcastMessage("OnPlace()");
            this.setTransform.OnPlace();
        });


        keywords.Add("Execute Place", () =>
        {
            this.setTransform.OnConfirmPlace();
            // Triggers execution of previously planned actions
            //this.BroadcastMessage("OnConfirmPlace");
            //this.setTransform.O
        });
        // keywords.Add("Confirm", () =>
        // {
        //     // Triggers execution of previously planned actions
        //     OnConfirm();
        // });

        /*
  keywords.Add("Change Method", () =>
  {
      // Call the OnReset method on every descendant object.
      this.BroadcastMessage("OnChangeMethod");
  });

  keywords.Add("Debug", () =>
  {
      // Call the OnReset method on every descendant object.
      this.BroadcastMessage("OnDebug");
  });

  keywords.Add("Gripper Close", () =>
  {
      // Call the OnReset method on every descendant object.
      this.BroadcastMessage("OnGripperClose");
  });

  keywords.Add("Gripper Open", () =>
  {
      // Call the OnReset method on every descendant object.
      this.BroadcastMessage("OnGripperOpen");
  });
  */
        /*
        keywords.Add("Change Computer", () =>
        {
            // Call the OnReset method on every descendant object.
            this.BroadcastMessage("OnChangeComputer");
        });
        */
        /*
                keywords.Add("Drop Sphere", () =>
                {
                    var focusObject = GazeGestureManager.Instance.FocusedObject;
                    if (focusObject != null)
                    {
                        // Call the OnDrop method on just the focused object.
                        focusObject.SendMessage("OnDrop");
                    }
                });
                */


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
