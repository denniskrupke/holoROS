using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Text;
using System;
using System.Linq;


public struct ConfigurationPose{
	public Vector3 position;
	public bool success;
	public float distance;
	public float quality;

	public ConfigurationPose(Vector3 vec, bool suc){
		position = vec;
		success = suc;
		distance = -1.0f;
		quality = -1.0f;
	}

	public ConfigurationPose(Vector3 vec, bool suc, float dist, float qual){
		position = vec;
		success = suc;
		distance = dist;
		quality = qual;
	}
}

public class OctreeSampling : MonoBehaviour {
	public Transform start = null;
	public float size = 1.0f;
	public int iterations = 3;
	public float threshold_success = 0.01f;
	public GameObject target = null;
	public GameObject TCP = null;
	public GameObject solutionParent = null;

	public bool drawSolutions = true;
	public bool drawFails = false;

	public string pointcloudFilename = "default.csv";
	public Material material = null;


	private int currentIteration = 0;
	private int currentCube = 0;
	private int currentParent = 0;
	private List<Vector3> parents;

	private Vector3 lastPosition;
	private List<ConfigurationPose> solutions;

	private bool done = false;


	public Mesh mesh;
	private int[] indices;
	private List<Vector3> vertices;
	private List<Color32> colors;

	private const bool SUCCESS = true;
	private const bool FAIL = false;

	private float currentDistance;
	private float currentQuality;

	float step;
	float cubeSize;
	// Use this for initialization
	void Start () {
		parents = new List<Vector3> ();
		lastPosition = start.position;
		cubeSize = size / Mathf.Pow (2, iterations);
		solutions = new List<ConfigurationPose> ();

		vertices = new List<Vector3> ();
		colors = new List<Color32> ();
		mesh = new Mesh ();
		//mesh.Clear ();
		//solutionParent.GetComponent<MeshRenderer>().
		material.shader = Shader.Find("Particles/Additive");
	}

	// Update is called once per frame
	void Update () {
		if (!done) {
			if (currentIteration <= iterations)
				NextCube ();
			else {
				WriteSolutionPointsToFile ();
				done = true;
			}			
		}
	}

	void FixedUpdate(){
		Graphics.DrawMesh(mesh, Vector3.zero, Quaternion.identity, material, 0);
	}

