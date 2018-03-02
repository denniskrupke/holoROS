using UnityEngine;
using System.Collections.Generic;

namespace BioIK {
	//OFKT - Optimised Forward Kinematics Tree (Data Structure)
	//The class models a linked list of segment nodes to efficiently update forward kinematics by feed-forward calculations.
	//The data structure significantly reduces the computational load for fitness evaluations in the evolutionary optimisation.
	//Repeating redundant calculations are avoided using additional computational storage, as configurations only slightly vary.
	public class Model {
		private Transform Root;										//Reference to Root Transform

		private double OPX, OPY, OPZ;								//Offset rosition to world frame
		private double ORX, ORY, ORZ, ORW;							//Offset rotation to world frame
		private double OSX, OSY, OSZ;								//Offset wcale to world Frame

		public Node[] Nodes = new Node[0];							//Array of nodes to model the linked list
		public MotionPtr[] MotionPtrs = new MotionPtr[0];			//Global pointers to the motions
		public ObjectivePtr[] ObjectivePtrs = new ObjectivePtr[0];	//Global pointers to the objectives

		//The constructor automatically creates the whole data structure, given an arbitrary root transform
		public Model(Transform root) {
			BuildModel(root);
			UpdateState();
		}

		//Updates all transformations of the model for the current frame
		public void UpdateState() {
			UpdateOffset();
			Nodes[0].UpdateTransformations();
		}

		//Assigns a joint variable configuration to the kinematic tree
		public void ApplyConfiguration(double[] configuration, Node startNode = null) {
			if(startNode == null) {
				Nodes[0].FeedForwardConfiguration(configuration);
			} else {
				startNode.FeedForwardConfiguration(configuration);
			}
		}

		//Returns a random joint variable configuration
		public double[] GetRandomConfiguration()  {
			double[] configuration = new double[MotionPtrs.Length];
			for(int i=0; i<configuration.Length; i++) {
				configuration[i] = Random.Range((float)MotionPtrs[i].Motion.GetLowerLimit(), (float)MotionPtrs[i].Motion.GetUpperLimit());
			}
			return configuration;
		}

		//Returns the target joint variable configuration
		public double[] GetTargetConfiguration()  {
			double[] configuration = new double[MotionPtrs.Length];
			for(int i=0; i<configuration.Length; i++) {
				configuration[i] = MotionPtrs[i].Motion.GetTargetValue();
			}
			return configuration;
		}

		//Updates the reference offset from world space to root
		private void UpdateOffset() {
			if(Root == Root.root) {
				OPX = OPY = OPZ = ORX = ORY = ORZ = 0.0;
				ORW = OSX = OSY = OSZ = 1.0;
			} else {
				Vector3 p = Root.parent.position;
				Quaternion r = Root.parent.rotation;
				Vector3 s = Root.parent.lossyScale;
				OPX = p.x; OPY = p.y; OPZ = p.z;
				ORX = r.x; ORY = r.y; ORZ = r.z; ORW = r.w;
				OSX = s.x; OSY = s.y; OSZ = s.z;
			}
		}

		//Automatically constructs the OFKT data structure
		private void BuildModel(Transform root) {
			Root = root;
			AddNode(Root);
			Objective[] objectives = FindObjectives(Root, new List<Objective>());
			for(int i=0; i<objectives.Length; i++) {
				Chain chain = new Chain(Root, objectives[i].transform);
				for(int j=1; j<chain.Segments.Length; j++) {
					AddNode(chain.Segments[j]);
				}
			}
			//Initialise containers for single transform modifications
			for(int i=0; i<Nodes.Length; i++) {
				Nodes[i].ObjectiveImpacts = new bool[ObjectivePtrs.Length];
				Nodes[i].PX = new double[ObjectivePtrs.Length];
				Nodes[i].PY = new double[ObjectivePtrs.Length];
				Nodes[i].PZ = new double[ObjectivePtrs.Length];
				Nodes[i].RX = new double[ObjectivePtrs.Length];
				Nodes[i].RY = new double[ObjectivePtrs.Length];
				Nodes[i].RZ = new double[ObjectivePtrs.Length];
				Nodes[i].RW = new double[ObjectivePtrs.Length];
			}
			//Assigns references to all objective nodes that are affected by a parenting node
			for(int i=0; i<ObjectivePtrs.Length; i++) {
				Node node = ObjectivePtrs[i].Node;
				while(node != null) {
					node.AddObjectiveNode(ObjectivePtrs[i].Node, i);
					node = node.Parent;
				}
			}
		}

