using UnityEngine;
using System.Collections.Generic;

namespace BioIK {
	//----------------------------------------------------------------------------------------------------
	//====================================================================================================
	//Hybrid Genetic Swarm Algorithm (HGSA)
	//The algorithm operates in-place in order to require zero further data allocation.
	//====================================================================================================
	//----------------------------------------------------------------------------------------------------
	public class Evolution {		
		private Model Model;									//Reference to the kinematic model
		private Motion[] Motions;								//Reference to all joint motions
		private int Size;										//Number of individuals (population size)
		private int Elites;										//Number of elite individuals
		private int Dimensionality;								//Search space dimensionality

		private double[] Solution;								//Global evolutionary solution
		private double Fitness;									//Global evolutionary fitness

		private Individual[] Population;						//Array for current individuals
		private Individual[] Offspring;							//Array for offspring individuals

		private List<Individual> Pool = new List<Individual>();	//Selection pool for recombination
		private int PoolCount;									//Current size of the selection pool
		private double[] Probabilities;							//Current probabilities for selection
		private double Storage;									//Simple storage variable
		private double[] LowerBounds;							//Constraints for the lower bounds
		private double[] UpperBounds;							//Constraints for the upper bounds
		private bool[] UseBounds;								//Whether bounds can be exceeded or not

		private const double PI = 3.14159265358979;

		//Initialises the HGSA
		public Evolution(Model model, int size, int elites, double[] seed) {
			Model = model;
			Size = size;
			Elites = elites;
			Dimensionality = Model.MotionPtrs.Length;
			Motions = new Motion[Dimensionality];
			for(int i=0; i<Dimensionality; i++) {
				Motions[i] = model.MotionPtrs[i].Motion;
			}

			Storage = 0.0;
			LowerBounds = new double[Dimensionality];
			UpperBounds = new double[Dimensionality];
			UseBounds = new bool[Dimensionality];
			UpdateBounds();

			Population = new Individual[Size];
			Offspring = new Individual[Size];

			for(int i=0; i<Size; i++) {
				Population[i] = new Individual(Dimensionality);
				Offspring[i] = new Individual(Dimensionality);
			}

			Solution = seed;
			Probabilities = new double[Size];

			Initialise();
		}

		//Initialises a new population and integrates the evolutionary solution (seed)
		private bool Initialise() {
			for(int i=0; i<Dimensionality; i++) {
				Population[0].Genes[i] = Solution[i];
				Population[0].Gradient[i] = 0.0;
			}
			Population[0].Fitness = ComputeFitness(Population[0].Genes, true);

			for(int i=1; i<Size; i++) {
				for(int j=0; j<Dimensionality; j++) {
					Population[i].Genes[j] = (float)Random.Range((float)LowerBounds[j], (float)UpperBounds[j]);
					Population[i].Gradient[j] = 0.0;
				}
				Population[i].Fitness = ComputeFitness(Population[i].Genes, true);
			}

			SortByFitness();
			ComputeExtinctions();

			return TryUpdateSolution();
		}

		//Returns whether the solution could be improved
		public bool Evolve() {
			//Update search space bounds
			UpdateBounds();

			//Let elites survive (memetic exploitation)
			for(int i=0; i<Elites; i++) {
				Survive(i);
			}

			//Create mating pool
			Pool.Clear();
			Pool.AddRange(Population);
			PoolCount = Size;

			//Evolve offspring
			for(int i=Elites; i<Size; i++) {
				if(PoolCount > 0) {
					Individual parentA = Select(Pool);
					Individual parentB = Select(Pool);
					Individual prototype = Select(Pool);

					//Recombination, Mutation, Adoption
					Reproduce(i, parentA, parentB, prototype);

					//Pre-Selection Niching
					if(Offspring[i].Fitness < parentA.Fitness) {
						Pool.Remove(parentA);
						PoolCount -= 1;
					}
					if(Offspring[i].Fitness < parentB.Fitness) {
						Pool.Remove(parentB);
						PoolCount -= 1;
					}
				} else {
					//Fill the population
					Reroll(i);
				}
			}

			//Swap population and offspring
			Swap(ref Population, ref Offspring);

			//Finalise
			SortByFitness();
			
			//Check improvement and wipeout criterion
			if(!TryUpdateSolution()) {
				if(!Exploit(Population[0], true)) {
					return Initialise();
				} else {
					ComputeExtinctions();
					return TryUpdateSolution();
				}
			} else {
				ComputeExtinctions();
				return true;
			}
		}

