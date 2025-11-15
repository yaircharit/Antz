using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Assets.Scripts.Interface
{
    public class AntListItem : MonoBehaviour, IPointerClickHandler
    {
        [SerializeField] private TMP_Text antNameText;
        [SerializeField] private RawImage antStatus;
        [SerializeField] private Slider healthBar;
        [SerializeField] private Slider energyBar;
        [SerializeField] private Button moreButton;
        [SerializeField] private Image background;

        private Action OnSelect;

        internal void Init(Ant ant)
        {
            antNameText.text = ant.Name;
            healthBar.maxValue = ant.maxHealth;
            healthBar.value = ant.currentHealth;
            energyBar.maxValue = ant.maxEnergy;
            energyBar.value = ant.currentEnergy;

            ant.OnEnergyChanged += UpdateEnergy;
            ant.OnHealthChanged += UpdateHealth;
            ant.OnSelected += Select;
            ant.OnDeselected += Deselect;
            ant.OnDestroyed += () => Destroy(gameObject);
            OnSelect += ant.RaiseOnSelected;
        }

        public void UpdateHealth(float currentHealth)
        {
            healthBar.value = currentHealth;
        }

        public void UpdateEnergy(float currentEnergy)
        {
            energyBar.value = currentEnergy;
        }

        public void Select()
        {
            background.color = Color.yellow;
        }
        public void Deselect()
        {
            background.color = Color.white;
        }

        public void FlashColor(Color color)
        {
            background.material.color = Color.Lerp(Color.white, color, 0.5f);
            Invoke(nameof(ResetColor) , 0.2f);
        }

        public void ResetColor()
        {
            background.material.color = Color.white;
        }

        void IPointerClickHandler.OnPointerClick(PointerEventData eventData)
        {
            OnSelect?.Invoke();
        }
    }
}
