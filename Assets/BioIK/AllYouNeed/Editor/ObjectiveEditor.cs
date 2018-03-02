using UnityEngine;
using UnityEditor;

namespace BioIK {
	[CustomEditor(typeof(Objective))]
	public class Objective_CE : Editor {
		private Objective Target;

		void Awake() {
			Target = (Objective)target;
		}

		public override void OnInspectorGUI() {
			Undo.RecordObject(Target, Target.name);

			using (var scope = new EditorGUILayout.VerticalScope ("Button")) {
				EditorGUILayout.HelpBox("Settings", MessageType.None);

				Target.Target = (Transform)EditorGUILayout.ObjectField("Target", Target.Target, typeof(Transform), true);
				Target.Type = (ObjectiveType)EditorGUILayout.EnumPopup("Objective", Target.Type);

				using (var error = new EditorGUILayout.VerticalScope ("Box")) {
					EditorGUILayout.LabelField("Maximum Error");
					if(Target.Type == ObjectiveType.Position) {
						Target.MaximumPositionError = EditorGUILayout.FloatField("Position", (float)Target.MaximumPositionError);
					}
					if(Target.Type == ObjectiveType.Orientation) {
						Target.MaximumOrientationError = EditorGUILayout.FloatField("Orientation", (float)Target.MaximumOrientationError);
					}
					if(Target.Type == ObjectiveType.Pose) {
						Target.MaximumPositionError = EditorGUILayout.FloatField("Position", (float)Target.MaximumPositionError);
						Target.MaximumOrientationError = EditorGUILayout.FloatField("Orientation", (float)Target.MaximumOrientationError);
					}
					if(Target.Type == ObjectiveType.LookAt) {
						Target.Direction = EditorGUILayout.Vector3Field("Direction", Target.Direction);
						Target.MaximumLookAtError = EditorGUILayout.FloatField("Error", (float)Target.MaximumLookAtError);
					}
				}
			}

			//Chain chain = new Chain(Target.transform.root, Target.transform);
			//EditorGUILayout.LabelField("Length", chain.Length.ToString());

			EditorUtility.SetDirty(Target);
		}

		public virtual void OnSceneGUI() {
			//Draw Objective
			if(Target.Type == ObjectiveType.Position) {
				Handles.SphereCap(0, Target.transform.position, Quaternion.identity, (float)Target.MaximumPositionError);
			}
			if(Target.Type == ObjectiveType.Orientation) {
				//Visualized by Unity's Transform
			}
			if(Target.Type == ObjectiveType.Pose) {
				Handles.SphereCap(0, Target.transform.position, Quaternion.identity, (float)Target.MaximumPositionError);
			}
			if(Target.Type == ObjectiveType.LookAt) {
				Handles.color = Color.magenta;
				Handles.ArrowCap(0, Target.transform.position, Target.transform.rotation*Quaternion.LookRotation(Target.Direction), 0.25f);
			}
		}
	}
}