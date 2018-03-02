using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(ParticleSystem))]

public class RenderWorkspaceAsParticles : MonoBehaviour {
	[SerializeField]
	string filename = "default.csv";

	ParticleSystem ps;

	[SerializeField]
	float startSize = 0.01f;

	[SerializeField]
	float alphaValue = 0.05f;

    public Text debugText = null;

	private float lastParticleSize;
	private ParticleSystem.Particle[] points;
	private Color solutionSuccess;
	private Color solutionFail;
	private string[] tempArray;
    private List<ConfigurationPose> poslist;

    // Use this for initialization
    void Start () {
		solutionFail = new Color (1.0f, .0f, .0f, alphaValue);
		solutionSuccess = new Color (.0f, 1.0f, .0f, alphaValue);

		lastParticleSize = startSize;
		ps = GetComponent<ParticleSystem>();

		poslist = new List<ConfigurationPose>();

        this.debugText.text = "\n Try reading csv..." + this.debugText.text;
        if (File.Exists(filename))
        {
            this.debugText.text = "\n found csv..." + this.debugText.text;
        }
        else this.debugText.text = "\n no csv found" + this.debugText.text;

        foreach (string point in File.ReadAllLines(filename)) {
			tempArray = point.Split (';');
			poslist.Add (new ConfigurationPose(new Vector3 (
				float.Parse (tempArray[0]),
				float.Parse (tempArray[1]),
				float.Parse (tempArray[2])), 
				bool.Parse(tempArray[3])
			));
		}
			
		// generates random points
//		for(int i = 0; i < 1000; i ++)
//		{
//			poslist.Add(UnityEngine.Random.insideUnitSphere);
//		}

		AddPositions(poslist.ToArray());
	}

	 

	void AddPositions(ConfigurationPose[] arr) {
		//Debug.Log(arr[0]);
		points = new ParticleSystem.Particle[arr.Length];
		for(int i = 0; i < arr.Length; i++)
		{
            points[i].position = arr[i].position;// -transform.position;
			points[i].startSize = startSize;
			points[i].startColor = (arr[i].success) ? solutionSuccess : solutionFail;
		}
		ps.SetParticles(points, points.Length);
	}
	
	// Update is called once per frame
	void Update () {
        //AddPositions(poslist.ToArray());

        // getParticles -> größe ändern -> setParticles
        /*
        if (Mathf.Abs(startSize-lastParticleSize)>.001f){ // makes sure that inaccuracy of float will not cause a size-update			
			for(int i=0; i<points.Length; i++){
				points[i].startSize = startSize;	
			}
			ps.SetParticles(points, points.Length);
			lastParticleSize = startSize;
		}
        */
	}
}
