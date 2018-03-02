using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

[RequireComponent(typeof(ParticleSystem))]

public class RenderLocalWorkspaceSphereAsParticles : MonoBehaviour {
	[SerializeField]
	string filename = "default.csv";	

	[SerializeField]
	float startSize = 0.01f;

	[SerializeField]
	float alphaValue = 0.05f;

	[SerializeField]
	float maxDistance = 0.05f;

	[SerializeField]
	Transform goalPosition = null;

	[SerializeField]
	Transform referenceOrigin = null;

    [SerializeField]
    Transform tcp = null;

    [SerializeField]
    bool followRobot = true;

    ParticleSystem ps;

	private float lastParticleSize;
	private ParticleSystem.Particle[] points;
	private Color solutionSuccess;
	private Color solutionFail;
	private string[] tempArray;
	private List<ConfigurationPose> poslist;
	private Vector3 invPos;
	private int count = 0;
	private Color currentColor;
	private ConfigurationPose[] positionArray;


	void Start () {
		solutionFail = new Color (1.0f, .0f, .0f, alphaValue);
		solutionSuccess = new Color (.0f, 1.0f, .0f, alphaValue);
		lastParticleSize = startSize;
		ps = GetComponent<ParticleSystem>();
		poslist = new List<ConfigurationPose>();
		foreach (string point in File.ReadAllLines(filename)) {
			tempArray = point.Split (';');
			poslist.Add (new ConfigurationPose(new Vector3 (
				float.Parse (tempArray[0]), 	//x
				float.Parse (tempArray[1]),		//y
				float.Parse (tempArray[2])),	//z
				bool.Parse(tempArray[3]),		//success
				float.Parse (tempArray[4]), 	//distance
				float.Parse (tempArray[5]) 	//quality 
			));
		}
		positionArray = poslist.ToArray ();
		points = new ParticleSystem.Particle[positionArray.Length];
		CalculateParticles(positionArray);
	}


	void CalculateParticles(ConfigurationPose[] arr) {		
		invPos = referenceOrigin.transform.InverseTransformPoint(goalPosition.position);
		for (int i = 0; i < arr.Length; i++) {            
			// Checks the distance of the solution to the current goal position and adds the point as a particle if smaller
			if (Vector3.Distance (arr [i].position, invPos) <= maxDistance) {
				points [count].position = arr [i].position;
				points [count].startSize = startSize;
				//points [count].startColor = (arr [i].success) ? solutionSuccess : solutionFail;
				currentColor = Color.HSVToRGB(arr[i].quality/3.0f,1.0f,1.0f);
				currentColor.a = alphaValue;
				points [count].startColor = currentColor;
				//		Debug.Log (arr [i].quality * 0.3f);
				count++;
			}

		}
		//		Debug.Log (count);
		ps.SetParticles(points, count+1);
		count = 0;
	}


	void Update () {
        if(followRobot) goalPosition.position = new Vector3(tcp.position.x, tcp.position.y, tcp.position.z);
		CalculateParticles(positionArray);
	}

}