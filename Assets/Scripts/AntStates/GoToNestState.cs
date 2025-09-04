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
            ant.ResetPheromoneDepositRate();
            if (ant.IsCarrying)
            {
                ant.Colony.AddFood(ant.CarriedMass);
                ant.Drop(); // Drop carried object in nest
            }

            if (!ant.IsFull())
            {
                ant.ChangeState(new HungryState(ant)); // If hungry, eat food in nest
                return;
            }

            ant.ChangeState(new SeekingFoodState(ant)); // If not hungry, seek food
            return;
        }

        if (ant.IsHungry() && ant.IsCarrying)
        {
            ant.ChangeState(new HungryState(ant)); // If hungry, eat food in nest
            return;
        }
        ant.TargetPosition = ant.Colony.NestPos;

        ant.Move(PheromoneType.Home); // Go to nest

        if (pheroType != PheromoneType.None && !ant.IsCarrying && ant.FoundFood()) //TODO: FoundFood sets target away from nest
        {
            ant.ChangeState(new SeekingFoodState(ant)); // If not carrying food, seek food
            return;
        }

        ant.AddPheromone(pheroType);


        if (pheroType == PheromoneType.None)
        {
            // TODO: seperate to new ClearPheromoneState?
            ant.RemovePheromone(PheromoneType.Food); // Clear food pheromone path if no food was found and ant went back
        }
    }

    public override string ToString()
    {
        return "Going home";
    }
}

