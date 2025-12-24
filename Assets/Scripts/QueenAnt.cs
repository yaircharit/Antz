using Assets.Scripts.AntStates;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts
{
    public class QueenAnt : Ant
    {
        public override string Name { get; set; } = "Queen Ant"; // Name of the queen ant

        protected BaseQueenState _queenCurrentState;
        public override BaseState currentState
        {
            get => _queenCurrentState;
            protected set { _queenCurrentState = (BaseQueenState)value;            }
        }


        void Start()
        {
            ChangeState(new BaseQueenState(this, PheromoneType.Home));
        }

        protected override void UpdateEffectiveStats(float sizeModifier = 0)
        {
            if (sizeModifier == 0)
                sizeModifier = genome["Size"].Value ;

            base.UpdateEffectiveStats(sizeModifier * genome["QueenSizeModifier"].Value);
        }

        public Ant SpawnChild()
        {
            Genome childGenome = genome.Clone(); // TODO: crossover with given genome 
            childGenome.Mutate();
            Ant childAnt = Colony.SpawnAnt(childGenome);
            return childAnt;
        }

        protected override void OnDrawGizmosSelected()
        {
            base.OnDrawGizmosSelected();
            Gizmos.color = Color.magenta;
            Gizmos.DrawWireSphere(transform.position, 1.0f);
        }
    }
}
