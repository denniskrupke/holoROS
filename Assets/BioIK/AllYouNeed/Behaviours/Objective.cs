using UnityEngine;

namespace BioIK {
	//Objective to specify kinematic postures.
	[DisallowMultipleComponent]
	[ExecuteInEditMode]
	public class Objective : MonoBehaviour {

		//Target specification
		public Transform Target;
		public ObjectiveType Type = ObjectiveType.Position;		//The optimization objective
		public double MaximumPositionError = 0.01; 				//In metres
		public double MaximumOrientationError = 0.01; 			//In radians
		public double MaximumLookAtError = 0.01; 				//In radians
		public Vector3 Direction = Vector3.forward;				//Looking direction
		public double Weight = 1.0;								//Should always be between 0 and 1

		//Save for more calculating transformations multiple times more efficiently
		private double TPX;
		private double TPY;
		private double TPZ;
		private double TRX;
		private double TRY;
		private double TRZ;
		private double TRW;

		private const double PI = 3.14159265358979;

		void Awake() {
			UpdateTarget();

			Refresh();
		}

		void Update() {
			UpdateTarget();
		}

		void OnEnable() {
			Refresh();
		}

		void OnDisable() {
			Refresh();
		}

		void OnDestroy() {
			Refresh();
		}

		public void SetTarget(Transform target, ObjectiveType type) {
			Target = target;
		}

		public void SetTarget(Vector3 position) {
			Target = null;
			TPX = position.x;
			TPY = position.y;
			TPZ = position.z;
			Type = ObjectiveType.Position;
		}

		public void SetTarget(Quaternion rotation) {
			Target = null;
			TRX = rotation.x;
			TRY = rotation.y;
			TRZ = rotation.z;
			TRW = rotation.w;
			Type = ObjectiveType.Orientation;
		}

		public void SetTarget(Vector3 position, Quaternion rotation) {
			Target = null;
			TPX = position.x;
			TPY = position.y;
			TPZ = position.z;
			TRX = rotation.x;
			TRY = rotation.y;
			TRZ = rotation.z;
			TRW = rotation.w;
			Type = ObjectiveType.Pose;
		}

		public void SetTarget(Vector3 position, Vector3 direction) {
			Target = null;
			TPX = position.x;
			TPY = position.y;
			TPZ = position.z;
			Direction = direction;
			Type = ObjectiveType.LookAt;
		}

		public void UpdateTarget() {
			if(Target != null) {
				Vector3 targetPosition = Target.position;
				TPX = targetPosition.x;
				TPY = targetPosition.y;
				TPZ = targetPosition.z;

				Quaternion targetRotation = Target.rotation;
				TRX = targetRotation.x;
				TRY = targetRotation.y;
				TRZ = targetRotation.z;
				TRW = targetRotation.w;
			}
		}

