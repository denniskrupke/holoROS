using UnityEngine;
using UnityEngine.UI;

public class FadingNotification : MonoBehaviour
{

	[SerializeField]
	Canvas notificationCanvas;

	[SerializeField]
	Text text;

	[SerializeField]
	long defaultDuration; //in seconds
	
	private bool showMessage;
	private float startTime;

	// Use this for initialization
	void Start () {
        notificationCanvas.gameObject.SetActive(false);
    }
	
	// Update is called once per frame
	void Update () {
		if(showMessage){
			notificationCanvas.gameObject.SetActive(true);
			startTime = Time.realtimeSinceStartup;		
			showMessage = false;
		}
        
		if(Time.realtimeSinceStartup < (startTime+defaultDuration)){
			float timeLeft = (startTime+defaultDuration) - Time.realtimeSinceStartup;
			float val = (defaultDuration-timeLeft)/defaultDuration;			
			Color color = text.color;
			color.a = 1-val;
			text.color = color;
		}       
        else notificationCanvas.gameObject.SetActive(false);
    }

	public void ShowMessage(string message, Color col){
		text.text = message;
        text.color = col;
		showMessage = true;
	}

}