		//Returns all objectives which are childs in the hierarcy, beginning from the root
		private Objective[] FindObjectives(Transform t, List<Objective> objectives) {
			Objective objective = t.GetComponent<Objective>();
			if(objective != null) {
				if(objective.isActiveAndEnabled) {
					objectives.Add(objective);
				}
			}
			for(int i=0; i<t.childCount; i++) {
				FindObjectives(t.GetChild(i), objectives);
			}
			return objectives.ToArray();
		}

		//Adds a segment node into the OFKT data structure
		private void AddNode(Transform segment) {
			if(FindNode(segment) == null) {
				KinematicJoint joint = segment.GetComponent<KinematicJoint>();
				Objective objective = segment.GetComponent<Objective>();

				Node node = new Node(this, FindNode(segment.parent), segment, joint, objective);

				if(joint != null) {
					if(joint.GetDoF() == 0) {
						joint = null;
					} else {
						if(joint.GetXMotion().IsEnabled()) {
							MotionPtr motionPtr = new MotionPtr(joint.GetXMotion(), node, MotionPtrs.Length);
							System.Array.Resize(ref MotionPtrs, MotionPtrs.Length+1);
							MotionPtrs[MotionPtrs.Length-1] = motionPtr;
							node.XEnabled = true;
							node.XIndex = motionPtr.Index;
						}
						if(joint.GetYMotion().IsEnabled()) {
							MotionPtr motionPtr = new MotionPtr(joint.GetYMotion(), node, MotionPtrs.Length);
							System.Array.Resize(ref MotionPtrs, MotionPtrs.Length+1);
							MotionPtrs[MotionPtrs.Length-1] = motionPtr;
							node.YEnabled = true;
							node.YIndex = motionPtr.Index;
						}
						if(joint.GetZMotion().IsEnabled()) {
							MotionPtr motionPtr = new MotionPtr(joint.GetZMotion(), node, MotionPtrs.Length);
							System.Array.Resize(ref MotionPtrs, MotionPtrs.Length+1);
							MotionPtrs[MotionPtrs.Length-1] = motionPtr;
							node.ZEnabled = true;
							node.ZIndex = motionPtr.Index;
						}
					}
				}

				if(objective != null) {
					System.Array.Resize(ref ObjectivePtrs, ObjectivePtrs.Length+1);
					ObjectivePtrs[ObjectivePtrs.Length-1] = new ObjectivePtr(segment.GetComponent<Objective>(), node);
				}

				System.Array.Resize(ref Nodes, Nodes.Length+1);
				Nodes[Nodes.Length-1] = node;
			}
		}

		//Returns a node in the data structure given a segment transform
		public Node FindNode(Transform segment) {
			return System.Array.Find(
				Nodes,
				node => node.Segment == segment
			);
		}

		//Returns the total DoF in the kinematic tree
		public int GetDoF() {
			return MotionPtrs.Length;
		}

		//Subclass representing tthe single nodes for the OFKT data structure.
		//Values are stored using primitive data types for faster access and efficient computation.
		public class Node {
			public Model Model;							//Reference to the kinematic model
			public Node Parent;							//Reference to the parent of this node
			public Node[] Childs = new Node[0];			//Reference to all child nodes
			public Transform Segment;					//Reference of the represented segment
			public Chain Chain;							//Kinematic chain from this node to the root
			public KinematicJoint Joint;				//Reference to the kinematic joint component (can be 'null')
			public Objective Objective;					//Reference to the objective component (can be 'null')

			public double HeuristicError = 0.0;			//Heuristic error for the memetic evolutionary exploitation (average over all affected objective losses)
			public Node[] ObjectiveNodes = new Node[0];	//References to all affected objective nodes
			public bool[] ObjectiveImpacts;				//Boolean values to represent which objective indices in the whole kinematic tree are affected (TODO: Refactor this)
			public double[] PX,PY,PZ,RX,RY,RZ,RW;		//Value containers to return single transform modifications without allocating new memory

