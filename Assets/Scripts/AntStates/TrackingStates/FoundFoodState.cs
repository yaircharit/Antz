

using UnityEngine;

public class FoundFoodState : TrackPheromonesState
{
    public override Color StateColor => Color.Lerp(Color.red,Color.yellow,0.5f); // orange

    public FoundFoodState(Ant ant, PheromoneType pheroDropType = PheromoneType.Home) : base(ant, pheroDropType, PheromoneType.Food) { }
    public override void Tick()
    {
        if (!ant.FoundFood())
        {
            ant.ChangeState(new ExploreState(ant, pheroDropType)); // Lost sight of food, resume exploring
            return;
        }

        base.Tick();

        if (ant.isStuck)
        {
            ant.ChangeState(new GoToNestState(ant)); // Go back to nest and delete trail
            return;
        }
    }

    public override void OnCollisionEnter(Collision other)
    {
        if (other.gameObject.layer != LayerMask.NameToLayer("Food")) 
            return;

        ant.Pickup(other.transform);

        if (ant.CurrentHealth < ant.MaxHealth)
        {
            ant.ChangeState(new OutHealingState(ant));
            return;
        }

        if (!ant.IsFull)
        {
            ant.Eat(); // Eat some of the food until full
            return;
        }

        ant.ChangeState(new CarryingFoodState(ant)); 
    }
}