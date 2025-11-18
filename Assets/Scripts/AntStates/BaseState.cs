using UnityEngine;

public abstract class BaseState
{
    protected Ant ant;
    protected PheromoneType pheroDropType;
    public abstract Color StateColor { get; }

    public BaseState(Ant ant, PheromoneType type = PheromoneType.None) { this.ant = ant; pheroDropType = type; }
    public virtual void Enter() {
        ant.RaiseOnStateChanged();
    }
    public virtual void Exit() { } 
    public abstract void Tick();

    public virtual void OnCollisionEnter(Collision other)
    {
        // Default implementation does nothing, can be overridden in derived classes
    }

    public abstract float GetEnergyModifier();

    public override string ToString()
    {
        return $"{GetType().Name}<{pheroDropType}>";
    }
}
