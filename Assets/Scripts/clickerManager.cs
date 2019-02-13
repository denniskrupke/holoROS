
using UnityEngine;
using HoloToolkit.Unity.InputModule;



public class clickerManager : MonoBehaviour, IInputClickHandler, IInputHandler{

    public GameObject MessageClicker = null;
    public SetTransform setTransform = null;
    int i = 0;



    [SerializeField]
    StateController sc;

    // Use this for initialization
    void Start () {

    }

    public void OnInputClicked(InputClickedEventData eventData)
    {
        i++;

        MessageClicker.GetComponent<UnityEngine.UI.Text>().text = i.ToString();

        if (sc.CurrentState.GetType() == typeof(Idle_State))
        {
            MessageClicker.GetComponent<UnityEngine.UI.Text>().text = i.ToString() + " pick";
            sc.PreviousState = sc.CurrentState;
            // Triggers pick-point extraction, message creation and enqueueing            
            this.setTransform.OnPickUp();
            sc.lastCommand = "Pick";
            if ((sc.CurrentState.GetType() == typeof(PlannedPick_State)) || (sc.CurrentState.GetType() == typeof(Idle_State)))
            {
                sc.CurrentState.SetNext(true);
            }
        }

        else if (sc.CurrentState.GetType() == typeof(PlannedPick_State))
        {
            MessageClicker.GetComponent<UnityEngine.UI.Text>().text = i.ToString() + " exec pick";

            this.setTransform.OnConfirmPick();
            sc.CurrentState.SetNext(true);
            sc.lastCommand = "Execute";

        }

        else if (sc.CurrentState.GetType() == typeof(Picked_State))
        {
            MessageClicker.GetComponent<UnityEngine.UI.Text>().text =  i.ToString() +" place";

            sc.PreviousState = sc.CurrentState;
            // Triggers place-point extraction, message creation and enqueueing            
            this.setTransform.OnPlace();
            sc.lastCommand = "Place";
            if ((sc.CurrentState.GetType() == typeof(PlannedPlace_State)) || (sc.CurrentState.GetType() == typeof(Picked_State)))
            {
                sc.CurrentState.SetNext(true);
            }
        }
       
        else if (sc.CurrentState.GetType() == typeof(PlannedPlace_State))
        {
            MessageClicker.GetComponent<UnityEngine.UI.Text>().text = i.ToString() + " exec place";

            this.setTransform.OnConfirmPlace();
            sc.CurrentState.SetNext(true);
            sc.lastCommand = "Execute";

        }
    }

    public void OnInputDown(InputEventData eventData)
    {
        return;
    }
    public void OnInputUp(InputEventData eventData)
    {
        return;
    }

 
}