		//Returns whether all objectives are satisfied by the evolution
		public bool IsConverged() {
			Model.ApplyConfiguration(Solution);
			for(int i=0; i<Model.ObjectivePtrs.Length; i++) {
				Model.Node node = Model.ObjectivePtrs[i].Node;
				if(!Model.ObjectivePtrs[i].Objective.CheckConvergence(node.WPX, node.WPY, node.WPZ, node.WRX, node.WRY, node.WRZ, node.WRW)) {
					return false;
				}
			}
			return true;
		}

		//Tries to improve the evolutionary solution by the population, and returns whether it was successful
		private bool TryUpdateSolution() {
			Fitness = ComputeFitness(Solution, true);
			double candidateFitness = ComputeFitness(Population[0].Genes, true);
			if(candidateFitness < Fitness) {
				for(int i=0; i<Dimensionality; i++) {
					Solution[i] = Population[0].Genes[i];
				}
				Fitness = candidateFitness;
				return true;
			} else {
				return false;
			}
		}

		//Lets elite individuals survive and performs a memetic exploitation
		private void Survive(int index) {
			Individual survivor = Population[index];
			Individual offspring = Offspring[index];

			for(int i=0; i<Dimensionality; i++) {
				offspring.Genes[i] = survivor.Genes[i];
				offspring.Gradient[i] = survivor.Gradient[i];
			}

			Exploit(offspring, false);
		}

		//Evolves a new individual
		private void Reproduce(int index, Individual parentA, Individual parentB, Individual prototype) {
			Individual offspring = Offspring[index];
			double weight;
			double mutationProbability = GetMutationProbability(parentA, parentB);
			double mutationStrength = GetMutationStrength(parentA, parentB);

			for(int i=0; i<Dimensionality; i++) {
				//Recombination
				weight = Random.value;
				double gradient = Random.value*parentA.Gradient[i] + Random.value*parentB.Gradient[i];
				offspring.Genes[i] = weight*parentA.Genes[i] + (1-weight)*parentB.Genes[i] + gradient;

				//Store
				Storage = offspring.Genes[i];

				//Mutation
				if(Random.value < mutationProbability) {
					float span = (float)(UpperBounds[i] - LowerBounds[i]);
					offspring.Genes[i] += Random.Range(-span, span) * mutationStrength;
				}

				//Adoption
				weight = Random.value;
				offspring.Genes[i] += 
					weight * Random.value * (0.5 * (parentA.Genes[i] + parentB.Genes[i]) - offspring.Genes[i])
					+ (1-weight) * Random.value * (prototype.Genes[i] - offspring.Genes[i]);

				//Clip
				offspring.Genes[i] = Constrain(offspring.Genes[i], LowerBounds[i], UpperBounds[i], UseBounds[i]);

				//Evolutionary Gradient
				offspring.Gradient[i] = Random.value*gradient + (offspring.Genes[i] - Storage);
			}

			//Fitness
			offspring.Fitness = ComputeFitness(offspring.Genes, false);
		}

		//Generates a random individual
		private void Reroll(int index) {
			Individual offspring = Offspring[index];
			for(int i=0; i<Dimensionality; i++) {
				offspring.Genes[i] = Random.Range((float)LowerBounds[i], (float)UpperBounds[i]);
				offspring.Gradient[i] = 0.0;
			}
			offspring.Fitness = ComputeFitness(offspring.Genes, false);
		}

