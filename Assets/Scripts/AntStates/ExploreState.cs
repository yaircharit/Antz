using Unity.IO.LowLevel.Unsafe;
using Unity.VisualScripting;
using UnityEngine;

public class ExploreState : StateBase
{
    public override Color StateColor => Color.blue;

    public ExploreState(Ant ant, PheromoneType type = PheromoneType.Home) : base(ant, type) { }
    public override void Tick()
    {
        if (ant.IsInNest())
        {
            ant.ResetPheromoneDepositRate();
        }

        if (ant.FoundFood())
        {
            ant.ChangeState(new FoundFoodState(ant, pheroDropType));
            return;
        }

        if (ant.IsHungry && ant.Colony.HasFood
            || ant.LowOnPheromones())
        {
            ant.ChangeState(new GoToNestState(ant, pheroDropType));
            return;
        }

        

        ant.Wander();
        ant.DropPheromone(pheroDropType);
    }


    public override string ToString()
    {
        return "Seeking food";
    }
}
