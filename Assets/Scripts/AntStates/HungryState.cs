using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class HungryState : StateBase
{
    public HungryState(Ant ant, PheromoneType type = PheromoneType.None) : base(ant, type, Color.yellow) { }
    public override void Enter()
    {
        base.Enter();
    }
    public override void Exit()
    {

    }
    public override void Update()
    {
        if (ant.IsFull())
        {
            ant.ChangeState(new SeekingFoodState(ant, (ant.IsInNest() ? PheromoneType.Home : PheromoneType.None)));
            return;
        }

        if (ant.IsCarrying)
        {
            ant.Eat(); // Eat the carried food mass
            ant.ChangeState(new SeekingFoodState(ant, PheromoneType.None)); // Go back to get more of the food you found
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
            if (ant.FoundFood())
                ant.ChangeState(new SeekingFoodState(ant));
            else
                ant.ChangeState(new GoToNestState(ant, PheromoneType.Home)); // If not in nest, go to nest to eat
            return;
        }

        ant.Eat();
    }
}

