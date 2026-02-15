using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameSetup : MonoBehaviour
{
    [SerializeField] Transform genesContainer;
    [SerializeField] GeneSlider genesSliderPrefab;

    public static Genome queenGenome;
    private List<GeneSlider> sliders = new();

    void Start()
    {
        // Ensure trait definitions are loaded before creating genomes
        Genome.InitializeTraitDefinitions();

        // now safe to create the queen genome
        queenGenome = new Genome();

        foreach (var trait in queenGenome.Traits.Values) //TODO: show only genes that can be modified? or disable ones that can't?
        {
            var currTrait = Instantiate(genesSliderPrefab, genesContainer);
            currTrait.Init(trait);
            currTrait.Intercatable = true;

            sliders.Add(currTrait);
        }
    }

    public void ResetSliders()
    {
        foreach (var slider in sliders)
        {
            slider.Value = slider.GeneRef.Definition.DefaultValue;
        }
    }
}
