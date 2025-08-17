using UnityEngine;

public abstract class StateBase
{
    protected Ant ant;
    protected PheromoneType pheroType;

    public StateBase(Ant ant, PheromoneType type = PheromoneType.None ) { this.ant = ant; pheroType = type; }
    public virtual void Enter() { }
    public virtual void Exit() { } 
    public abstract void Update();

    public virtual void OnCollisionEnter(Collision other)
    {
        // Default implementation does nothing, can be overridden in derived classes
    }

    public override string ToString()
    {
        return $"{GetType().Name}<{pheroType}>";
    }
}
