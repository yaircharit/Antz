using UnityEngine;

[CreateAssetMenu(menuName = "Ants/Genome Trait Definition")]
public class GenomeTraitDefinition : ScriptableObject
{
    [Header("Identification")]
    public string Id;
    public string DisplayName;
    public string Description;

    [Header("Values")]
    public float DefaultValue = 0.5f;
    public float MinValue = 0;
    public float MaxValue = 1;
    public float EnergyCostPerUnit = 1f;

    [Header("Genetics")]
    [Range(0f, 1f)]
    public float BaseMutationRate = 0.1f;
    [Range(0f, 1f)]
    public float BaseMutationStrength = 0.1f;
}