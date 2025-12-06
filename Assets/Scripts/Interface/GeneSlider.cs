
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GeneSlider : MonoBehaviour
{
    [SerializeField] private Slider slider;
    [SerializeField] private TMP_Text label;
    [SerializeField] private TMP_Text description;
    [SerializeField] private TMP_Text value;
    
    
    public GenomeTrait GeneRef { get; private set; }

    public bool Intercatable
    {
        get {  return slider.interactable; }
        set { slider.interactable = value; }
    }

    public float Value
    {
        get { return slider.value; }
        set { slider.value = value; }
    }


    public void Init(GenomeTrait gene)
    {
        slider.minValue = gene.Definition.MinValue;
        slider.maxValue = gene.Definition.MaxValue;
        slider.value = gene.Value;

        label.text = gene.Definition.DisplayName;
        value.text = gene.Value.ToString("0.000");

        GeneRef = gene;
        slider.onValueChanged.AddListener(UpdateValue);
    }


    public void UpdateValue(float newValue)
    {
        GeneRef.Value = newValue;
        value.text = newValue.ToString("0.000");
    }
}