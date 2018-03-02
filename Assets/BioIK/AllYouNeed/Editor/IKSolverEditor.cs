using UnityEngine;
using UnityEditor;

namespace BioIK {
	[CustomEditor(typeof(IKSolver))]
	public class IKSolverEditor : Editor {

		public bool ShowGeometry = true;
		public Color LineColor = Color.cyan;
		public float LineWidth = 5f;
		public Color RootColor = Color.black;
		public float RootSize = 0.015f;
		public Color JointColor = Color.magenta;
		public float JointSize = 0.03f;
		public Color TipColor = Color.black;
		public float TipSize = 0.015f;
		public Color SegmentColor = Color.gray;
		public float SegmentSize = 0.015f;
		public float ArrowSize = 0.1f;

		private IKSolver Target;
		private Model Model;

		void Awake() {
			Target = (IKSolver)target;
			Model = new Model(Target.transform);
		}

		public override void OnInspectorGUI() {
			Undo.RecordObject(Target, Target.name);

			if(Model == null) {
				Model = new Model(Target.transform);
			}

			//Show DoF
			using (var degreeoffreedom = new EditorGUILayout.VerticalScope ("Button")) {
				EditorGUILayout.LabelField("Degree of Freedom: " + Model.GetDoF());
			}

			//Show Solver Settings
			using (var scope = new EditorGUILayout.VerticalScope ("Button")) {
				EditorGUILayout.HelpBox("Solver", MessageType.None);

				Target.MaximumFrameTime = EditorGUILayout.DoubleField("Maximum Frame Time", Target.MaximumFrameTime);
				Target.SetPopulationSize(EditorGUILayout.IntField("Population Size", Target.PopulationSize));
				Target.SetElites(EditorGUILayout.IntField("Elites", Target.Elites));
			}

			//Show IK Targets
			using (var scope = new EditorGUILayout.VerticalScope ("Button")) {
				EditorGUILayout.HelpBox("Objective Weights", MessageType.None);

				for(int i=0; i<Model.ObjectivePtrs.Length; i++) {
					Undo.RecordObject(Model.ObjectivePtrs[i].Objective, Model.ObjectivePtrs[i].Objective.name);

					using (var box = new EditorGUILayout.VerticalScope ("Box")) {
						EditorGUILayout.LabelField(Model.ObjectivePtrs[i].Objective.name);
						Model.ObjectivePtrs[i].Objective.Weight = EditorGUILayout.Slider("Weight", (float)Model.ObjectivePtrs[i].Objective.Weight, 0f, 1f);;
					}

					EditorUtility.SetDirty(Model.ObjectivePtrs[i].Objective);
				}
			}

			//Execute In Edit Mode
			using (var executeineditmode = new EditorGUILayout.VerticalScope ("Button")) {
				Target.ExecuteInEditMode = EditorGUILayout.Toggle("Execute In Edit Mode: ", Target.ExecuteInEditMode);
				if(GUILayout.Button("Reset Posture")) {
					ResetPosture(Target.transform);
				}
			}

			//Performance
			using (var degreeoffreedom = new EditorGUILayout.VerticalScope ("Button")) {
				EditorGUILayout.HelpBox("Performance", MessageType.None);
				EditorGUILayout.LabelField("Generations: " + Target.GetElapsedGenerations());
				EditorGUILayout.LabelField("Elapsed Time: " + Target.GetElapsedTime());
			}

			/*
			//Visualization
			using (var visualization = new EditorGUILayout.VerticalScope ("Button")) {
				EditorGUILayout.HelpBox("Visualization", MessageType.None);
				ShowGeometry = EditorGUILayout.Toggle("Show Geometry", ShowGeometry);
				LineColor = EditorGUILayout.ColorField("Line Color", LineColor);
				LineWidth = EditorGUILayout.FloatField("Line Width", LineWidth);
				RootColor = EditorGUILayout.ColorField("Root Color", RootColor);
				RootSize = EditorGUILayout.FloatField("Root Size", RootSize);
				JointColor = EditorGUILayout.ColorField("Joint Color", JointColor);
				JointSize = EditorGUILayout.FloatField("Joint Size", JointSize);
				TipColor = EditorGUILayout.ColorField("Tip Color", TipColor);
				TipSize = EditorGUILayout.FloatField("Tip Size", TipSize);
				SegmentColor = EditorGUILayout.ColorField("Segment Color", SegmentColor);
				SegmentSize = EditorGUILayout.FloatField("Segment Size", SegmentSize);
				ArrowSize = EditorGUILayout.FloatField("Arrow Size", ArrowSize);
			}
			*/

			EditorUtility.SetDirty(Target);
		}

