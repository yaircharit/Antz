using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.TerrainUtils;
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

            ant.OnDamageTaken += (x) => UpdateHealth(ant.currentHealth);
            ant.OnDamageTaken += (x) => FlashColor(ant.damageColor, ant.damageFlashDuration);

            ant.OnHealed += (x) => UpdateHealth(ant.currentHealth);
            ant.OnHealed += (x) => FlashColor(ant.healColor, ant.healFlashDuration);
            
            ant.OnSelected += () => Select(ant.highlightColor);
            ant.OnDeselected += () => Deselect(Color.white);
            ant.OnDestroyed += () => Destroy(gameObject);
            ant.OnStateChanged += (state) =>
            {
                antStatus.color = state.StateColor;
            };
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

        public void Select(Color highlightColor)
        {
            background.color = highlightColor;
        }
        public void Deselect(Color baseColor)
        {
            background.color = baseColor;
        }

        public IEnumerable FlashColor(Color color, float duration)
        {
            Color prevColor = background.material.color;
            background.material.color = Color.Lerp(prevColor, color, 0.5f);
            yield return new WaitForSeconds(duration);
            background.material.color = prevColor;
        }

        public void ResetColor()
        {
        }

        void IPointerClickHandler.OnPointerClick(PointerEventData eventData)
        {
            OnSelect?.Invoke();
        }
    }
}