		public double ComputeLoss(double WPX, double WPY, double WPZ, double WRX, double WRY, double WRZ, double WRW, Model.Node node, bool balanced) {
			double d,s;
			switch(Type) {
				case ObjectiveType.Position:
					d = System.Math.Sqrt((TPX-WPX)*(TPX-WPX) + (TPY-WPY)*(TPY-WPY) + (TPZ-WPZ)*(TPZ-WPZ));
					s = System.Math.Sqrt((node.Chain.Length+d)*(System.Math.Sqrt((WPX-node.RootX)*(WPX-node.RootX) + (WPY-node.RootY)*(WPY-node.RootY) + (WPZ-node.RootZ)*(WPZ-node.RootZ))+d));
					return PI * d / s;
				
				case ObjectiveType.Orientation:
					d = WRX*TRX + WRY*TRY + WRZ*TRZ + WRW*TRW;
					if(d < 0.0) {
						d = -d;
					}
					if(d > 1.0) {
						d = 1.0;
					}
					return 2.0 * System.Math.Acos(d);

				case ObjectiveType.Pose:
					d = System.Math.Sqrt((TPX-WPX)*(TPX-WPX) + (TPY-WPY)*(TPY-WPY) + (TPZ-WPZ)*(TPZ-WPZ));
					s = System.Math.Sqrt((node.Chain.Length+d)*(System.Math.Sqrt((WPX-node.RootX)*(WPX-node.RootX) + (WPY-node.RootY)*(WPY-node.RootY) + (WPZ-node.RootZ)*(WPZ-node.RootZ))+d));
					double PE = PI * d / s;
					double OE = WRX*TRX + WRY*TRY + WRZ*TRZ + WRW*TRW;
					if(OE < 0.0) {
						OE = -OE;
					}
					if(OE > 1.0) {
						OE = 1.0;
					}
					OE = 2.0 * System.Math.Acos(OE);
					double weight = balanced ? 0.5 : Random.value;
					return weight * PE + (1.0-weight) * OE;
			
				case ObjectiveType.LookAt:
					double aX = 2.0 * ((0.5 - (WRY * WRY + WRZ * WRZ)) * Direction.x + (WRX * WRY - WRW * WRZ) * Direction.y + (WRX * WRZ + WRW * WRY) * Direction.z);
					double aY = 2.0 * ((WRX * WRY + WRW * WRZ) * Direction.x + (0.5 - (WRX * WRX + WRZ * WRZ)) * Direction.y + (WRY * WRZ - WRW * WRX) * Direction.z);
					double aZ = 2.0 * ((WRX * WRZ - WRW * WRY) * Direction.x + (WRY * WRZ + WRW * WRX) * Direction.y + (0.5 - (WRX * WRX + WRY * WRY)) * Direction.z);
					double bX = TPX-WPX;
					double bY = TPY-WPY;
					double bZ = TPZ-WPZ;
					double dot = aX*bX + aY*bY + aZ*bZ;
					double len = System.Math.Sqrt(aX*aX + aY*aY + aZ*aZ) * System.Math.Sqrt(bX*bX + bY*bY + bZ*bZ);
					double arg = dot/len;
					if(arg > 1.0) {
						arg = 1.0;
					} else if(arg < -1.0) {
						arg = -1.0;
					}
					return System.Math.Acos(arg);
			}
			return 0.0;
		}

		public bool CheckConvergence(double WPX, double WPY, double WPZ, double WRX, double WRY, double WRZ, double WRW) {
			switch(Type) {
				case ObjectiveType.Position:
					return System.Math.Sqrt((TPX-WPX)*(TPX-WPX) + (TPY-WPY)*(TPY-WPY) + (TPZ-WPZ)*(TPZ-WPZ)) <= MaximumPositionError;
				
				case ObjectiveType.Orientation:
					double d = WRX*TRX + WRY*TRY + WRZ*TRZ + WRW*TRW;
					if(d < 0.0) {
						d = -d;
					}
					if(d > 1.0) {
						d = 1.0;
					}
					return 2.0 * System.Math.Acos(d) <= MaximumOrientationError;

				case ObjectiveType.Pose:
					double PE = System.Math.Sqrt((TPX-WPX)*(TPX-WPX) + (TPY-WPY)*(TPY-WPY) + (TPZ-WPZ)*(TPZ-WPZ));
					double OE = WRX*TRX + WRY*TRY + WRZ*TRZ + WRW*TRW;
					if(OE < 0.0) {
						OE = -OE;
					}
					if(OE > 1.0) {
						OE = 1.0;
					}
					OE = 2.0 * System.Math.Acos(OE);
					return PE <= MaximumPositionError && OE <= MaximumOrientationError;
			
				case ObjectiveType.LookAt:
					double aX = 2.0 * ((0.5 - (WRY * WRY + WRZ * WRZ)) * Direction.x + (WRX * WRY - WRW * WRZ) * Direction.y + (WRX * WRZ + WRW * WRY) * Direction.z);
					double aY = 2.0 * ((WRX * WRY + WRW * WRZ) * Direction.x + (0.5 - (WRX * WRX + WRZ * WRZ)) * Direction.y + (WRY * WRZ - WRW * WRX) * Direction.z);
					double aZ = 2.0 * ((WRX * WRZ - WRW * WRY) * Direction.x + (WRY * WRZ + WRW * WRX) * Direction.y + (0.5 - (WRX * WRX + WRY * WRY)) * Direction.z);
					double bX = TPX-WPX;
					double bY = TPY-WPY;
					double bZ = TPZ-WPZ;
					double dot = aX*bX + aY*bY + aZ*bZ;
					double len = System.Math.Sqrt(aX*aX + aY*aY + aZ*aZ) * System.Math.Sqrt(bX*bX + bY*bY + bZ*bZ);
					double arg = dot/len;
					if(arg > 1.0) {
						arg = 1.0;
					} else if(arg < -1.0) {
						arg = -1.0;
					}
					return System.Math.Acos(arg) <= MaximumLookAtError;
			}
			return true;
		}

