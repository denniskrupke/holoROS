using UnityEngine;

namespace BioIK {
	//Kinematic joint to specify the 3D-motion of a segment.
	[DisallowMultipleComponent]
	[ExecuteInEditMode]
	public class KinematicJoint : MonoBehaviour {
		[SerializeField] private JointType JointType = JointType.Revolute;			//Type of the joint
		[SerializeField] private Vector3 Anchor = Vector3.zero;						//Joint anchor
		[SerializeField] private Vector3 Orientation = Vector3.zero;				//Joint orientation
		[SerializeField] private MotionType MotionType = MotionType.Instantaneous;	//Type of the applied motion
		[SerializeField] private Motion XMotion = new Motion(Vector3.right);		//X motion object
		[SerializeField] private Motion YMotion = new Motion(Vector3.up);			//Y motion object
		[SerializeField] private Motion ZMotion = new Motion(Vector3.forward);		//Z motion object
		[SerializeField] private float MaximumVelocity = 1f;						//Maximum joint velocity
		[SerializeField] private float MaximumAcceleration = 1f;					//Maximum joint acceleration
		[SerializeField] private float Smoothing = 0.5f;							//Factor to smooth Cartesian motion

		[SerializeField] private bool Initialised = false;							//The values below are serialised
		[SerializeField] private float DPX, DPY, DPZ, DRX, DRY, DRZ, DRW;			//Default frame
		[SerializeField] private double AX, AY, AZ;									//Scaled anchor
		[SerializeField] private double DPDRAX, DPDRAY, DPDRAZ;						//Transformed scaled anchor in default frame
		[SerializeField] private double r1, r2, r3, r4, r5, r6, r7, r8, r9;

		private Vector3 LastPosition;
		private Quaternion LastRotation;
		private const float PI = 3.14159265358979f;

		void Awake() {
			transform.hideFlags = HideFlags.NotEditable;
			if(!Initialised) {
				Initialise();
			}
			LastPosition = GetDefaultPosition();
			LastRotation = GetDefaultRotation();
		}
		
		public void Initialise() {
			XMotion.Joint = this;
			YMotion.Joint = this;
			ZMotion.Joint = this;
			UpdateReferenceFrame();
			SetJointType(JointType);
			SetMotionType(MotionType);
			SetAnchor(Anchor);
			SetOrientation(Orientation);
			Initialised = true;
		}

		private void UpdateReferenceFrame() {
			DPX = transform.localPosition.x;
			DPY = transform.localPosition.y;
			DPZ = transform.localPosition.z;
			DRX = transform.localRotation.x;
			DRY = transform.localRotation.y;
			DRZ = transform.localRotation.z;
			DRW = transform.localRotation.w;

			r1 = (1.0 - 2.0 * (DRY * DRY + DRZ * DRZ));
			r2 = 2.0 * (DRX * DRY + DRW * DRZ);
			r3 = 2.0 * (DRX * DRZ - DRW * DRY);
			r4 = 2.0 * (DRX * DRY - DRW * DRZ);
			r5 = (1.0 - 2.0 * (DRX * DRX + DRZ * DRZ));
			r6 = 2.0 * (DRY * DRZ + DRW * DRX);
			r7 = 2.0 * (DRX * DRZ + DRW * DRY);
			r8 = 2.0 * (DRY * DRZ - DRW * DRX);
			r9 = (1.0 - 2.0 * (DRX * DRX + DRY * DRY));

			PrecomputeAnchor();
		}

		void LateUpdate() {
			//Compute local transformation
			double lpX, lpY, lpZ, lrX, lrY, lrZ, lrW;
			ComputeLocalTransformation(XMotion.UpdateMotion(), YMotion.UpdateMotion(), ZMotion.UpdateMotion(), out lpX, out lpY, out lpZ, out lrX, out lrY, out lrZ, out lrW);

			//Apply local transformation
			if(Application.isPlaying) {
				transform.localPosition = (1f-Smoothing) * new Vector3((float)lpX, (float)lpY, (float)lpZ) + Smoothing * LastPosition;
				transform.localRotation = Quaternion.Slerp(new Quaternion((float)lrX, (float)lrY, (float)lrZ, (float)lrW), LastRotation, Smoothing);
			} else {
				transform.localPosition = new Vector3((float)lpX, (float)lpY, (float)lpZ);
				transform.localRotation = new Quaternion((float)lrX, (float)lrY, (float)lrZ, (float)lrW);
			}

			//Remember transformation
			LastPosition = transform.localPosition;
			LastRotation = transform.localRotation;
		}

