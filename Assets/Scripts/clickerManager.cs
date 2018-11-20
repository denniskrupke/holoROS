
using UnityEngine;
using HoloToolkit.Unity.InputModule;
using UnityEngine.UI;




public class clickerManager : MonoBehaviour, IInputClickHandler, IInputHandler{

    public GameObject MessageClicker = null;
    public SetTransform setTransform = null;


    [SerializeField]
    StateController sc;

    // Use this for initialization
    void Start () {
        MessageClicker.GetComponent<UnityEngine.UI.Text>().text = "start";

    }

    public void OnInputClicked(InputClickedEventData eventData)
    {
        MessageClicker.GetComponent<UnityEngine.UI.Text>().text = "clicked";
        if (sc.CurrentState.GetType() == typeof(Idle_State))
        {
            sc.PreviousState = sc.CurrentState;
            // Triggers pick-point extraction, message creation and enqueueing            
            this.setTransform.OnPickUp();
            if ((sc.CurrentState.GetType() == typeof(PlannedPick_State)) || (sc.CurrentState.GetType() == typeof(Idle_State))) sc.CurrentState.SetNext(true);
            //          sc.CurrentState.SetNext(true);
        }
    }

    public void OnInputDown(InputEventData eventData)
    {
        MessageClicker.GetComponent<UnityEngine.UI.Text>().text = "clicked";
    }
    public void OnInputUp(InputEventData eventData)
    {
        MessageClicker.GetComponent<UnityEngine.UI.Text>().text = "clicked";
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

    }


}
