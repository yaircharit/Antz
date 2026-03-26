using UnityEngine;

[CreateAssetMenu(menuName = "Ants/Blocks Block Definition")]
public class BlockDefinition : ScriptableObject
{
    [Header("Identification")]
    public string Id;
    public string DisplayName;
    public bool IsSolid;

    [Header("Rendering")]
    public Material material;
}