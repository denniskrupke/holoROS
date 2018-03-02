using UnityEngine;
using UnityEditor;

namespace BioIK {
	[CustomEditor(typeof(KinematicJoint))]
	public class KinematicJoint_CE : Editor {
		private KinematicJoint Target;

		void Awake() {
			Target = (KinematicJoint)target;
		}

		void OnEnable() {
			Tools.hidden = true;
		}
		
		void OnDisable() {
			Tools.hidden = false;
		}

		public override void OnInspectorGUI() {
			Undo.RecordObject(Target, Target.name);
			
			using (var scope = new EditorGUILayout.VerticalScope ("Button")) {
				EditorGUILayout.HelpBox("Geometry", MessageType.None);
				Target.SetJointType((JointType)EditorGUILayout.EnumPopup("Joint Type", Target.GetJointType()));
				Target.SetAnchor(EditorGUILayout.Vector3Field("Anchor", Target.GetAnchor()));
				Target.SetOrientation(EditorGUILayout.Vector3Field("Orientation", Target.GetOrientation()));
			}

			using (var scope = new EditorGUILayout.VerticalScope ("Button")) {
				EditorGUILayout.HelpBox("Motion", MessageType.None);
				Target.SetMotionType((MotionType)EditorGUILayout.EnumPopup("Motion Type", Target.GetMotionType()));
				Target.SetSmoothing(EditorGUILayout.Slider("Smoothing", Target.GetSmoothing(), 0f, 1f));
				if(Target.GetMotionType() == MotionType.Realistic) {
					Target.SetMaximumVelocity(EditorGUILayout.FloatField("Max Velocity", Target.GetMaximumVelocity()));
					Target.SetMaximumAcceleration(EditorGUILayout.FloatField("Max Acceleration", Target.GetMaximumAcceleration()));
					if(Target.GetMaximumVelocity() == 0f || Target.GetMaximumAcceleration() == 0f) {
						EditorGUILayout.HelpBox("Velocity and Acceleration must be assigned, or nothing will move.", MessageType.Warning);
					}
				}

				DrawMotionInspector(Target.GetXMotion(), "X");
				DrawMotionInspector(Target.GetYMotion(), "Y");
				DrawMotionInspector(Target.GetZMotion(), "Z");
			}

			/*
			EditorGUILayout.HelpBox(
				"Current Value: " + Target.GetCurrentValue().ToString("F3") + "\n" +
				"Current Error: " + Target.GetCurrentError().ToString("F3") + "\n" +
				"Current Velocity: " + Target.GetCurrentVelocity().ToString("F3") + "\n" + 
				"Current Acceleration: " + Target.GetCurrentAcceleration().ToString("F3"), MessageType.None);
				*/

			EditorUtility.SetDirty(Target);

			/*
			using (var scope = new EditorGUILayout.VerticalScope ("Button")) {
				EditorGUILayout.HelpBox("Debug", MessageType.None);
				EditorGUILayout.Vector3Field("Anchor", Target.GetAnchor());
				EditorGUILayout.Vector3Field("World Anchor", Target.GetAnchorInWorldSpace());
				EditorGUILayout.Vector3Field("Orientation", Target.GetOrientation());
				EditorGUILayout.Vector3Field("X Axis", Target.GetXMotion().Axis);
				EditorGUILayout.Vector3Field("Y Axis", Target.GetYMotion().Axis);
				EditorGUILayout.Vector3Field("Z Axis", Target.GetZMotion().Axis);
				EditorGUILayout.Vector3Field("Default Reference Position", Target.GetDefaultReferencePosition());
				EditorGUILayout.Vector3Field("Default Reference Rotation", Target.GetDefaultReferenceRotation().eulerAngles);
			}
			*/
		}
		
		private void DrawMotionInspector(Motion motion, string name) {
			using (var scope = new EditorGUILayout.VerticalScope ("Box")) {
				EditorGUILayout.HelpBox(name, MessageType.None);
				motion.SetEnabled(EditorGUILayout.Toggle("Enabled", motion.IsEnabled()));
				if(motion.IsEnabled()) {
					if(motion.Joint.GetJointType() != JointType.Continuous) {
						motion.SetLowerLimit(EditorGUILayout.FloatField("Lower Limit", motion.GetLowerLimit()));
						motion.SetUpperLimit(EditorGUILayout.FloatField("Upper Limit", motion.GetUpperLimit()));
					}
					motion.SetTargetValue(EditorGUILayout.Slider("Target Value", motion.GetTargetValue(), motion.GetLowerLimit(), motion.GetUpperLimit()));
				}
			}
		}