			public double WPX, WPY, WPZ;				//World position
			public double WRX, WRY, WRZ, WRW;			//World rotation
			public double WSX, WSY, WSZ;				//World scale

			public double LPX, LPY, LPZ;				//Local position
			public double LRX, LRY, LRZ, LRW;			//Local rotation

			public double RootX, RootY, RootZ;			//World position of root joint

			public bool XEnabled = false;				//
			public int XIndex = -1;						//
			private double XValue = 0.0;				//
			public bool YEnabled = false;				//
			public int YIndex = -1;						//Some data storage for efficient feed-forward passes
			private double YValue = 0.0;				//
			public bool ZEnabled = false;				//
			public int ZIndex = -1;						//
			private double ZValue = 0.0;				//
			
			//Setup for the node
			public Node(Model model, Node parent, Transform segment, KinematicJoint joint, Objective objective) {
				Model = model;
				Parent = parent;
				if(Parent != null) {
					Parent.AddChild(this);
				}
				Segment = segment;
				Chain = new Chain(model.Root, segment);
				Joint = joint;
				Objective = objective;
			}

			//Adds a child to this node
			public void AddChild(Node child) {
				System.Array.Resize(ref Childs, Childs.Length+1);
				Childs[Childs.Length-1] = child;
			}

			//Adds an affected objective to this node
			public void AddObjectiveNode(Node objective, int i) {
				System.Array.Resize(ref ObjectiveNodes, ObjectiveNodes.Length+1);
				ObjectiveNodes[ObjectiveNodes.Length-1] = objective;
				ObjectiveImpacts[i] = true;
			}

			//Updates local and world transform, and feeds the joint variable configuration forward to all childs
			public void FeedForwardConfiguration(double[] configuration, bool updateWorld = false) {
				//Assume no local update is required
				bool updateLocal = false;

				if(XEnabled && configuration[XIndex] != XValue) {
					XValue = configuration[XIndex];
					updateLocal = true;
				}
				if(YEnabled && configuration[YIndex] != YValue) {
					YValue = configuration[YIndex];
					updateLocal = true;
				}
				if(ZEnabled && configuration[ZIndex] != ZValue) {
					ZValue = configuration[ZIndex];
					updateLocal = true;
				}
				
				//Only update local transformation if a joint value has changed
				if(updateLocal) {
					Joint.ComputeLocalTransformation(XValue, YValue, ZValue, out LPX, out LPY, out LPZ, out LRX, out LRY, out LRZ, out LRW);
					updateWorld = true;
				}

				//Only update world transformation if local transformation (in this or parent node) has changed
				if(updateWorld) {
					ComputeWorldTransformation();
				}

				//Feed forward the joint variable configuration
				foreach(Node child in Childs) {
					child.FeedForwardConfiguration(configuration, updateWorld);
				}
			}

			//Efficiently computes the world transformation using the current joint variable configuration
			private void ComputeWorldTransformation() {
				//WorldPosition = ParentPosition + ParentRotation*LocalPosition;
				//WorldRotation = ParentRotation*LocalRotation;
				double RX,RY,RZ,RW,X,Y,Z;
				if(Parent == null) {
					WPX = Model.OPX;
					WPY = Model.OPY;
					WPZ = Model.OPZ;
					RX = Model.ORX;
					RY = Model.ORY;
					RZ = Model.ORZ;
					RW = Model.ORW;
					X = Model.OSX*LPX;
					Y = Model.OSY*LPY;
					Z = Model.OSZ*LPZ;
				} else {
					WPX = Parent.WPX;
					WPY = Parent.WPY;
					WPZ = Parent.WPZ;
					RX = Parent.WRX;
					RY = Parent.WRY;
					RZ = Parent.WRZ;
					RW = Parent.WRW;
					X = Parent.WSX*LPX;
					Y = Parent.WSY*LPY;
					Z = Parent.WSZ*LPZ;
				}
				WPX += 2.0 * ((0.5 - RY * RY - RZ * RZ) * X + (RX * RY - RW * RZ) * Y + (RX * RZ + RW * RY) * Z);
				WPY += 2.0 * ((RX * RY + RW * RZ) * X + (0.5 - RX * RX - RZ * RZ) * Y + (RY * RZ - RW * RX) * Z);
				WPZ += 2.0 * ((RX * RZ - RW * RY) * X + (RY * RZ + RW * RX) * Y + (0.5 - RX * RX - RY * RY) * Z);
				WRX = RX * LRW + RY * LRZ - RZ * LRY + RW * LRX;
				WRY = -RX * LRZ + RY * LRW + RZ * LRX + RW * LRY;
				WRZ = RX * LRY - RY * LRX + RZ * LRW + RW * LRZ;
				WRW = -RX * LRX - RY * LRY - RZ * LRZ + RW * LRW;
			}

