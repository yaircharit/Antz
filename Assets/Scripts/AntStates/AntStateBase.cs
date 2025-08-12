using UnityEngine;

public abstract class AntStateBase
{
    protected Ant ant;
    protected PheromoneType defaultPheromoneType;
    public AntStateBase(Ant ant, PheromoneType defaultPheromone ) { this.ant = ant; this.defaultPheromoneType = defaultPheromone; }
    public virtual void Enter() { }
    public virtual void Exit() { } 
    public abstract void Update();

    public virtual void OnTriggerEnter(Collider other)
    {
        // Default implementation does nothing, can be overridden in derived classes
    }

    public virtual void AddPheromone()
    {
        ant.AddPheromone(defaultPheromoneType);
    }
}
