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

	private string message;
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

		if(Time.time > (startTime+defaultDuration)){
			float timeLeft = (startTime+defaultDuration) - Time.realtimeSinceStartup;
			float val = (defaultDuration-timeLeft)/defaultDuration;
			notificationCanvas.gameObject.SetActive(false);
			Color color = text.color;
			color.a = val * 255.0f;
			text.color = color;
		}
	}

	public void ShowMessage(string message){
		this.message = message;
		this.showMessage = true;
	}

}