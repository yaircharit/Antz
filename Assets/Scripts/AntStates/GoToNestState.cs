using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class GoToNestState : StateBase
{
    public GoToNestState(Ant ant, PheromoneType type = PheromoneType.None) : base(ant, type)
    {
        // This state is used when the ant needs to return to the nest, either to deposit food or because it has no more tasks.
    }
    public override void Enter()
    {
        ant.TargetPosition = ant.Colony.NestPos;
        if (pheroType == PheromoneType.Food)
        {
            ant.ResetPheromoneDepositRate(); // Reset pheromone deposit rate when entering this state
        }
    }
    public override void Exit()
    {

    }
    public override void Update()
    {

        if (ant.IsInNest())
        {

            if (ant.IsCarrying)
            {
                ant.Colony.AddFood(ant.CarriedMass);
                ant.Drop(); // Drop carried object in nest
            }

            ant.ChangeState(new SeekingFoodState(ant)); // If not hungry, seek food
            return;
        }

        if (ant.IsHungry() && ant.IsCarrying)
        {
            ant.ChangeState(new HungryState(ant)); // If hungry, eat food in nest
            return;
        }

        ant.Move(PheromoneType.Home); // Go to nest
        ant.AddPheromone(pheroType);
    }

}

