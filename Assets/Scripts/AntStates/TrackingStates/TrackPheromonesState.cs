

public class TrackPheromonesState : BaseState
{
    public override UnityEngine.Color StateColor => UnityEngine.Color.black;

    protected PheromoneType pheroTargetType;
    public TrackPheromonesState(Ant ant, PheromoneType pheroDropType = PheromoneType.None, PheromoneType pheroTargetType = PheromoneType.None) : base(ant, pheroDropType) 
    { 
        this.pheroTargetType = pheroTargetType; 
    }
    public override void Tick()
    {
        ant.Move(pheroTargetType);
        ant.DropPheromone(pheroDropType);
    }

    public override float GetEnergyModifier()
    {
        return ant.CurrentSpeed;
    }
}