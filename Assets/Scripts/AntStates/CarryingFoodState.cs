using UnityEngine;

public class CarryingFoodState : AntStateBase
{
    public CarryingFoodState(Ant ant) : base(ant) { }
    
    public override void Enter() { }
    
    public override void Exit() { }
    
    public override void Update()
    {
        if (ant.carriedFood == null) return;
        
        float speedFactor = Mathf.Lerp(0.5f, 1f, ant.strength);
        float carryingSpeed = ant.speed * speedFactor;
        Vector3 homeDirection = ant.GetDirectionFromPheromones(Pheromone.PheromoneType.Home);
        Vector3 finalDirection = (homeDirection.sqrMagnitude > 0.001f)
            ? homeDirection
            : ant.currentWanderDirection;
            
        Vector3 targetPosition = ant.transform.position + finalDirection;
        targetPosition.y = ant.transform.position.y;
        ant.transform.position = Vector3.MoveTowards(ant.transform.position, targetPosition, carryingSpeed * Time.deltaTime);
        
        if (finalDirection.sqrMagnitude > 0.001f)
        {
            Quaternion lookRotation = Quaternion.LookRotation(finalDirection, Vector3.up);
            ant.transform.rotation = Quaternion.Euler(0, lookRotation.eulerAngles.y, 0);
        }
        
        float distanceToNest = Vector3.Distance(ant.transform.position, ant.nestObj.transform.position);
        float depositMultiplier = 1f + (1f / (distanceToNest + 1f));
        ant.pheromoneMap.AddPheromone(ant.transform.position, ant.pheromoneDepositRate * depositMultiplier * Time.deltaTime, Pheromone.PheromoneType.Food);
        
        // Update carried food position
        Vector3 aboveAnt = ant.transform.position + Vector3.up * ant.carryHeight;
        ant.carriedFood.transform.position = aboveAnt;
        ant.carriedFood.transform.rotation = Quaternion.identity;
        
        ant.RecordPath();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject == ant.nestObj && ant.carriedFood != null)
        {
            foreach (Vector3 position in ant.pathHistory)
            {
                ant.pheromoneMap.AddPheromone(position, ant.pheromoneDepositRate * 2f, Pheromone.PheromoneType.Food);
            }
            ant.pathHistory.Clear();
            ant.DropFoodInNest();

            ant.ChangeState(new SeekingFoodState(ant));
        }
    }
}
