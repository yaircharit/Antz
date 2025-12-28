using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Assets.Scripts;

namespace Assets.Scripts.AntStates
{
    public class BaseQueenState : BaseState
    {
        private float broodTimer = 0f;
        private float broodInterval = 90f; // seconds between brood production attempts

        public new QueenAnt ant { 
            get { return (QueenAnt)base.ant; } 
            private set { base.ant = value; } }

        public BaseQueenState(QueenAnt ant, PheromoneType type = PheromoneType.Nest) : base(ant, type)
        {
            broodInterval = ant.genome["QueenCooldown"].Value * 60;
        }

        public override Color StateColor => Color.magenta;

        public override float GetEnergyModifier()
        {
            return ant.transform.localScale.x; // TODO: ???
        }

        public override void Tick()
        {
            ant.DropPheromone(pheroDropType);
            ant.ResetPheromoneDepositRate();

            // TODO: Implement queen-specific behavior
            if (!ant.IsFull && ant.Colony.HasFood)
            {
                ant.Eat(); // If hungry, eat food in nest
            }
            ant.ReduceEnergy();

            broodTimer += Time.deltaTime;
            if (broodTimer >= broodInterval)
            {
                broodTimer = 0f;
                if (ant.IsFull)
                {
                    ant.Mate(ant.Colony.Ants[UnityEngine.Random.Range(0, ant.Colony.Ants.Count)]); // Attempt to mate and produce brood
                }
            }

        }
    }
}
