using Assets.Scripts.AntStates;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts
{
    public class QueenAnt : Ant
    {
        [SerializeField] private Brood BroodPrefab; // Prefab for the brood

        public override string Name { get; set; } = "Queen Ant"; // Name of the queen ant

        protected BaseQueenState _queenCurrentState
        {
            get { return base.currentState as BaseQueenState; }
            set { base.currentState = value; }
        }
        public override BaseState currentState
        {
            get => base.currentState; // delegate to base to preserve non-queen states
            protected set
            {
                base.currentState = value;
                _queenCurrentState = value as BaseQueenState; // safe cast, null if not a queen state
            }
        }


        void Start()
        {
            ChangeState(new BaseQueenState(this, PheromoneType.Home));
        }

        protected override void UpdateEffectiveStats(float sizeModifier = 0)
        {
            if (sizeModifier == 0)
                sizeModifier = Size;

            base.UpdateEffectiveStats(sizeModifier * genome["QueenSizeModifier"].Value);
        }

        public float GetMatingEnergyCost(Brood b)
        {
            return (0.1f / genome["BroodHatchTime"].Value * genome["QueenCooldown"].Value) * b.Size;
        }

        // Generates a random float with an approximate normal distribution 
        // centered around 'mean', with a spread defined by 'stdDev'.
        public static float RandomNormalDistribution(float mean, float stdDev)
        {
            float sum = 0f;
            // Average 3 random numbers for a noticeable curve (Central Limit Theorem)
            sum += UnityEngine.Random.value;
            sum += UnityEngine.Random.value;
            sum += UnityEngine.Random.value;
            // The resulting range is now 0 to 3, with a mean of 1.5.

            // Normalize the result to a mean of 0 and a range of approx -1.5 to 1.5
            float avg = (sum / 3f) - 0.5f;

            // Scale and shift the value to fit the desired mean and standard deviation
            return mean + avg * (stdDev * 2f); // Adjust multiplier as needed for desired 'tightness'
        }

        public void Mate(Ant ant)
        {
            int numberOfOffspring = (int)(RandomNormalDistribution(genome["OffspringCount"].Value, 2));

            for (int i = 0; i < numberOfOffspring; i++)
            {
                Genome childGenome = Genome.Cross(this.genome, ant.genome);
                childGenome.Mutate();
                Brood b = SpawnBrood(childGenome);
                ReduceEnergy(GetMatingEnergyCost(b) * ant.GetEnergyCost()); // Mating energy cost
            }
            Debug.Log($"{numberOfOffspring} broods created!");
        }

        public Brood SpawnBrood(Genome broodGenome)
        {
            Brood b = Instantiate(BroodPrefab, transform.position + (UnityEngine.Random.insideUnitSphere + Vector3.up) * Size, Quaternion.identity,Colony.AntsContainer); // added position and rotation
            b.Init(Colony, broodGenome); // changed genome parameter to broodGenome
            return b; // corrected the return statement
        }

        public Ant SpawnChild()
        {
            Genome childGenome = genome.Clone(); // TODO: crossover with given genome 
            childGenome.Mutate();
            Ant childAnt = Colony.SpawnAnt(childGenome);
            return childAnt;
        }

        protected override void OnDrawGizmosSelected()
        {
            base.OnDrawGizmosSelected();
            Gizmos.color = Color.magenta;
            Gizmos.DrawWireSphere(transform.position, 1.0f);
        }
    }
}