		private void ResetPosture(Transform t) {
			KinematicJoint joint = t.GetComponent<KinematicJoint>();
			if(joint != null) {
				joint.GetXMotion().SetTargetValue(0f);
				joint.GetYMotion().SetTargetValue(0f);
				joint.GetZMotion().SetTargetValue(0f);
			}
			for(int i=0; i<t.childCount; i++) {
				ResetPosture(t.GetChild(i));
			}
		}

		public virtual void OnSceneGUI() {
			if(Model == null) {
				Model = new Model(Target.transform);
			}

			if(ShowGeometry) {
				DrawGeometry(Target.transform, null);
				DrawModel(Model.Nodes[0]);
			}
		}

		private void DrawModel(Model.Node node) {
			DrawSphere(node.Segment.position, SegmentSize, SegmentColor);
			//if(node.Parent != null) {
			//	DrawLine(node.Parent.Segment.position, node.Segment.position, LineColor);
			//} else {
			//	DrawSphere(node.Segment.position, RootSize, RootColor);
			//}
			if(node.Joint != null) {
				DrawJoint(node.Joint);
			}
			if(node.Objective != null) {
				DrawSphere(node.Objective.transform.position, TipSize, TipColor);
			}

			for(int i=0; i<node.Childs.Length; i++) {
				DrawModel(node.Childs[i]);
			}
		}

		private void DrawGeometry(Transform node, Transform parent) {
			DrawSphere(node.position, SegmentSize, SegmentColor);
			if(parent != null) {
				DrawLine(parent.position, node.position, LineColor);
			} else {
				DrawSphere(node.position, RootSize, RootColor);
			}
			//KinematicJoint joint = node.GetComponent<KinematicJoint>();
			//if(joint != null) {
			//	DrawLine(node.position, joint.ComputeConnectionInWorldSpace(), JointColor);
			//	DrawJoint(joint);
			//}
			//IKTip tip = node.GetComponent<IKTip>();
			//if(tip != null) {
			//	DrawSphere(tip.transform.position, TipSize, TipColor);
			//}

			for(int i=0; i<node.childCount; i++) {
				DrawGeometry(node.GetChild(i), node);
			}
		}
		
		private void DrawJoint(KinematicJoint joint) {
			Vector3 connection = joint.GetAnchorInWorldSpace();
			//DrawSphere(connection, JointSize, JointColor);
			DrawCube(connection, joint.transform.rotation * Quaternion.Euler(joint.GetOrientation()), JointSize, JointColor);
			DrawLine(joint.transform.position, joint.GetAnchorInWorldSpace(), JointColor);

			//GUIStyle style = new GUIStyle();
			//style.normal.textColor = Color.black;
			//Handles.Label(connection, joint.name, style);

			if(joint.GetXMotion().IsEnabled()) {
				Handles.color = Color.red;
				Handles.ArrowCap(0, connection, joint.transform.rotation * Quaternion.LookRotation(joint.GetXMotion().Axis), ArrowSize);
			}
			if(joint.GetYMotion().IsEnabled()) {
				Handles.color = Color.green;
				Handles.ArrowCap(0, connection, joint.transform.rotation * Quaternion.LookRotation(joint.GetYMotion().Axis), ArrowSize);
			}
			if(joint.GetZMotion().IsEnabled()) {
				Handles.color = Color.blue;
				Handles.ArrowCap(0, connection, joint.transform.rotation * Quaternion.LookRotation(joint.GetZMotion().Axis), ArrowSize);
			}
		}

		private void DrawSphere(Vector3 position, float radius, Color color) {
			Handles.color = color;
			Handles.SphereCap(0, position, Quaternion.identity, radius);
		}

		private void DrawCube(Vector3 position, Quaternion rotation, float size, Color color) {
			Handles.color = color;
			Handles.CubeCap(0, position, rotation, size);
		}

		private void DrawLine(Vector3 a, Vector3 b, Color color) {
			Handles.color = color;
			Handles.DrawAAPolyLine(LineWidth, new Vector3[2] {a,b});
		}
	}
}