using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.AntStates
{
    public class BaseQueenState : BaseState
    {
        public BaseQueenState(QueenAnt ant, PheromoneType type = PheromoneType.Nest) : base(ant, type)
        {
        }

        public override Color StateColor => Color.magenta;

        public override float GetEnergyModifier()
        {
            return ant.transform.localScale.x; // TODO: ???
        }

        public override void Tick()
        {
            ant.ReduceEnergy();
            ant.DropPheromone(pheroDropType);
            ant.ResetPheromoneDepositRate();

            // TODO: Implement queen-specific behavior
            if (!ant.IsFull && ant.Colony.HasFood)
            {
                ant.Eat(); // If hungry, eat food in nest
                return;
            }
        }
    }
}
