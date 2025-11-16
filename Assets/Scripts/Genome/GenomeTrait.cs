using System;
using UnityEngine;

[Serializable]
public class GenomeTrait
{
    public GenomeTraitDefinition Definition;

    public float Value;
    [Range(0f, 1f)]
    public float HereditaryStrength;

    [Range(0f, 1f)]
    public float MutationRate;

    [Range(0f, 1f)]
    public float MutationStrength;

    public GenomeTrait(GenomeTraitDefinition traitDef)
    {
        Definition = traitDef;
        Value = traitDef.DefaultValue;
        HereditaryStrength = 1f;
        MutationRate = traitDef.BaseMutationRate;
        MutationStrength = traitDef.BaseMutationStrength;
    }

    public GenomeTrait() { }

    public GenomeTrait Clone()
    {
        return new GenomeTrait
        {
            Definition = this.Definition,
            Value = this.Value,
            HereditaryStrength = this.HereditaryStrength,
            MutationRate = this.MutationRate,
            MutationStrength = this.MutationStrength
        };
    }

    public float GetEnergyCost()
    {
        return Value * Definition.EnergyCostPerUnit;
    }

    public void Mutate(bool forceMutation = false)
    {
        if (forceMutation || UnityEngine.Random.value < MutationRate)
        {
            float maxDelta = (Definition.MaxValue - Definition.MinValue) * MutationStrength;
            Value += UnityEngine.Random.Range(-maxDelta, maxDelta);
            Value = Mathf.Clamp(Value, Definition.MinValue, Definition.MaxValue);
        }
    }
}
