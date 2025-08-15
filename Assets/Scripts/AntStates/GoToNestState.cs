using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class GoToNestState : StateBase
{
    PheromoneType pheroType;
    public GoToNestState(Ant ant, PheromoneType pheroType) : base(ant)
    {
        // This state is used when the ant needs to return to the nest, either to deposit food or because it has no more tasks.
        this.pheroType = pheroType;
    }
    public override void Enter()
    {
        ant.TargetPosition = ant.Colony.NestPos;
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
        if (pheroType != PheromoneType.None)
            ant.AddPheromone(pheroType);
    }

}

