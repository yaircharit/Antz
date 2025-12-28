using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameSetup : MonoBehaviour
{
    [SerializeField] Transform genesContainer;
    [SerializeField] GeneSlider genesSliderPrefab;

    public static Genome queenGenome = new();
    private List<GeneSlider> sliders = new();

    void Start()
    {
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
