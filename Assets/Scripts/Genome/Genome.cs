using System;
using System.Collections.Generic;
using UnityEngine;


public class Genome
{
    public static GenomeTraitDefinition[] TraitDefinitions;

    public Dictionary<string, GenomeTrait> Traits
        = new Dictionary<string, GenomeTrait>();

    public GenomeTrait this[string id]
    {
        get { return Traits[id]; }
        set { Traits[id] = value; }
    }

    /// <summary>
    /// Initialize trait definitions from resources. Call this once at startup before creating Genome instances.
    /// </summary>
    public static void InitializeTraitDefinitions()
    {
        if (TraitDefinitions == null)
        {
            TraitDefinitions = Resources.LoadAll<GenomeTraitDefinition>("GenomeTraits");
        }
    }

    public Genome()
    {
        // Do NOT load resources here — loading during construction (or during deserialization)
        // can trigger "Recursive Serialization" in WebGL. Require explicit initialization.
        if (TraitDefinitions == null)
        {
            Debug.LogError("Genome: TraitDefinitions are not initialized. Call Genome.InitializeTraitDefinitions() before creating Genome instances.");
            return;
        }

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
        return clone;
    }

    // Recombine two parent genomes into a child genome.
    // For each trait we randomly pick one parent's value or average them; copy mutation parameters as averaged.
    public static Genome Cross(Genome parentA, Genome parentB)
    {
        if (parentA == null && parentB == null) return new Genome();
        if (parentA == null) return parentB.Clone();
        if (parentB == null) return parentA.Clone();

        Genome child = new Genome();

        foreach (var def in TraitDefinitions)
        {
            string id = def.Id;
            parentA.Traits.TryGetValue(id,out GenomeTrait a );
            parentB.Traits.TryGetValue(id,out GenomeTrait b );

            GenomeTrait ct = new GenomeTrait();

            if (UnityEngine.Random.Range(0,a.HereditaryStrength) > UnityEngine.Random.Range(0, b.HereditaryStrength))
                ct = a.Clone();
            else
                ct = b.Clone();

            // small chance to average instead to smooth traits
            if (UnityEngine.Random.value < 0.1f)
            {
                ct.Value = (a.Value + b.Value) * 0.5f;
            }

            // ensure within definition bounds
            ct.Value = Mathf.Clamp(ct.Value, def.MinValue, def.MaxValue);
            
            child.Traits[id] = ct;
        }

        return child;
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
    }
}

