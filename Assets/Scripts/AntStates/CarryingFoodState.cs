using UnityEngine;

public class CarryingFoodState : AntStateBase
{
    public CarryingFoodState(Ant ant) : base(ant, PheromoneType.Food) { }

    public override void Enter() { }

    public override void Exit() { }

    public override void Update()
    {
        if (ant.carriedFood == null) return;

        float speedFactor = Mathf.Lerp(0.5f, 1f, ant.strength);
        float carryingSpeed = ant.speed * speedFactor;//0.5f * (1f + ant.strength);
        Vector3 targetPos;

        if (Vector3.Distance(ant.transform.position, ant.nestObj.transform.position) < ant.detectRadius)
        {
            targetPos = ant.nestObj.transform.position;
        }
        else
        {
            var phero = ant.GetMinPheromone(PheromoneType.Home);

            if ( phero != null )
            {
                // Move towards pheromone marker
                targetPos = phero.Position;
            }
            else
            {
                // No pheromone found, wander randomly
                ant.transform.forward = (Quaternion.Euler(0, Random.Range(-10f, 10f), 0) * ant.transform.forward).normalized;
                targetPos = ant.transform.position + ant.transform.forward * ant.sampleRadius;
            }
        }

        targetPos.y = ant.transform.position.y;

        ant.transform.forward = (targetPos - ant.transform.position).normalized;
        ant.transform.position = Vector3.MoveTowards(ant.transform.position, targetPos, carryingSpeed * Time.deltaTime);
        
        AddPheromone();
    }

    public override void OnTriggerEnter(Collider other)
    {
        if (other.gameObject == ant.nestObj && ant.carriedFood != null)
        {
            ant.DropFoodInNest();
        }
    }

    
}
