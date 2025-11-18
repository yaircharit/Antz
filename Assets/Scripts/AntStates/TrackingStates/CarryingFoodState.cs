using UnityEngine;

public class CarryingFoodState : GoToNestState
{
    public override Color StateColor => Color.red;

    public CarryingFoodState(Ant ant) : base(ant, PheromoneType.Food) { 
    
    }

    public override void Enter()
    {
        ant.ResetPheromoneDepositRate();
        base.Enter();
    }

    public override void Tick()
    {
        if (ant.IsHungry)
        {
            ant.Eat(); // Eat the carried food mass
            ant.ChangeState(new GoToNestState(ant,pheroDropType));
            return;
        }

        if (ant.IsInNest())
        {
            ant.Colony.AddFood(ant.CarriedMass);
            ant.Drop(); // Drop carried object in nest
            ant.ResetPheromoneDepositRate();
            ant.ChangeState(new ExploreState(ant)); // After dropping food, explore for more
            return;
        }

        base.Tick();
    }

    public override float GetEnergyModifier()
    {
        return base.GetEnergyModifier() * Mathf.Min(1, ant.CarriedMass / ant.genome.EffectiveStrength);
    }
}
