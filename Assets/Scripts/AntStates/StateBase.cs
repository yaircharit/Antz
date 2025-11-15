using UnityEngine;

public abstract class StateBase
{
    protected Ant ant;
    protected PheromoneType pheroDropType;
    public abstract Color StateColor { get; }

    public StateBase(Ant ant, PheromoneType type = PheromoneType.None) { this.ant = ant; pheroDropType = type; }
    public virtual void Enter() {
        ant.RaiseOnStateChanged();
    }
    public virtual void Exit() { } 
    public abstract void Tick();

    public virtual void OnCollisionEnter(Collision other)
    {
        // Default implementation does nothing, can be overridden in derived classes
    }

    public override string ToString()
    {
        return $"{GetType().Name}<{pheroDropType}>";
    }
}