		void OnSceneGUI() {
			//Draw Anchor
			Vector3 anchor = Target.GetAnchorInWorldSpace();
			Handles.color = Color.magenta;
			Handles.SphereCap(0, anchor, Quaternion.identity, 1/100f);
			Handles.Label(anchor, "Anchor");

			//Draw Axes
			Quaternion rotation = Target.transform.rotation;

			if(Target.GetXMotion().IsEnabled()) {
				Handles.color = new Color(1f, 0f, 0f, 0.2f);
				Vector3 scale = Vector3.zero;
				if(Target.transform.root != Target.transform) {
					scale = Target.transform.parent.lossyScale;
				}

				if(Target.GetJointType() == JointType.Prismatic) {
					Vector3 pivot = anchor - Vector3.Scale(rotation * Quaternion.Euler(Target.GetOrientation()) * new Vector3(Target.GetXMotion().GetCurrentValue(), Target.GetYMotion().GetCurrentValue(), Target.GetZMotion().GetCurrentValue()), scale);
					Vector3 A = pivot + Vector3.Scale(Target.GetXMotion().GetLowerLimit() * (rotation * Target.GetXMotion().Axis), scale);
					Vector3 B = pivot + Vector3.Scale(Target.GetXMotion().GetUpperLimit() * (rotation * Target.GetXMotion().Axis), scale);
					Handles.DrawLine(anchor, A);
					Handles.CubeCap(0, A, rotation, 0.025f);
					Handles.DrawLine(anchor, B);
					Handles.CubeCap(0, B, rotation, 0.025f);
				} else {
					float lowerLimit = Mathf.Rad2Deg*Target.GetXMotion().GetLowerLimit();
					float upperLimit = Mathf.Rad2Deg*Target.GetXMotion().GetUpperLimit();
					Handles.DrawSolidArc(anchor, rotation * Target.GetXMotion().Axis, Quaternion.AngleAxis(lowerLimit, rotation * Target.GetXMotion().Axis) * rotation * Target.GetZMotion().Axis, upperLimit-lowerLimit, 0.2f);
				}
				Handles.color = Color.red;
			} else {
				Handles.color = Color.grey;
			}
			Handles.ArrowCap(0, anchor, rotation * Quaternion.LookRotation(Target.GetXMotion().Axis), 0.25f);

			if(Target.GetYMotion().IsEnabled()) {
				Handles.color = new Color(0f, 1f, 0f, 0.2f);
				Vector3 scale = Vector3.zero;
				if(Target.transform.root != Target.transform) {
					scale = Target.transform.parent.lossyScale;
				}

				if(Target.GetJointType() == JointType.Prismatic) {
					Vector3 pivot = anchor - Vector3.Scale(rotation * Quaternion.Euler(Target.GetOrientation()) *  new Vector3(Target.GetXMotion().GetCurrentValue(), Target.GetYMotion().GetCurrentValue(), Target.GetZMotion().GetCurrentValue()), scale);
					Vector3 A = pivot + Vector3.Scale(Target.GetYMotion().GetLowerLimit() * (rotation * Target.GetYMotion().Axis), scale);
					Vector3 B = pivot + Vector3.Scale(Target.GetYMotion().GetLowerLimit() * (rotation * Target.GetYMotion().Axis), scale);
					Handles.DrawLine(anchor, A);
					Handles.CubeCap(0, A, rotation, 0.025f);
					Handles.DrawLine(anchor, B);
					Handles.CubeCap(0, B, rotation, 0.025f);
				} else {
					float lowerLimit = Mathf.Rad2Deg*Target.GetYMotion().GetLowerLimit();
					float upperLimit = Mathf.Rad2Deg*Target.GetYMotion().GetUpperLimit();
					Handles.DrawSolidArc(anchor, rotation * Target.GetYMotion().Axis, Quaternion.AngleAxis(lowerLimit, rotation * Target.GetYMotion().Axis) * rotation * Target.GetXMotion().Axis, upperLimit-lowerLimit, 0.2f);
				}
				Handles.color = Color.green;
			} else {
				Handles.color = Color.grey;
			}
			Handles.ArrowCap(0, anchor, rotation * Quaternion.LookRotation(Target.GetYMotion().Axis), 0.25f);

			if(Target.GetZMotion().IsEnabled()) {
				Handles.color = new Color(0f, 0f, 1f, 0.2f);
				Vector3 scale = Vector3.zero;
				if(Target.transform.root != Target.transform) {
					scale = Target.transform.parent.lossyScale;
				}

				if(Target.GetJointType() == JointType.Prismatic) {
					Vector3 pivot = anchor - Vector3.Scale(rotation * Quaternion.Euler(Target.GetOrientation()) * new Vector3(Target.GetXMotion().GetCurrentValue(), Target.GetYMotion().GetCurrentValue(), Target.GetZMotion().GetCurrentValue()), scale);
					Vector3 A = pivot + Vector3.Scale(Target.GetZMotion().GetLowerLimit() * (rotation * Target.GetZMotion().Axis), scale);
					Vector3 B = pivot + Vector3.Scale(Target.GetZMotion().GetUpperLimit() * (rotation * Target.GetZMotion().Axis), scale);
					Handles.DrawLine(anchor, A);
					Handles.CubeCap(0, A, rotation, 0.025f);
					Handles.DrawLine(anchor, B);
					Handles.CubeCap(0, B, rotation, 0.025f);
				} else {
					float lowerLimit = Mathf.Rad2Deg*Target.GetZMotion().GetLowerLimit();
					float upperLimit = Mathf.Rad2Deg*Target.GetZMotion().GetUpperLimit();
					Handles.DrawSolidArc(anchor, rotation * Target.GetZMotion().Axis, Quaternion.AngleAxis(lowerLimit, rotation * Target.GetZMotion().Axis) * rotation * Target.GetYMotion().Axis, upperLimit-lowerLimit, 0.2f);
				}
				Handles.color = Color.blue;
			} else {
				Handles.color = Color.grey;
			}
			Handles.ArrowCap(0, anchor, rotation * Quaternion.LookRotation(Target.GetZMotion().Axis), 0.25f);
		}
	}
}