			//Compute the heuristic error by average-pooling over all affected objective errors
			public double PoolHeuristicError() {
				double error = 0.0;
				foreach(Node node in ObjectiveNodes) {
					error += node.HeuristicError;
				}
				return error / (double)ObjectiveNodes.Length;
			}

			//Recursively updates the transform information for the current frame
			public void UpdateTransformations() {
				//Update Root
				if(Chain.Joints.Length > 0) {
					Vector3 root = Chain.Joints[0].GetAnchorInWorldSpace();
					RootX = root.x;
					RootY = root.y;
					RootZ = root.z;
				} else {
					RootX = Model.OPX;
					RootY = Model.OPY;
					RootZ = Model.OPZ;
				}

				//Local
				if(Joint == null) {
					Vector3 lp = Segment.localPosition;
					Quaternion lr = Segment.localRotation;
					LPX = lp.x;
					LPY = lp.y;
					LPZ = lp.z;
					LRX = lr.x;
					LRY = lr.y;
					LRZ = lr.z;
					LRW = lr.w;
				} else {
					XValue = Joint.GetXMotion().GetTargetValue();
					YValue = Joint.GetYMotion().GetTargetValue();
					ZValue = Joint.GetZMotion().GetTargetValue();
					Joint.ComputeLocalTransformation(XValue, YValue, ZValue, out LPX, out LPY, out LPZ, out LRX, out LRY, out LRZ, out LRW);
				}
				Vector3 ws = Segment.lossyScale;
				WSX = ws.x;
				WSY = ws.y;
				WSZ = ws.z;

				//World
				ComputeWorldTransformation();

				//Feed Forward
				foreach(Node child in Childs) {
					child.UpdateTransformations();
				}
			}

