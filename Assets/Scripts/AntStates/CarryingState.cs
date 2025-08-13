using UnityEngine;

public class CarryingState : AntStateBase
{
    public CarryingState(Ant ant) : base(ant) { }

    public override void Enter() {
        ant.Pickup();
        ant.TargetPosition = ant.Colony.NestPos;
        ant.ResetPheromoneDepositRate();
    }

    public override void Exit() {
        ant.Drop();
    }

    public override void Update()
    {
        if (ant.CarriedObj == null) return;

        if (ant.IsInNest() && ant.CarriedObj != null)
        {
            ant.Colony.AddFood(ant.CarriedMass);
            ant.ChangeState(new SeekingFoodState(ant));
            return; 
        }

        ant.Move(PheromoneType.Home);
        ant.AddPheromone(PheromoneType.Food);
    }


}
