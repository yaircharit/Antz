

using UnityEngine;

public class HealingState : BaseState
{
    public float healingCostModifier = 2f;

    public HealingState(Ant ant, PheromoneType type = PheromoneType.None) : base(ant, type)
    {
    }

    public override Color StateColor => Color.green;

    public override float GetEnergyModifier()
    {
        return healingCostModifier * ant.genome.HealingRate;
    }

    public override void Tick()
    {
        if (ant.CurrentHealth == ant.MaxHealth)
        {
            ant.ChangeState(new ExploreState(ant));
            return;
        }

        if (!ant.IsHungry)
        {
            ant.RaiseOnHealed();
        }
        else
        {
            ant.Eat();
        }


    }
}