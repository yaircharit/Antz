
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GeneSlider : MonoBehaviour
{
    [SerializeField] private Slider slider;
    [SerializeField] private TMP_Text label;
    [SerializeField] private TMP_Text description;
    [SerializeField] private TMP_Text value;

    public void Init(GenomeTrait gene)
    {
        slider.minValue = gene.Definition.MinValue;
        slider.maxValue = gene.Definition.MaxValue;
        slider.value = gene.Value;

        label.text = gene.Definition.DisplayName;
        value.text = gene.Value.ToString("0.000");
    }
}