			//Simulates a single transform modification while leaving the whole data structure unchanged
			//Returns the resulting Cartesian posture transformations in the out values
			public void SimulateModification(
				double[] configuration,
				out double[] px, out double[] py, out double[] pz, out double[] rx, out double[] ry, out double[] rz, out double[] rw
			) {
				px=PX; py=PY; pz=PZ; rx=RX; ry=RY; rz=RZ; rw=RW;
				for(int i=0; i<Model.ObjectivePtrs.Length; i++) {
					Node node = Model.ObjectivePtrs[i].Node;
					if(ObjectiveImpacts[i]) {
						//WorldPosition = ParentPosition + ParentRotation * (LocalPosition . ParentScale) + ParentRotation * LocalRotation * WorldRotation^-1 * (ObjectivePosition - WorldPosition)
						//WorldRotation = ParentRotation * LocalRotation * WorldRotation^-1 * ObjectiveRotation
						double lpX, lpY, lpZ, lrX, lrY, lrZ, lrW;
						Joint.ComputeLocalTransformation(
							XEnabled ? configuration[XIndex] : XValue,
							YEnabled ? configuration[YIndex] : YValue, 
							ZEnabled ? configuration[ZIndex] : ZValue, 
							out lpX, out lpY, out lpZ, out lrX, out lrY, out lrZ, out lrW
						);
						double Rx, Ry, Rz, Rw, X, Y, Z;
						if(Parent == null) {
							px[i] = Model.OPX;
							py[i] = Model.OPY;
							pz[i] = Model.OPZ;
							Rx = Model.ORX;
							Ry = Model.ORY;
							Rz = Model.ORZ;
							Rw = Model.ORW;
							X = Model.OSX*lpX;
							Y = Model.OSY*lpY;
							Z = Model.OSZ*lpZ;
						} else {
							px[i] = Parent.WPX;
							py[i] = Parent.WPY;
							pz[i] = Parent.WPZ;
							Rx = Parent.WRX;
							Ry = Parent.WRY;
							Rz = Parent.WRZ;
							Rw = Parent.WRW;
							X = Parent.WSX*lpX;
							Y = Parent.WSY*lpY;
							Z = Parent.WSZ*lpZ;
						}
						double qx = Rx * lrW + Ry * lrZ - Rz * lrY + Rw * lrX;
						double qy = -Rx * lrZ + Ry * lrW + Rz * lrX + Rw * lrY;
						double qz = Rx * lrY - Ry * lrX + Rz * lrW + Rw * lrZ;
						double qw = -Rx * lrX - Ry * lrY - Rz * lrZ + Rw * lrW;
						double dot = WRX*WRX + WRY*WRY + WRZ*WRZ + WRW*WRW;
						double x = qx/dot; double y = qy/dot; double z = qz/dot; double w = qw/dot;
						qx = x * WRW + y * -WRZ - z * -WRY + w * -WRX;
						qy = -x * -WRZ + y * WRW + z * -WRX + w * -WRY;
						qz = x * -WRY - y * -WRX + z * WRW + w * -WRZ;
						qw = -x * -WRX - y * -WRY - z * -WRZ + w * WRW;
						px[i] +=
								+ 2.0 * ((0.5 - Ry * Ry - Rz * Rz) * X + (Rx * Ry - Rw * Rz) * Y + (Rx * Rz + Rw * Ry) * Z)
								+ 2.0 * ((0.5 - qy * qy - qz * qz) * (node.WPX-WPX) + (qx * qy - qw * qz) * (node.WPY-WPY) + (qx * qz + qw * qy) * (node.WPZ-WPZ));
						py[i] += 
								+ 2.0 * ((Rx * Ry + Rw * Rz) * X + (0.5 - Rx * Rx - Rz * Rz) * Y + (Ry * Rz - Rw * Rx) * Z)
								+ 2.0 * ((qx * qy + qw * qz) * (node.WPX-WPX) + (0.5 - qx * qx - qz * qz) * (node.WPY-WPY) + (qy * qz - qw * qx) * (node.WPZ-WPZ));
						pz[i] += 
								+ 2.0 * ((Rx * Rz - Rw * Ry) * X + (Ry * Rz + Rw * Rx) * Y + (0.5 - (Rx * Rx + Ry * Ry)) * Z)
								+ 2.0 * ((qx * qz - qw * qy) * (node.WPX-WPX) + (qy * qz + qw * qx) * (node.WPY-WPY) + (0.5 - qx * qx - qy * qy) * (node.WPZ-WPZ));
						rx[i] = qx * node.WRW + qy * node.WRZ - qz * node.WRY + qw * node.WRX;
						ry[i] = -qx * node.WRZ + qy * node.WRW + qz * node.WRX + qw * node.WRY;
						rz[i] = qx * node.WRY - qy * node.WRX + qz * node.WRW + qw * node.WRZ;
						rw[i] = -qx * node.WRX - qy * node.WRY - qz * node.WRZ + qw * node.WRW;
					} else {
						px[i] = node.WPX;
						py[i] = node.WPY;
						pz[i] = node.WPZ;
						rx[i] = node.WRX;
						ry[i] = node.WRY;
						rz[i] = node.WRZ;
						rw[i] = node.WRW;
					}
				}
			}
		}

		//Data class to store pointers to the objectives
		public class ObjectivePtr {
			public Objective Objective;
			public Node Node;
			public ObjectivePtr(Objective objective, Node node) {
				Objective = objective;
				Node = node;
			}
		}

		//Data class to store pointers to the joint motions
		public class MotionPtr {
			public Motion Motion;
			public Node Node;
			public int Index;
			public MotionPtr(Motion motion, Node node, int index) {
				Motion = motion;
				Node = node;
				Index = index;
			}
		}
	}
}