		public double ComputeTranslationalDistance(double WPX, double WPY, double WPZ) {
			//Euclidean Distance: ||A-B||
			return System.Math.Sqrt((TPX-WPX)*(TPX-WPX) + (TPY-WPY)*(TPY-WPY) + (TPZ-WPZ)*(TPZ-WPZ));
		}

		public double ComputeRotationalDistance(double WRX, double WRY, double WRZ, double WRW) {
			//Quaternion Angle: 2*ACos(|AxB|)
			double d = WRX*TRX + WRY*TRY + WRZ*TRZ + WRW*TRW;
			if(d < 0.0) {
				d = -d;
			}
			if(d > 1.0) {
				d = 1.0;
			}
			return 2.0 * System.Math.Acos(d);
		}

		public double ComputeDirectionalDistance(double WPX, double WPY, double WPZ, double WRX, double WRY, double WRZ, double WRW) {
			//Vector Angle: Acos(Dot/Length)
			double aX = 2.0 * ((0.5 - (WRY * WRY + WRZ * WRZ)) * Direction.x + (WRX * WRY - WRW * WRZ) * Direction.y + (WRX * WRZ + WRW * WRY) * Direction.z);
			double aY = 2.0 * ((WRX * WRY + WRW * WRZ) * Direction.x + (0.5 - (WRX * WRX + WRZ * WRZ)) * Direction.y + (WRY * WRZ - WRW * WRX) * Direction.z);
			double aZ = 2.0 * ((WRX * WRZ - WRW * WRY) * Direction.x + (WRY * WRZ + WRW * WRX) * Direction.y + (0.5 - (WRX * WRX + WRY * WRY)) * Direction.z);
			double bX = TPX-WPX;
			double bY = TPY-WPY;
			double bZ = TPZ-WPZ;
			double dot = aX*bX + aY*bY + aZ*bZ;
			double len = System.Math.Sqrt(aX*aX + aY*aY + aZ*aZ) * System.Math.Sqrt(bX*bX + bY*bY + bZ*bZ);
			double arg = dot/len;
			if(arg > 1.0) {
				arg = 1.0;
			} else if(arg < -1.0) {
				arg = -1.0;
			}
			return System.Math.Acos(arg);
		}

		public double ComputeAngularScale(double Length) {
			return Length / PI;
		}

		public double ComputeAngularScale(double WPX, double WPY, double WPZ, double RootX, double RootY, double RootZ, double Length) {
			return System.Math.Sqrt(Length*System.Math.Sqrt((WPX-RootX)*(WPX-RootX) + (WPY-RootY)*(WPY-RootY) + (WPZ-RootZ)*(WPZ-RootZ))) / PI;
		}

		private void Refresh() {
			IKSolver solver = SearchIKSolver();
			if(solver != null) {
				solver.Rebuild();
			}
		}

		private IKSolver SearchIKSolver() {
			Transform t = transform;
			while(true) {
				IKSolver solver = t.GetComponent<IKSolver>();
				if(solver != null) {
					return solver;
				} else if(t != t.root) {
					t = t.parent;
				} else {
					return null;
				}
			}
		}
	}
}