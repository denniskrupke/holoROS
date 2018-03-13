using UnityEngine;
using System.Collections.Generic;

public class ManageObjectSelection : MonoBehaviour {
    private GameObject lastSelectedObject = null;
    private GameObject currentSelectedObject = null;

    Dictionary<string, Vector3> initialObjectPositions;
	// Use this for initialization
	void Start () {
        initialObjectPositions = new Dictionary<string, Vector3>();
        foreach (Transform t in transform)
        {
            initialObjectPositions.Add(t.name, t.position);
        }        
	}

    public void RememberPositions()
    {        
        foreach (Transform t in transform)
        {
            initialObjectPositions[t.name] = t.position;
        }
    }

    public void ResetPositions()
    {
        foreach (Transform t in transform)
        {
            t.position = initialObjectPositions[t.name];
        }           
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
