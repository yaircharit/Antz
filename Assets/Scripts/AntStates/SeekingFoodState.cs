using UnityEngine;

public class SeekingFoodState : AntStateBase
{
    public SeekingFoodState(Ant ant) : base(ant) { }
    public override void Enter() { }
    public override void Exit() { }
    public override void Update()
    {
        ant.FindNearestFood();
        Vector3 finalDirection = Vector3.zero;
        if (ant.targetFood == null)
        {
            Vector3 pheromoneDirection = ant.GetDirectionFromPheromones(Pheromone.PheromoneType.Food);
            if (pheromoneDirection.sqrMagnitude > 0.01f)
            {
                finalDirection = Vector3.Lerp(ant.currentWanderDirection, pheromoneDirection, ant.pheromoneInfluence);
            }
            else
            {
                float currentPheromoneLevel = ant.pheromoneMap.GetPheromone(ant.transform.position + ant.currentWanderDirection * ant.sampleRadius, Pheromone.PheromoneType.Home);
                if (currentPheromoneLevel > 0.8f)
                {
                    float leftPheromone = ant.pheromoneMap.GetPheromone(ant.transform.position + Quaternion.Euler(0, -90f, 0) * ant.currentWanderDirection * ant.sampleRadius, Pheromone.PheromoneType.Home);
                    float rightPheromone = ant.pheromoneMap.GetPheromone(ant.transform.position + Quaternion.Euler(0, 90f, 0) * ant.currentWanderDirection * ant.sampleRadius, Pheromone.PheromoneType.Home);
                    float steerAngle = (leftPheromone > rightPheromone) ? 20f : -20f;
                    ant.currentWanderDirection = Quaternion.Euler(0, steerAngle, 0) * ant.currentWanderDirection;
                }
                else
                {
                    ant.currentWanderDirection = Quaternion.Euler(0, Random.Range(-10f, 10f), 0) * ant.currentWanderDirection;
                }
                finalDirection = ant.currentWanderDirection;
            }
            ant.pheromoneMap.AddPheromone(ant.transform.position, ant.pheromoneDepositRate * Time.deltaTime, Pheromone.PheromoneType.Home);
        }
        else
        {
            Vector3 targetDirection = (ant.targetFood.position - ant.transform.position).normalized;
            Vector3 pheromoneDirection = ant.GetDirectionFromPheromones(Pheromone.PheromoneType.Food);
            if (pheromoneDirection.sqrMagnitude > 0.01f)
            {
                finalDirection = Vector3.Lerp(targetDirection, pheromoneDirection, ant.pheromoneInfluence);
            }
            else
            {
                if (Random.value > ant.explorationRate)
                {
                    finalDirection = targetDirection;
                }
                else
                {
                    finalDirection = Quaternion.Euler(0, Random.Range(-45f, 45f), 0) * targetDirection;
                }
            }
            ant.pheromoneMap.AddPheromone(ant.transform.position, ant.pheromoneDepositRate * Time.deltaTime, Pheromone.PheromoneType.Home);
        }
        Vector3 targetPosition = ant.transform.position + finalDirection;
        targetPosition.y = ant.transform.position.y;
        ant.transform.position = Vector3.MoveTowards(ant.transform.position, targetPosition, ant.speed * Time.deltaTime);
        if (finalDirection.sqrMagnitude > 0.001f)
        {
            Quaternion lookRotation = Quaternion.LookRotation(finalDirection, Vector3.up);
            ant.transform.rotation = Quaternion.Euler(0, lookRotation.eulerAngles.y, 0);
        }
        if (ant.carriedFood != null)
        {
            Vector3 aboveAnt = ant.transform.position + Vector3.up * ant.carryHeight;
            ant.carriedFood.transform.position = aboveAnt;
            ant.carriedFood.transform.rotation = Quaternion.identity;
        }
        ant.RecordPath();
    }
}
