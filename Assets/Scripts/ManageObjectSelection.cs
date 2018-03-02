using UnityEngine;

public class ManageObjectSelection : MonoBehaviour {
    private GameObject lastSelectedObject = null;
    private GameObject currentSelectedObject = null;
	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    public GameObject LastSelectedObject {
        get
        {
            return lastSelectedObject;
        }

        set
        {
            lastSelectedObject = value;
        }
    }

    public GameObject CurrentSelectedObject {
        get
        {
            return currentSelectedObject;
        }

        set
        {
            currentSelectedObject = value;
        }
    }
}
