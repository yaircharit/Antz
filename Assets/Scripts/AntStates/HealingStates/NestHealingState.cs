

using UnityEngine;

public class NestHealingState : HealingState
{
    public override Color StateColor => Color.Lerp(Color.green, Color.black, 0.5f);

    public NestHealingState(Ant ant, PheromoneType type = PheromoneType.None) : base(ant, type)
    {
    }

    public override void Tick()
    {
        base.Tick();

        if (!ant.Colony.HasFood)
        {
            ant.ChangeState(new ExploreState(ant));
        }

        if (!ant.IsInNest())
        {
            ant.ChangeState(new GoToNestState(ant));
            return;
        }
    }
}