		//Rank-based selection of an individual
		private Individual Select(List<Individual> pool) {
			double rankSum = PoolCount*(PoolCount+1) / 2.0;
			for(int i=0; i<PoolCount; i++) {
				Probabilities[i] = (PoolCount-i)/rankSum;
			}
			return pool[GetRandomWeightedIndex(Probabilities, PoolCount)];
		}
		
		//Returns a random index with respect to the probability weights
		private int GetRandomWeightedIndex(double[] probabilities, int count) {
			double weightSum = 0.0;
			for(int i=0; i<count; i++) {
				weightSum += probabilities[i];
			}
			double rVal = Random.value*weightSum;
			for(int i=0; i<count; i++) {
				rVal -= probabilities[i];
				if(rVal <= 0.0) {
					return i;
				}
			}
			return count-1;
		}

		//Returns the mutation probability from two parents
		private double GetMutationProbability(Individual parentA, Individual parentB) {
			double extinction = 0.5 * (parentA.Extinction + parentB.Extinction);
			double inverse = 1.0/(double)Dimensionality;
			return extinction * (1.0-inverse) + inverse;
		}

		//Returns the mutation strength from two parents
		private double GetMutationStrength(Individual parentA, Individual parentB) {
			return 0.5 * (parentA.Extinction + parentB.Extinction);
		}

		//Computes the extinction factors for all individuals
		private void ComputeExtinctions() {
			double min = Population[0].Fitness;
			double max = Population[Size-1].Fitness;
			for(int i=0; i<Size; i++) {
				double grading = (double)i/((double)Size-1);
				Population[i].Extinction = (Population[i].Fitness + min*(grading-1.0)) / max;
			}
		}

		//Performs the memetic exploitation
		private bool Exploit(Individual individual, bool balanced) {
			double fitnessSum = 0;
			bool improved = false;
			bool update = true;
			for(int i=0; i<Dimensionality; i++) {
				double fitness = ComputeFitness(individual.Genes, balanced, Model.MotionPtrs[i].Node.Parent, update);
				update = false;

				double heuristicError = Model.MotionPtrs[i].Node.PoolHeuristicError();
				double gene = individual.Genes[i];

				double inc = Constrain(gene + Random.value*heuristicError, LowerBounds[i], UpperBounds[i], UseBounds[i]);
				individual.Genes[i] = inc;
				double incFitness = ComputeFitness(individual.Genes, balanced, Model.MotionPtrs[i]);

				double dec = Constrain(gene - Random.value*heuristicError, LowerBounds[i], UpperBounds[i], UseBounds[i]);
				individual.Genes[i] = dec;
				double decFitness = ComputeFitness(individual.Genes, balanced, Model.MotionPtrs[i]);

				if(incFitness <= decFitness && incFitness < fitness) {
					individual.Genes[i] = inc;
					individual.Gradient[i] = Random.value*individual.Gradient[i] + inc - gene;
					fitnessSum += incFitness;
					improved = true;
					update = true;
				} else if(decFitness <= incFitness && decFitness < fitness) {
					individual.Genes[i] = dec;
					individual.Gradient[i] = Random.value*individual.Gradient[i] + dec - gene;
					fitnessSum += decFitness;
					improved = true;
					update = true;
				} else {
					individual.Genes[i] = gene;
					fitnessSum += fitness;
				}
			}
			individual.Fitness = fitnessSum/(double)Dimensionality;
			return improved;
		}

		//Updates the lower and upper search space bounds
		private void UpdateBounds() {
			for(int i=0; i<Dimensionality; i++) {
				LowerBounds[i] = Motions[i].GetLowerLimit();
				UpperBounds[i] = Motions[i].GetUpperLimit();
				UseBounds[i] = Motions[i].Joint.GetJointType() == JointType.Continuous;
			}
		}

