using UnityEngine;

public class CarryingState : AntStateBase
{
    public CarryingState(Ant ant) : base(ant) { }

    public override void Enter() {
        ant.Pickup();
        ant.TargetPosition = ant.Colony.NestPos;
    }

    public override void Exit() {
        ant.Drop();
    }

    public override void Update()
    {
        if (ant.CarriedObj == null) return;

        if (ant.IsInNest() && ant.CarriedObj != null)
        {
            ant.ChangeState(new SeekingFoodState(ant));
            ant.Colony.AddFood(ant.CarriedMass);
            return; 
        }

        ant.Move(PheromoneType.Home);
        ant.AddPheromone(PheromoneType.Food);
    }

   
}
