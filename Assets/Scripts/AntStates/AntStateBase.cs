using UnityEngine;

public abstract class AntStateBase
{
    protected Ant ant;
    public AntStateBase(Ant ant) { this.ant = ant; }
    public virtual void Enter() { }
    public virtual void Exit() { }
    public abstract void Update();
}
