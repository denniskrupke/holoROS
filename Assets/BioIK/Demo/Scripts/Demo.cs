using UnityEngine;

public class Demo : MonoBehaviour {

	public GameObject[] Models;

	public BioIK.MotionType MotionType = BioIK.MotionType.Instantaneous;

	private GameObject Model = null;

	void Start() {
		LoadModel(1);
	}

	public void LoadModel(int index) {
		index -= 1;
		if(Models.Length >= index) {
			if(Models[index] != null) {
				if(Model != null) {
					Model.SetActive(false);
				}
				Model = Models[index];
				Model.SetActive(true);
				AssignMotionType(Model.transform, Model.name == "Robot Kyle Full Body" ? BioIK.MotionType.Instantaneous : MotionType);
			}
		}
	}

	public void SetMotionType(int number) {
		if(number == 0) {
			MotionType = BioIK.MotionType.Instantaneous;
		}
		if(number == 1) {
			MotionType = BioIK.MotionType.Realistic;
		}
		if(Model.name == "Robot Kyle Full Body") {
			MotionType = BioIK.MotionType.Instantaneous;
		}
		AssignMotionType(Model.transform, MotionType);
	}

	private void AssignMotionType(Transform t, BioIK.MotionType motionType) {
		if(t.GetComponent<BioIK.KinematicJoint>() != null) {
			t.GetComponent<BioIK.KinematicJoint>().SetMotionType(motionType);
		}
		for(int i=0; i<t.childCount; i++) {
			AssignMotionType(t.GetChild(i), motionType);
		}
	}

}
