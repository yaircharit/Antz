using UnityEngine;

public class CarryingState : AntStateBase
{
    public CarryingState(Ant ant) : base(ant, PheromoneType.Food) { }

    public override void Enter() {
        ant.Pickup();
    }

    public override void Exit() {
        ant.Drop();
    }

    public override void Update()
    {
        if (ant.carriedObj == null) return;
        Vector3 targetDirection;

        if (ant.IsInRange(ant.nestObj))
        {
            targetDirection = ant.GetDirectionTo(ant.nestObj);
        }
        else
        {
            var phero = ant.GetMinPheromone(PheromoneType.Home);

            if (phero != null)
            {
                // Move towards pheromone marker
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
            }
        }

        ant.MoveInDirection(targetDirection);
        AddPheromone();
    }

    public override void OnTriggerEnter(Collider other)
    {
        if (other.gameObject == ant.nestObj && ant.carriedObj != null)
        {
            ant.ChangeState(new SeekingFoodState(ant));
        }
    }
}
