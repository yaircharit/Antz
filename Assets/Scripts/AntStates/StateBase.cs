using UnityEngine;

public abstract class StateBase
{
    protected Ant ant;
    protected PheromoneType pheroType;
    public Color stateColor = Color.white;

    public StateBase(Ant ant, PheromoneType type = PheromoneType.None, Color color = default(Color)) { this.ant = ant; pheroType = type; stateColor = color; }
    public virtual void Enter() {
        ant.RaiseOnStateChanged();
    }
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
