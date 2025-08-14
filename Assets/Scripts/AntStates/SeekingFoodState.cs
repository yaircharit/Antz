using UnityEngine;

public class SeekingFoodState : AntStateBase
{
    public SeekingFoodState(Ant ant) : base(ant) { }
    public override void Enter() {
    
        ant.ResetPheromoneDepositRate();
    }
    public override void Exit() { }
    public override void Update()
    {
        if (ant.IsInNest())
        {
            ant.ResetPheromoneDepositRate();
        }

        ant.Target = ant.FindNearest(LayerMask.GetMask("Food"));
        ant.Move(PheromoneType.Food);
        ant.AddPheromone(PheromoneType.Home);

        if (ant.Target == null && ant.IsHungry() && ant.Colony.HasFood()
            //|| ant.IsLowOnPheromones()
            )
        {
            ant.ChangeState(new HungryState(ant)); // No food found, go to eat
            return;
        }
    }

    public override void OnCollisionEnter(Collision collision)
    {
        if (collision.collider.transform == ant.Target)
        {
            ant.Pickup();
            if (ant.IsHungry())
            {
                ant.ChangeState(new HungryState(ant)); // If hungry, eat food
                return;
            }

            ant.ChangeState(new GoToNestState(ant, PheromoneType.Food));
            return;
        }
    }
}