		//Constrains a single gene to stay within search space
		private double Constrain(double gene, double min, double max, bool ignoreBounds) {
			if(max - min == 0.0) {
				return min;
			}
			if(ignoreBounds) {
				//Overflow
				while(gene < -PI || gene > PI) {
					if(gene > PI) {
						gene -= PI + PI;
					}
					if(gene < -PI) {
						gene += PI + PI;
					}
				}
			} else {
				//Bounce
				while(gene < min || gene > max) {
					if(gene > max) {
						gene = max + max - gene;
					}
					if(gene < min) {
						gene = min + min - gene;
					}
				}
			}
			return gene;
		}

		//Evaluates the fitness of an individual
		private double ComputeFitness(double[] genes, bool balanced, Model.Node startNode = null, bool update = true) {
			if(update) {
				Model.ApplyConfiguration(genes, startNode);
			}
			double fitness = 0.0;
			for(int i=0; i<Model.ObjectivePtrs.Length; i++) {
				Objective objective = Model.ObjectivePtrs[i].Objective;
				Model.Node node = Model.ObjectivePtrs[i].Node;
				double loss = objective.ComputeLoss(node.WPX, node.WPY, node.WPZ, node.WRX, node.WRY, node.WRZ, node.WRW, node, balanced);
				node.HeuristicError = loss;
				fitness += objective.Weight * loss * loss;
			}
			return System.Math.Sqrt(fitness / (double)Model.ObjectivePtrs.Length);
		}

		//Evaluates the fitness of an individual after modifying one specific gene
		private double ComputeFitness(double[] genes, bool balanced, Model.MotionPtr modification) {
			double[] px, py, pz, rx, ry, rz, rw;
			modification.Node.SimulateModification(genes, out px, out py, out pz, out rx, out ry, out rz, out rw);
			double fitness = 0.0;
			for(int i=0; i<Model.ObjectivePtrs.Length; i++) {
				Objective objective = Model.ObjectivePtrs[i].Objective;
				Model.Node node = Model.ObjectivePtrs[i].Node;
				double loss = objective.ComputeLoss(px[i], py[i], pz[i], rx[i], ry[i], rz[i], rw[i], node, balanced);
				fitness += objective.Weight * loss * loss;
			}
			return System.Math.Sqrt(fitness / (double)Model.ObjectivePtrs.Length);
		}

		//Sorts the population by their fitness values (descending)
		private void SortByFitness() {
			System.Array.Sort(Population,
				delegate(Individual a, Individual b) {
					return a.Fitness.CompareTo(b.Fitness);
				}
			);
		}

		//In-place swap by references
		private void Swap(ref Individual[] a, ref Individual[] b) {
			Individual[] tmp = a;
			a = b;
			b = tmp;
		}

		public Model GetModel() {
			return Model;
		}

		public Individual[] GetPopulation() {
			return Population;
		}

		public int GetDimensionality() {
			return Dimensionality;
		}

		public int GetSize() {
			return Size;
		}

		public int GetElites() {
			return Elites;
		}

		public double[] GetSolution() {
			return Solution;
		}

		public double GetFitness() {
			return Fitness;
		}

		public double[,] GetGeneLandscape() {
			double[,] values = new double[Size,Dimensionality];
			for(int i=0; i<Size; i++) {
				for(int j=0; j<Dimensionality; j++) {
					values[i,j] = Population[i].Genes[j];
				}
			}
			return values;
		}

		public double[] GetFitnessLandscape() {
			double[] values = new double[Size];
			for(int i=0; i<Size; i++) {
				values[i] = Population[i].Fitness;
			}
			return values;
		}

		public double[] GetExtinctionLandscape() {
			double[] values = new double[Size];
			for(int i=0; i<Size; i++) {
				values[i] = Population[i].Extinction;
			}
			return values;
		}

		//Data class for the individuals
		public class Individual {
			public double[] Genes;
			public double[] Gradient;
			public double Extinction;
			public double Fitness;

			public Individual(int dimensionality) {
				Genes = new double[dimensionality];
				Gradient = new double[dimensionality];
				Extinction = 0f;
				Fitness = 0f;
			}
		}
	}
}