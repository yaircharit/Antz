using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.VisualScripting;
using UnityEngine;


public class Genome
{
    public static GenomeTraitDefinition[] TraitDefinitions
        = Resources.LoadAll<GenomeTraitDefinition>("GenomeTraits");

    public Dictionary<string, GenomeTrait> Traits
        = new Dictionary<string, GenomeTrait>();

    private static readonly float BaseHealth = 100f;
    private static readonly float BaseEnergy = 100f;
    public float EffectiveSpeed;
    public float EffectiveStrength;
    public float MaxHealth;
    public float MaxEnergy;
    public float HealingRate;

    public GenomeTrait this[string id]
    {
        get { return Traits[id]; }
        set { Traits[id] = value; }
    }


    //// Constructor to initialize the genome traits with default values or specified values
    public Genome()
    {
        foreach (var traitDef in TraitDefinitions)
        {
            Traits[traitDef.Id] = new GenomeTrait(traitDef);
        }
    }


    public Genome Clone()
    {
        Genome clone = new Genome();
        foreach (var trait in Traits)
        {
            clone.Traits[trait.Key] = trait.Value.Clone();
        }
        UpdateEffectiveStats();
        return clone;
    }

    public void UpdateEffectiveStats()
    {
        float sizeModifier = Traits["Size"].Value;
        EffectiveSpeed = Traits["Speed"].Value / sizeModifier;
        EffectiveStrength = Traits["Strength"].Value * sizeModifier;
        MaxHealth = BaseHealth * sizeModifier;
        MaxEnergy = BaseEnergy * sizeModifier;
        HealingRate = MaxHealth * Traits["Metabolism"].Value;
    }

    public float GetEnergyCost()
    {
        float totalCost = 0f;
        foreach (var trait in Traits.Values)
        {
            totalCost += trait.GetEnergyCost();
        }
        totalCost *= Traits["Size"].Value; // Size multiplier
        totalCost *= Traits["Metabolism"].Value; // Energy Efficiency multiplier

        return totalCost;
    }

    public void Mutate(bool forceMutation = false)
    {
        foreach (var trait in Traits.Values)
        {
            trait.Mutate(forceMutation);
        }
        UpdateEffectiveStats();
    }
}

