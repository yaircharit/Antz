using UnityEngine;

public class SeekingFoodState : AntStateBase
{
    public SeekingFoodState(Ant ant) : base(ant) { }
    public override void Enter() { }
    public override void Exit() { }
    public override void Update()
    {
        ant.Target = ant.FindNearest(LayerMask.GetMask("Food"));
        ant.Move(PheromoneType.Food);
        ant.AddPheromone(PheromoneType.Home);
    }

    public override void OnCollisionEnter(Collision collision)
    {
        if (collision.collider.transform == ant.Target)
        {
            ant.ChangeState(new CarryingState(ant));
        }
    }
}
