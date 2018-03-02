using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public enum Mode {X,Y,Z,A,B,G};

// sets translation and rotation using a keyboard
public class KeyboardTransform : MonoBehaviour {
    [SerializeField]
    float stepsize = 0.001f;

    [SerializeField]
    float multiplicator = 1.0f;

	[SerializeField]
	string filenameResults = "poses.csv";

	[SerializeField]
	Transform referenceOrigin = null;

	[SerializeField]
	GameObject targetMarker = null;

	[SerializeField]
	List<Vector3> goalPositions;

    private Mode mode = Mode.X;
    private Quaternion startRot;
	private Vector3 startPos;
    private Vector3 localPos;

	private List<Vector3> userinputPositions;
    private List<Quaternion> userinputRotations;

    private int count = 0;

    // TODO: these values could be read directly from the associated gameobjects
	private const float widthTable = 1.0f;
	private const float heightTable = .8f;
	private const float sizeObject = 0.05f;
	private const float heightObject = 0.1f;
    

    // Use this for initialization
    void Start () {
        startRot = Quaternion.Euler(0, 0, 0);  //transform.rotation;   
		startPos = transform.position;

		userinputPositions = new List<Vector3> ();
        userinputRotations = new List<Quaternion>();

        goalPositions = new List<Vector3> ();
		MoveTargetToNextPosition ();
    }

    // calculates local-tablesurface-coordinates and then transforms to world coordinates
	void MoveTargetToNextPosition(){
        Vector3 localPosition = new Vector3();

        if ((count % 4)  == 0){localPosition = new Vector3 (
			- widthTable / 2 + sizeObject / 2, 
			+ heightObject / 2,
			- heightTable / 2 + sizeObject / 2);            
        }
		else if((count % 4)  == 1){localPosition = new Vector3 (
			- widthTable / 2 + sizeObject / 2, 
			+ heightObject / 2,
			+ heightTable/ 2 - sizeObject / 2); 
		}
		else if((count % 4)  == 2) {localPosition = new Vector3 (
			+ widthTable / 2 - sizeObject / 2, 
			+ heightObject / 2,
			+ heightTable / 2 - sizeObject / 2); 
		}
		else if((count % 4)  == 3){localPosition = new Vector3 (
			+ widthTable / 2 - sizeObject / 2, 
			+ heightObject / 2,
			- heightTable / 2 + sizeObject / 2); 
		}
        targetMarker.transform.position = referenceOrigin.TransformPoint(localPosition);
        goalPositions.Add (targetMarker.transform.position);
		count++;
	}
	
