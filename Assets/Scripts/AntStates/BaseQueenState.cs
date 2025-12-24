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
        public BaseQueenState(QueenAnt ant, PheromoneType type = PheromoneType.None) : base(ant, type)
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
            // TODO: Implement queen-specific behavior
        }
    }
}