	// Calulates the next cube center
	void NextCube(){
		// if iteration not done
		if (currentCube < (Mathf.Pow (8, currentIteration))) {
			if (currentIteration != 0) {				
				target.transform.position = CalculateNextPosition (this.currentIteration, this.currentCube, this.parents[currentCube/8]); // todo next parent pos
			} else {
				target.transform.position = new Vector3 (start.position.x, start.position.y, start.position.z);
			}
			this.parents.Add (target.transform.position);

			GameObject cube;

			//Debug.Log (Vector3.Distance (target.transform.position, TCP.transform.position) + " targetX:" + target.transform.position.x + " targetY:" + target.transform.position.y + " targetZ:" + target.transform.position.z + " tcpX:" + TCP.transform.position.x + " tcpY:" + TCP.transform.position.y + " tcpZ:" + TCP.transform.position.z);
			currentDistance = Vector3.Distance (lastPosition, TCP.transform.position);
			if (currentDistance > threshold_success) {
				currentQuality = 0.0f; // zero is worst
				solutions.Add (new ConfigurationPose (lastPosition, FAIL, currentDistance, currentQuality));
				if (drawFails) {
					cube = GameObject.CreatePrimitive (PrimitiveType.Cube);
					cube.transform.localScale = new Vector3 (cubeSize, cubeSize, cubeSize);//(size / 2 * iterations, size / 2 * iterations, size / 2 * iterations);
					cube.transform.position = lastPosition;

					cube.GetComponent<Renderer> ().material.color = new Color (1.0f, .0f, .0f, .2f);
					cube.GetComponent<Renderer> ().material.SetInt ("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
					cube.GetComponent<Renderer> ().material.SetInt ("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
					cube.GetComponent<Renderer> ().material.SetInt ("_ZWrite", 0);
					cube.GetComponent<Renderer> ().material.DisableKeyword ("_ALPHATEST_ON");
					cube.GetComponent<Renderer> ().material.DisableKeyword ("_ALPHABLEND_ON");
					cube.GetComponent<Renderer> ().material.EnableKeyword ("_ALPHAPREMULTIPLY_ON");
					cube.GetComponent<Renderer> ().material.renderQueue = 3000;
				}
			} else {
				//normalize Distance to 0...1
				currentQuality = 1.0f - (currentDistance*1000) / (threshold_success*1000); // best is 1
				solutions.Add (new ConfigurationPose (lastPosition, SUCCESS, currentDistance, currentQuality));
				if (drawSolutions) {
					//					cube = GameObject.CreatePrimitive (PrimitiveType.Cube);
					//					cube.transform.localScale = new Vector3 (cubeSize, cubeSize, cubeSize);//(size / 2 * iterations, size / 2 * iterations, size / 2 * iterations);
					//					cube.transform.position = lastPosition;
					//					cube.transform.SetParent (solutionParent.transform);
					//					cube.GetComponent<Renderer> ().material.color = new Color (.0f, 1.0f, .0f);

					//extend and render the mesh
					vertices.Add (new Vector3 (lastPosition.x, lastPosition.y, lastPosition.z));
					mesh.vertices = vertices.ToArray();

					int[] indices = Enumerable.Range(0, mesh.vertices.Length).ToArray();
					mesh.SetIndices(indices, MeshTopology.Points,0);

					colors.Add (Color.green);
					mesh.colors32 = colors.ToArray();
					//					//Graphics.DrawMeshNow(mesh, Matrix4x4.TRS (Vector3.zero, Quaternion.identity, new Vector3 (1, 1, 1)), 0);
					//					Debug.Log(mesh.colors32.Length);
					//					Debug.Log(indices.Length);
					//					Debug.Log(mesh.vertices.Length);

					//solutionParent.GetComponent<MeshRenderer> ().material = material;
					//					if (mesh && material) {
					//						Graphics.DrawMeshNow (mesh, Vector3.zero, Quaternion.identity, 0);
					//					}
					//material.SetPass(1);
					//Graphics.DrawMeshNow(mesh, Vector3.zero, Quaternion.identity);
				}
				//solutions.Add (new Vector3 (lastPosition.x, lastPosition.y, lastPosition.z));
			}


			lastPosition = target.transform.position;
			currentCube++;
		} else { // next iteration is prepared
			if(currentIteration>0){ //remove parents, which are not needed anymore
				int max = (int)(Mathf.Pow (8, currentIteration-1));	
				//			foreach (Vector3 vec in parents){Debug.Log(vec);}
				for (int i = 0; i < max; i++) {
					parents.RemoveAt (0);
				}
				// todo remove old parents from the solutions
			}
			currentIteration++;
			currentCube = 0;
		}
	}




	//										1					0-7
	Vector3 CalculateNextPosition(int currentIteration, int currentCube, Vector3 parentPosition){
		step = size / Mathf.Pow (2, currentIteration);
		Vector3 pos = new Vector3 ();
		// oldParent +++ / ++- / +-+ / +-- / -++ / -+- / --+ / ---
		if(currentCube % 8 == 0){
			pos.x = parentPosition.x+step;
			pos.y = parentPosition.y+step;
			pos.z = parentPosition.z+step;
		}
		else if(currentCube % 8 == 1){
			pos.x = parentPosition.x+step;
			pos.y = parentPosition.y+step;
			pos.z = parentPosition.z-step;
		}
		else if(currentCube % 8 == 2){
			pos.x = parentPosition.x+step;
			pos.y = parentPosition.y-step;
			pos.z = parentPosition.z+step;
		}
		else if(currentCube % 8 == 3){
			pos.x = parentPosition.x+step;
			pos.y = parentPosition.y-step;
			pos.z = parentPosition.z-step;
		}
		else if(currentCube % 8 == 4){
			pos.x = parentPosition.x-step;
			pos.y = parentPosition.y+step;
			pos.z = parentPosition.z+step;
		}
		else if(currentCube % 8 == 5){
			pos.x = parentPosition.x-step;
			pos.y = parentPosition.y+step;
			pos.z = parentPosition.z-step;
		}
		else if(currentCube % 8 == 6){
			pos.x = parentPosition.x-step;
			pos.y = parentPosition.y-step;
			pos.z = parentPosition.z+step;
		}
		else if(currentCube % 8 == 7){
			pos.x = parentPosition.x-step;
			pos.y = parentPosition.y-step;
			pos.z = parentPosition.z-step;
		}

		//Debug.Log ("no:"+this.currentCube + " " + pos + "step:" + step + "parentIndex" + currentCube/8);
		return pos;
	}

	void WriteSolutionPointsToFile(){
		Debug.Log ("Writing File");
		foreach (ConfigurationPose conf in solutions) {
			File.AppendAllText (pointcloudFilename,		
				""
				+ conf.position.x
				+ ";"
				+ conf.position.y
				+ ";"
				+ conf.position.z
				+ ";"
				+ conf.success
				+ ";"
				+ conf.distance
				+ ";"
				+ conf.quality
				+ Environment.NewLine
			);
		}
	}


}
