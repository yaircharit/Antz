using UnityEngine;

public abstract class StateBase
{
    protected Ant ant;
    public StateBase(Ant ant ) { this.ant = ant; }
    public virtual void Enter() { }
    public virtual void Exit() { } 
    public abstract void Update();

    public virtual void OnCollisionEnter(Collision other)
    {
        // Default implementation does nothing, can be overridden in derived classes
    }
}
