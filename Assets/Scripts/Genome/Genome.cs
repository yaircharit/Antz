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
        return clone;
    }


    public void Mutate(bool forceMutation = false)
    {
        foreach (var trait in Traits.Values)
        {
            trait.Mutate(forceMutation);
        }
    }
}

