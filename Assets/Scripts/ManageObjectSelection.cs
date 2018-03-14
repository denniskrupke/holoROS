using UnityEngine;
using System.Collections.Generic;

public class ManageObjectSelection : MonoBehaviour {
    private GameObject lastSelectedObject = null;
    private GameObject currentSelectedObject = null;

    Dictionary<string, Vector3> initialObjectPositions;
    //Dictionary<string, Quaternion> initialObjectRotations;

    [SerializeField]
    Transform tabletopAnker;
    // Use this for initialization
    void Start () {
        initialObjectPositions = new Dictionary<string, Vector3>();
        //initialObjectRotations = new Dictionary<string, Quaternion>();
        foreach (Transform t in transform)
        {
            initialObjectPositions.Add(t.name, tabletopAnker.InverseTransformPoint(t.position));
            //initialObjectRotations.Add(t.name, t.rotation);
        }        
	}

    public void RememberPositions()
    {        
        foreach (Transform t in transform)
        {
            initialObjectPositions[t.name] = tabletopAnker.InverseTransformPoint(t.position);
            //initialObjectRotations[t.name] = t.rotation;
        }
    }

    public void ResetPositions()
    {
        foreach (Transform t in transform)
        {
            t.position = tabletopAnker.TransformPoint(initialObjectPositions[t.name]);
            //t.rotation = initialObjectRotations[t.name];
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
