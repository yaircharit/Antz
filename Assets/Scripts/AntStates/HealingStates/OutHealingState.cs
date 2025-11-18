

using UnityEngine;

public class OutHealingState : HealingState
{
    public override Color StateColor => Color.Lerp(Color.green, Color.blue, 0.5f);


    public OutHealingState(Ant ant, PheromoneType type = PheromoneType.None) : base(ant, type)
    {
    }

    public override void Tick()
    {
        base.Tick();

        if (ant.FindFood())
        {
            if (!ant.IsHungry) return; //heal and wait

            ant.ChangeState(new FoundFoodState(ant));
            return;
        }
        
        if (ant.Colony.HasFood)
        {
            ant.ChangeState(new NestHealingState(ant));
            return;
        }

        ant.ChangeState(new ExploreState(ant));
    }
}