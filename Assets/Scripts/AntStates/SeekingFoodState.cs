using UnityEngine;

public class SeekingFoodState : AntStateBase
{
    public SeekingFoodState(Ant ant) : base(ant, PheromoneType.Home) { }
    public override void Enter() { }
    public override void Exit() { }
    public override void Update()
    {
        Vector3 targetPos;

        if (ant.targetFood != null)
        {
            // Found food, move towards it
            targetPos = ant.targetFood.position;
        }
        else
        {
            var phero = ant.GetMinPheromone(PheromoneType.Food);

            if (phero != null)
            {
                // Found a pheromone marker, move towards it
                targetPos = phero.Position;
            }
            else
            {
                // No pheromone found, wander randomly
                ant.transform.forward = (Quaternion.Euler(0, Random.Range(-10f, 10f), 0) * ant.transform.forward).normalized;
                targetPos = ant.transform.position + ant.transform.forward * ant.sampleRadius;

                // TODO: Avoid existing pheromones?
            }
        }

        targetPos.y = ant.transform.position.y;

        ant.transform.forward = (targetPos - ant.transform.position).normalized;

        ant.transform.position = Vector3.MoveTowards(ant.transform.position, targetPos, ant.speed * Time.deltaTime);

        AddPheromone();

        ant.FindNearestFood();

    }

    public override void OnTriggerEnter(Collider other)
    {
        if (other.transform == ant.targetFood)
        {
            ant.PickupFood();
        }
    }
}