	// Update is called once per frame
	void Update () {
        if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.I))
        {            
			//Quaternion localRotation = referenceOrigin.InverseTransformRotation(transform.rotation);
            switch (mode)
            {
				case (Mode.X): {
					Vector3 localPosition = referenceOrigin.InverseTransformPoint(transform.position); // transform to table coordinates
					localPosition.z += stepsize * multiplicator;
					transform.position = referenceOrigin.TransformPoint(localPosition);// transform to world coordinates
				} 
				break;
				case (Mode.Y): {
					Vector3 localPosition = referenceOrigin.InverseTransformPoint(transform.position); // transform to table coordinates
					localPosition.x += stepsize * multiplicator;
					transform.position = referenceOrigin.TransformPoint(localPosition);// transform to world coordinates
				}
				break;
				case (Mode.Z): {
					Vector3 localPosition = referenceOrigin.InverseTransformPoint(transform.position); // transform to table coordinates
					localPosition.y += stepsize * multiplicator;
					transform.position = referenceOrigin.TransformPoint(localPosition);// transform to world coordinates
				} 
				break;
                
                // to rotate around another axis, extract the axis first from the desired reference frame
                case (Mode.A): transform.Rotate(new Vector3(0, 0, 1), stepsize * multiplicator * 100.0f, Space.Self); break;
                case (Mode.B): transform.Rotate(new Vector3(0, 1, 0), stepsize * multiplicator * 100.0f, Space.Self); break;
                case (Mode.G): transform.Rotate(new Vector3(1, 0, 0), stepsize * multiplicator * 100.0f, Space.Self); break;
            }            
        }
        else if (Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.K))
        {           
            switch (mode)
            {
                case (Mode.X): {
                	Vector3 localPosition = referenceOrigin.InverseTransformPoint(transform.position);
                	localPosition.z -= stepsize * multiplicator; 
                	transform.position = referenceOrigin.TransformPoint(localPosition);
            	}
            	break;
                case (Mode.Y): {
                	Vector3 localPosition = referenceOrigin.InverseTransformPoint(transform.position);
                	localPosition.x -= stepsize * multiplicator;
                	transform.position = referenceOrigin.TransformPoint(localPosition);
            	}
            	break;
                case (Mode.Z): {
                	Vector3 localPosition = referenceOrigin.InverseTransformPoint(transform.position);
                	localPosition.y -= stepsize * multiplicator;
                	transform.position = referenceOrigin.TransformPoint(localPosition);
            	} 
            	break;
                
                case (Mode.A): transform.Rotate(new Vector3(0, 0, 1), stepsize * multiplicator * -100.0f, Space.Self); break;
                case (Mode.B): transform.Rotate(new Vector3(0, 1, 0), stepsize * multiplicator * -100.0f, Space.Self); break;
                case (Mode.G): transform.Rotate(new Vector3(1, 0, 0), stepsize * multiplicator * -100.0f, Space.Self); break;
            }
            
        }
        else if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.L))
        {
            multiplicator *= 10.0f;
        }
        else if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.J))
        {
            multiplicator /= 10.0f;
        }
        else if (Input.GetKeyDown(KeyCode.X))
        {
            mode = Mode.X;
        }
        else if (Input.GetKeyDown(KeyCode.Y))
        {
            mode = Mode.Y;
        }
        else if (Input.GetKeyDown(KeyCode.Z))
        {
            mode = Mode.Z;
        }
        else if (Input.GetKeyDown(KeyCode.A))
        {
            mode = Mode.A;
        }
        else if (Input.GetKeyDown(KeyCode.B))
        {
            mode = Mode.B;
        }
        else if (Input.GetKeyDown(KeyCode.G))
        {
            mode = Mode.G;
        }
        else if (Input.GetKeyDown(KeyCode.R))
        {
            multiplicator = 1.0f;
        }
		else if (Input.GetKeyDown(KeyCode.Space)) //Add pose to List
		{
			userinputPositions.Add(transform.position);
            userinputRotations.Add(transform.rotation);
            MoveTargetToNextPosition ();
		}
		else if (Input.GetKeyDown(KeyCode.Return))//Save file
		{
			int n = 0;
			foreach (Vector3 t in userinputPositions) {
				localPos = referenceOrigin.transform.InverseTransformPoint(t);
				float relativeAngle = Quaternion.Angle(startRot, userinputRotations[n]);
				Vector3 localGoalPos = referenceOrigin.transform.InverseTransformPoint(goalPositions[n]);
                File.AppendAllText(Path.Combine(Application.persistentDataPath, filenameResults),		
					""
					+ t.x	//adjusted position
					+ ","
					+ t.y
					+ ","
					+ t.z
					+ ","
                    + localPos.x   //adjusted position in table-surface coordinates
                    + ","
                    + localPos.y
                    + ","
                    + localPos.z
                    + ","
                    + userinputRotations[n].x	//adjusted rotation (quaternion)
					+ ","
					+ userinputRotations[n].y
					+ ","
					+ userinputRotations[n].z
					+ ","
					+ userinputRotations[n].w
					+ ","
					+ userinputRotations[n].eulerAngles.x //adjusted rotation (euler)
					+ ","
					+ userinputRotations[n].eulerAngles.y
					+ ","
					+ userinputRotations[n].eulerAngles.z
					+ ","
					+ goalPositions[n].x	//ideal position
					+ ","
					+ goalPositions[n].y
					+ ","
					+ goalPositions[n].z 
					+ ","
					+ localGoalPos.x	//ideal position
					+ ","
					+ localGoalPos.y
					+ ","
					+ localGoalPos.z
					+ ","
					+ startRot.x	//ideal rotation (quaternion)
					+ ","
					+ startRot.y
					+ ","
					+ startRot.z
					+ ","
					+ startRot.w
					+ ","
					+ startRot.eulerAngles.x //ideal rotation (euler)
					+ ","
					+ startRot.eulerAngles.y
					+ ","
					+ startRot.eulerAngles.z
					+ ","
					+ Vector3.Distance(t, goalPositions[n])	//Euclidean distance to ideal position
					+ ","
					+ relativeAngle	//Difference to ideal rotation (shortest angle)
					+ Environment.NewLine
				);
                n += 1;
			}
		}
    }
		

}