		public void AssignTargetValues() {
			double lpX, lpY, lpZ, lrX, lrY, lrZ, lrW;
			ComputeLocalTransformation(XMotion.GetTargetValue(), YMotion.GetTargetValue(), ZMotion.GetTargetValue(), out lpX, out lpY, out lpZ, out lrX, out lrY, out lrZ, out lrW);
			transform.localPosition = new Vector3((float)lpX, (float)lpY, (float)lpZ);
			transform.localRotation = new Quaternion((float)lrX, (float)lrY, (float)lrZ, (float)lrW);
		}

		//Fast implementation to compute the local transform given the joint values
		public void ComputeLocalTransformation(double valueX, double valueY, double valueZ, out double lpX, out double lpY, out double lpZ, out double lrX, out double lrY, out double lrZ, out double lrW) {
			if(JointType == JointType.Prismatic) {
				double x = valueX * XMotion.Axis.x + valueY * YMotion.Axis.x + valueZ * ZMotion.Axis.x;
				double y = valueX * XMotion.Axis.y + valueY * YMotion.Axis.y + valueZ * ZMotion.Axis.y;
				double z = valueX * XMotion.Axis.z + valueY * YMotion.Axis.z + valueZ * ZMotion.Axis.z;
				lpX = DPX + r1 * x + r4 * y + r7 * z;
				lpY = DPY + r2 * x + r5 * y + r8 * z;
				lpZ = DPZ + r3 * x + r6 * y + r9 * z;
				lrX = DRX;
				lrY = DRY;
				lrZ = DRZ;
				lrW = DRW;
			} else {
				//Z-X-Y Order
				double sin, x1, y1, z1, w1, x2, y2, z2, w2, qx, qy, qz, qw = 0.0;
				if(valueZ != 0.0) {
					sin = System.Math.Sin(valueZ/2.0);
					qx = ZMotion.Axis.x * sin;
					qy = ZMotion.Axis.y * sin;
					qz = ZMotion.Axis.z * sin;
					qw = System.Math.Cos(valueZ/2.0);
					if(valueX != 0.0) {
						sin = System.Math.Sin(valueX/2.0);
						x1 = XMotion.Axis.x * sin;
						y1 = XMotion.Axis.y * sin;
						z1 = XMotion.Axis.z * sin;
						w1 = System.Math.Cos(valueX/2.0);
						x2 = qx; y2 = qy; z2 = qz; w2 = qw;
						qx = x1 * w2 + y1 * z2 - z1 * y2 + w1 * x2;
						qy = -x1 * z2 + y1 * w2 + z1 * x2 + w1 * y2;
						qz = x1 * y2 - y1 * x2 + z1 * w2 + w1 * z2;
						qw = -x1 * x2 - y1 * y2 - z1 * z2 + w1 * w2;
						if(valueY != 0.0) {
							sin = System.Math.Sin(valueY/2.0);
							x1 = YMotion.Axis.x * sin;
							y1 = YMotion.Axis.y * sin;
							z1 = YMotion.Axis.z * sin;
							w1 = System.Math.Cos(valueY/2.0);
							x2 = qx; y2 = qy; z2 = qz; w2 = qw;
							qx = x1 * w2 + y1 * z2 - z1 * y2 + w1 * x2;
							qy = -x1 * z2 + y1 * w2 + z1 * x2 + w1 * y2;
							qz = x1 * y2 - y1 * x2 + z1 * w2 + w1 * z2;
							qw = -x1 * x2 - y1 * y2 - z1 * z2 + w1 * w2;
						} else {
						}
					} else if(valueY != 0.0) {
						sin = System.Math.Sin(valueY/2.0);
						x1 = YMotion.Axis.x * sin;
						y1 = YMotion.Axis.y * sin;
						z1 = YMotion.Axis.z * sin;
						w1 = System.Math.Cos(valueY/2.0);
						x2 = qx; y2 = qy; z2 = qz; w2 = qw;
						qx = x1 * w2 + y1 * z2 - z1 * y2 + w1 * x2;
						qy = -x1 * z2 + y1 * w2 + z1 * x2 + w1 * y2;
						qz = x1 * y2 - y1 * x2 + z1 * w2 + w1 * z2;
						qw = -x1 * x2 - y1 * y2 - z1 * z2 + w1 * w2;
					} else {
					}
				} else if(valueX != 0.0) {
					sin = System.Math.Sin(valueX/2.0);
					qx = XMotion.Axis.x * sin;
					qy = XMotion.Axis.y * sin;
					qz = XMotion.Axis.z * sin;
					qw = System.Math.Cos(valueX/2.0);
					if(valueY != 0.0) {
						sin = System.Math.Sin(valueY/2.0);
						x1 = YMotion.Axis.x * sin;
						y1 = YMotion.Axis.y * sin;
						z1 = YMotion.Axis.z * sin;
						w1 = System.Math.Cos(valueY/2.0);
						x2 = qx; y2 = qy; z2 = qz; w2 = qw;
						qx = x1 * w2 + y1 * z2 - z1 * y2 + w1 * x2;
						qy = -x1 * z2 + y1 * w2 + z1 * x2 + w1 * y2;
						qz = x1 * y2 - y1 * x2 + z1 * w2 + w1 * z2;
						qw = -x1 * x2 - y1 * y2 - z1 * z2 + w1 * w2;
					} else {
					}
				} else if(valueY != 0.0) {
					sin = System.Math.Sin(valueY/2.0);
					qx = YMotion.Axis.x * sin;
					qy = YMotion.Axis.y * sin;
					qz = YMotion.Axis.z * sin;
					qw = System.Math.Cos(valueY/2.0);
				} else {
					lpX = DPX;
					lpY = DPY;
					lpZ = DPZ;
					lrX = DRX;
					lrY = DRY;
					lrZ = DRZ;
					lrW = DRW;
					return;
				}

				//Local Rotation
				//R' = R*Q
				lrX = DRX * qw + DRY * qz - DRZ * qy + DRW * qx;
				lrY = -DRX * qz + DRY * qw + DRZ * qx + DRW * qy;
				lrZ = DRX * qy - DRY * qx + DRZ * qw + DRW * qz;
				lrW = -DRX * qx - DRY * qy - DRZ * qz + DRW * qw;

				//Local Position
				if(AX == 0.0 && AY == 0.0 && AZ == 0.0) {
					//P' = Pz
					lpX = DPX;
					lpY = DPY;
					lpZ = DPZ;
				} else {
					//P' = P + RA + R*Q*(-A)
					lpX = DPDRAX + 2.0 * ((0.5 - lrY * lrY - lrZ * lrZ) * -AX + (lrX * lrY - lrW * lrZ) * -AY + (lrX * lrZ + lrW * lrY) * -AZ);
					lpY = DPDRAY + 2.0 * ((lrX * lrY + lrW * lrZ) * -AX + (0.5 - lrX * lrX - lrZ * lrZ) * -AY + (lrY * lrZ - lrW * lrX) * -AZ);
					lpZ = DPDRAZ + 2.0 * ((lrX * lrZ - lrW * lrY) * -AX + (lrY * lrZ + lrW * lrX) * -AY + (0.5 - lrX * lrX - lrY * lrY) * -AZ);
				}
			}
		}

