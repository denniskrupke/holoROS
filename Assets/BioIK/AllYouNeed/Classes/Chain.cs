using UnityEngine;
using System.Collections.Generic;

namespace BioIK {
	public class Chain {
		public Transform[] Segments;
		public KinematicJoint[] Joints;
		public float Length;

		public Chain(Transform start, Transform end) {
			List<Transform> segments = new List<Transform>();
			List<KinematicJoint> joints = new List<KinematicJoint>();
			Length = 0f;

			Transform t = end;
			while(true) {
				segments.Add(t);
				KinematicJoint joint = t.GetComponent<KinematicJoint>();
				if(joint != null) {
					if(joint.GetDoF() != 0) {
						joints.Add(joint);
					}
				}
				if(t == start) {
					break;
				} else {
					t = t.parent;
				}
			} 
			
			segments.Reverse();
			joints.Reverse();
			Segments = segments.ToArray();
			Joints = joints.ToArray();

			if(Joints.Length == 0) {
				Length = 0f;
			} else {
				for(int i=1; i<Joints.Length; i++) {
					Length += Vector3.Distance(Joints[i-1].GetAnchorInWorldSpace(), Joints[i].GetAnchorInWorldSpace());
				}
				Length += Vector3.Distance(Joints[Joints.Length-1].GetAnchorInWorldSpace(), end.position);
			}
		}
	}
}