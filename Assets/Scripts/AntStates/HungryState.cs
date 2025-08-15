using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class HungryState : StateBase
{
    public HungryState(Ant ant) : base(ant) { }
    public override void Enter()
    {
    }
    public override void Exit()
    {

    }
    public override void Update()
    {
        if (ant.IsFull())
        {
            ant.ChangeState(new SeekingFoodState(ant));
            return;
        }

        if (ant.IsCarrying)
        {
            ant.Eat(); // Eat the carried food mass
            ant.ChangeState(new SeekingFoodState(ant));
            return;
        }

        if (!ant.Colony.HasFood())
        {
            // TODO: panic? YES! PANIC!
            ant.ChangeState(new SeekingFoodState(ant)); // No food in nest, go find food
            return;
        }

        if (!ant.IsInNest())
        {
            ant.ChangeState(new GoToNestState(ant, PheromoneType.Home)); // If not in nest, go to nest to eat
            return;
        }

        if (ant.Colony.HasFood())
        {
            ant.Eat();
        }
    }
}

