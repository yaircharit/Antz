using UnityEngine;

public class GoToNestState : TrackPheromonesState
{
    public override Color StateColor => Color.Lerp(Color.blue,Color.red,0.5f);
    public GoToNestState(Ant ant, PheromoneType pheroDropType = PheromoneType.None) : base(ant, pheroDropType, PheromoneType.Home)
    {
        // This state is used when the ant needs to return to the nest, either to deposit food or because it has no more tasks.
    }
    public override void Enter()
    {
        ant.TargetPosition = ant.Colony.NestPos;
        base.Enter();
    }

    public override void Tick()
    {
        if (ant.IsInNest())
        {
            ant.ResetPheromoneDepositRate();

            if (!ant.IsFull && ant.Colony.HasFood)
            {
                ant.Eat(); // If hungry, eat food in nest
                return;
            }

            ant.ChangeState(new ExploreState(ant)); // If not hungry, seek food
            return;
        }

        base.Tick();

        if (pheroDropType == PheromoneType.None)
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

