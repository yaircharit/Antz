using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.AntStates.HealingStates
{
    public class QueenHealingState : BaseQueenState
    {
        private float healingCostModifier = 3;

        public QueenHealingState(QueenAnt ant) : base(ant)
        {
        }

        public override Color StateColor => Color.green;

        public override float GetEnergyModifier()
        {
            return healingCostModifier * ant.HealingRate;
        }

        public override void Tick()
        {
            base.Tick();

            if (ant.CurrentHealth == ant.MaxHealth)
            {
                ant.ChangeState(new BaseQueenState(ant));
            }

            if (!ant.IsHungry)
            {
                ant.RaiseOnHealed();
            }
            else if (ant.Colony.HasFood)
            {
                ant.Eat();
            }
            else
            {
               ant.ChangeState(new BaseQueenState(ant)); // No food available, switch back to base state //TODO: different state
            }
        }
    }
}
