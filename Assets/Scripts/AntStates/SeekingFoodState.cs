using Unity.VisualScripting;
using UnityEngine;

public class SeekingFoodState : StateBase
{
    public SeekingFoodState(Ant ant, PheromoneType type = PheromoneType.Home) : base(ant, type) { }
    public override void Enter() {
    
    }
    public override void Exit() { }
    public override void Update()
    {
        if (ant.IsInNest())
        {
            ant.ResetPheromoneDepositRate();
        }

        ant.Target = ant.FindNearest(LayerMask.GetMask("Food"), ant.genome.ViewDistance);
        ant.Move(PheromoneType.Food);
        ant.AddPheromone(pheroType);

        if (ant.Target == null && ant.IsHungry() && ant.Colony.HasFood()
            //|| ant.IsLowOnPheromones()
            )
        {
            ant.ChangeState(new HungryState(ant)); // No food found, go to eat
            return;
        }

        if (ant.Target == null && ant.LowOnPheromones())
        {
            ant.ChangeState(new GoToNestState(ant, PheromoneType.Home)); // No food found, go back to nest
            return;
        }
    }

    public override void OnCollisionEnter(Collision collision)
    {
        if (collision.collider.transform == ant.Target)
        {
            ant.Pickup();
            if (!ant.IsFull())
            {
                ant.ChangeState(new HungryState(ant)); // If hungry, eat food
                return;
            }

            ant.ChangeState(new GoToNestState(ant, PheromoneType.Food));
            return;
        }
    }
}