		public int GetDoF() {
			int dof = 0;
			if(XMotion.IsEnabled()) {
				dof += 1;
			}
			if(YMotion.IsEnabled()) {
				dof += 1;
			}
			if(ZMotion.IsEnabled()) {
				dof += 1;
			}
			return dof;
		}

		public void SetJointType(JointType type) {
			if(type == JointType.Continuous) {
				XMotion.SetLowerLimit(0f);
				XMotion.SetUpperLimit(0f);
				YMotion.SetLowerLimit(0f);
				YMotion.SetUpperLimit(0f);
				ZMotion.SetLowerLimit(0f);
				ZMotion.SetUpperLimit(0f);
			}
			JointType = type;
			if(type == JointType.Continuous) {
				XMotion.SetLowerLimit(-PI);
				XMotion.SetUpperLimit(PI);
				YMotion.SetLowerLimit(-PI);
				YMotion.SetUpperLimit(PI);
				ZMotion.SetLowerLimit(-PI);
				ZMotion.SetUpperLimit(PI);
			}
		}

		public JointType GetJointType() {
			return JointType;
		}


		public void SetMotionType(MotionType type) {
			MotionType = type;
		}

		public MotionType GetMotionType() {
			return MotionType;
		}

		public Motion GetXMotion() {
			return XMotion;
		}

