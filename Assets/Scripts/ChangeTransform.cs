using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChangeTransform : MonoBehaviour {
	public Transform moveableObject;
	private bool rotationMode = false;
	public float step = .25f;
	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		if (Input.GetKey (KeyCode.Joystick1Button0)) {
//			Debug.Log ("Button0");				
		} else if (Input.GetKey (KeyCode.H)) {
//			Debug.Log ("A");
		} else if (Input.GetKey (KeyCode.X)) {//J
			Debug.Log ("B");
			if (!rotationMode) {
				moveableObject.position = new Vector3 (moveableObject.position.x,moveableObject.position.y,moveableObject.position.z+step);
			} else {
			}
		} else if (Input.GetKey (KeyCode.Z)) {
//			Debug.Log ("X");
			if (!rotationMode) {
				moveableObject.position = new Vector3 (moveableObject.position.x,moveableObject.position.y,moveableObject.position.z-step);
			} else {
			}
		} else if (Input.GetKey (KeyCode.U)) {
//			Debug.Log ("Y");
		} else if (Input.GetKey (KeyCode.LeftArrow)) {
//			Debug.Log ("left");
			if (!rotationMode) {
				moveableObject.position = new Vector3 (moveableObject.position.x-step,moveableObject.position.y,moveableObject.position.z);
			} else {
			}
		} else if (Input.GetKey (KeyCode.RightArrow)) {
//			Debug.Log ("right");
			if (!rotationMode) {
				moveableObject.position = new Vector3 (moveableObject.position.x+step,moveableObject.position.y,moveableObject.position.z);
			} else {
			}
		} else if (Input.GetKey (KeyCode.UpArrow)) {
//			Debug.Log ("up");
			if (!rotationMode) {
				moveableObject.position = new Vector3 (moveableObject.position.x,moveableObject.position.y+step,moveableObject.position.z);
			} else {
			}
		} else if (Input.GetKey (KeyCode.DownArrow)) {
//			Debug.Log ("down");		
			if (!rotationMode) {
				moveableObject.position = new Vector3 (moveableObject.position.x,moveableObject.position.y-step,moveableObject.position.z);
			} else {
			}
		} else if (Input.GetKeyUp (KeyCode.P)) {
			//if(Input.GetKey (KeyCode.P))
			//Debug.Log ("select");		
			//rotationMode = !rotationMode;

		} 
	}
}
