using UnityEngine;

namespace BioIK {
	[DisallowMultipleComponent]
	[ExecuteInEditMode]
	public class IKSolver : MonoBehaviour {

		public bool ExecuteInEditMode = false;

		//Algorithm parameters
		public double MaximumFrameTime = 0.001; //Specify the maximum allowed time for optimization during one frame
		public int PopulationSize = 15;			//Should be much higher than the number of elites
		public int Elites = 3;					//Should be chosen comparatively low. Lower values cause more exploitation (smooth trajectory), higher values improve exploration (success rate).

		//Optimization algorithm
		private Model Model;
		private Evolution Evolution;

		private int Generations;
		private double ElapsedTime;
		private double IterationTime;

		private bool RequireReset;

		void Awake() {
			Initialise(PopulationSize, Elites);
		}

		void Update() {
			if(!ExecuteInEditMode && !Application.isPlaying) {
				return;
			}

			if(RequireReset || Model == null) {
				Initialise(PopulationSize, Elites);
			} else {
				Model.UpdateState();
				Generations = 0;
				ElapsedTime = 0.0;
				IterationTime = 0.0;
			}

			while(ElapsedTime < MaximumFrameTime && !IsConverged()) {
				Iterate();
			}
			Assign(Evolution.GetSolution());
		}

		public void Initialise(int populationSize, int elites) {
			Model = new Model(transform);
			Evolution = new Evolution(Model, populationSize, elites, Model.GetTargetConfiguration());
			Generations = 0;
			ElapsedTime = 0.0;
			IterationTime = 0.0;
			RequireReset = false;
		}

		public void Iterate() {
			System.DateTime then = System.DateTime.Now;
			Evolution.Evolve();
			IterationTime = (System.DateTime.Now-then).Duration().TotalSeconds;
			ElapsedTime += IterationTime;
			Generations += 1;
		}

		public void SetPopulationSize(int value) {
			if(PopulationSize != value) {
				PopulationSize = Mathf.Max(1, value);
				Elites = Mathf.Min(PopulationSize, Elites);
				Rebuild();
			}
		}

		public void SetElites(int value) {
			if(Elites != value) {
				Elites = Mathf.Min(PopulationSize, value);
				Rebuild();
			}
		}

		public void Rebuild() {
			RequireReset = true;
		}

		public Model GetModel() {
			return Model;
		}

		public Evolution GetEvolution() {
			return Evolution;
		}

		public int GetElapsedGenerations() {
			return Generations;
		}

		public double GetElapsedTime() {
			return ElapsedTime;
		}

		public double GetIterationTime() {
			return IterationTime;
		}

		public void Assign(double[] configuration) {
			for(int i=0; i<configuration.Length; i++) {
				if(Model.MotionPtrs[i].Motion.Joint.GetJointType() == JointType.Revolute) {
					Model.MotionPtrs[i].Motion.SetTargetValue((float)configuration[i]);
				} else if(Model.MotionPtrs[i].Motion.Joint.GetJointType() == JointType.Continuous) {
					Model.MotionPtrs[i].Motion.SetTargetValue(Model.MotionPtrs[i].Motion.GetTargetValue() + Mathf.Deg2Rad*Mathf.DeltaAngle(Mathf.Rad2Deg*Model.MotionPtrs[i].Motion.GetTargetValue(), Mathf.Rad2Deg*(float)configuration[i]));
				} else if(Model.MotionPtrs[i].Motion.Joint.GetJointType() == JointType.Prismatic) {
					Model.MotionPtrs[i].Motion.SetTargetValue((float)configuration[i]);
				}
			}
		}

		public bool IsConverged() {
			return Evolution.IsConverged();
		}
	}
}