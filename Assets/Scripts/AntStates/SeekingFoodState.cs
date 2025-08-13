using UnityEngine;

public class SeekingFoodState : AntStateBase
{
    public SeekingFoodState(Ant ant) : base(ant, PheromoneType.Home) { }
    public override void Enter() { }
    public override void Exit() { }
    public override void Update()
    {
        Vector3 targetDirection;

        if (ant.target != null)
        {
            // Found food, move towards it
            targetDirection = ant.GetDirectionTo(ant.target.position);
        }
        else
        {
            var phero = ant.GetMinPheromone(PheromoneType.Food);

            if (phero != null)
            {
                // Found a pheromone marker, move towards it
                targetDirection = ant.GetDirectionTo(phero.Position);

                if (Random.value < ant.explorationRate)
                {
                    // Randomly explore around the pheromone
                    targetDirection = ant.GetRandomDirection(targetDirection);
                }
            }
            else
            {
                // No pheromone found, wander randomly
                targetDirection = ant.GetRandomDirection();

                // TODO: Avoid existing pheromones?
            }
        }

        ant.MoveInDirection(targetDirection);

        AddPheromone();

        var nearestFood = ant.FindNearest(LayerMask.GetMask("Food"));

        if (nearestFood != null && ant.carriedObj == null)
        {
            ant.target = nearestFood;
        }
        else if (nearestFood == null)
        {
            ant.target = null;
        }
    }

    public override void OnTriggerEnter(Collider other)
    {
        if (other.transform == ant.target)
        {
            ant.ChangeState(new CarryingState(ant));
        }
    }
}