		public Motion GetYMotion() {
			return YMotion;
		}

		public Motion GetZMotion() {
			return ZMotion;
		}

		public void SetSmoothing(float value) {
			Smoothing = Mathf.Clamp(value, 0f, 1f);
		}

		public float GetSmoothing() {
			return Smoothing;
		}

		public void SetMaximumVelocity(float value) {
			MaximumVelocity = Mathf.Max(0f, value);
		}

		public float GetMaximumVelocity() {
			return MaximumVelocity;
		}

		public void SetMaximumAcceleration(float value) {
			MaximumAcceleration = Mathf.Max(0f, value);
		}

		public float GetMaximumAcceleration() {
			return MaximumAcceleration;
		}

		public Vector3 GetDefaultPosition() {
			return new Vector3(DPX, DPY, DPZ);
		}

		public Quaternion GetDefaultRotation() {
			return new Quaternion(DRX, DRY, DRZ, DRW);
		}

		public void SetAnchor(Vector3 anchor) {
			Anchor = anchor;
			PrecomputeAnchor();
		}

		public Vector3 GetAnchor() {
			return Anchor;
		}

		public double GetScaledAnchorX() {
			return AX;
		}

		public double GetScaledAnchorY() {
			return AY;
		}

		public double GetScaledAnchorZ() {
			return AZ;
		}

		private void PrecomputeAnchor() {
			AX = Anchor.x * transform.localScale.x;	AY = Anchor.y * transform.localScale.y;	AZ = Anchor.z * transform.localScale.z;
			DPDRAX = DPX + 2.0 * ((0.5 - DRY * DRY - DRZ * DRZ) * AX + (DRX * DRY - DRW * DRZ) * AY + (DRX * DRZ + DRW * DRY) * AZ);
			DPDRAY = DPY + 2.0 * ((DRX * DRY + DRW * DRZ) * AX + (0.5 - DRX * DRX - DRZ * DRZ) * AY + (DRX * DRZ - DRW * DRX) * AZ);
			DPDRAZ = DPZ + 2.0 * ((DRX * DRZ - DRW * DRY) * AX + (DRY * DRZ + DRW * DRX) * AY + (0.5 - DRX * DRX - DRY * DRY) * AZ);
		}

		public Vector3 GetAnchorInWorldSpace() {
			return transform.position + transform.rotation * new Vector3((float)AX, (float)AY, (float)AZ);
		}

		public void SetOrientation(Vector3 orientation) {
			Orientation = orientation;
			Quaternion o = Quaternion.Euler(Orientation);
			XMotion.Axis = o * Vector3.right;
			YMotion.Axis = o * Vector3.up;
			ZMotion.Axis = o * Vector3.forward;
		}

		public Vector3 GetOrientation() {
			return Orientation;
		}

		void OnDestroy() {
			transform.hideFlags = HideFlags.None;
			transform.localPosition = new Vector3(DPX, DPY, DPZ);
			transform.localRotation = new Quaternion(DRX, DRY, DRZ, DRW);
		}
	}
}