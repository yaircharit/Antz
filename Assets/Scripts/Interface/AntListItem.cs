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

        private Coroutine flashCoroutine;

        internal void Init(Ant ant)
        {
            antNameText.text = ant.Name;
            healthBar.maxValue = ant.maxHealth;
            healthBar.value = ant.currentHealth;
            energyBar.maxValue = ant.maxEnergy;
            energyBar.value = ant.currentEnergy;

            ant.OnEnergyChanged += (_) => UpdateEnergy(ant.currentEnergy);

            ant.OnDamageTaken += (_) => UpdateHealth(ant.currentHealth);
            ant.OnDamageTaken += (_) => {
                flashCoroutine ??= StartCoroutine(FlashColor(MovingEntity.damageColor, MovingEntity.damageFlashDuration));
                };

            ant.OnHealed += (_) => UpdateHealth(ant.currentHealth);
            ant.OnHealed += (_) => {
                flashCoroutine ??= StartCoroutine(FlashColor(MovingEntity.healColor, MovingEntity.healFlashDuration));
            };

            ant.OnSelected += () => Select(MovingEntity.highlightColor);
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

        public IEnumerator FlashColor(Color color, float duration)
        {
            if (flashCoroutine != null)
            {
                yield break;
            }

            Color prevColor = background.color;
            background.color = color;
            yield return new WaitForSeconds(duration);
            background.color = prevColor;
            yield return new WaitForSeconds(duration);
            flashCoroutine = null;
        }

        void IPointerClickHandler.OnPointerClick(PointerEventData eventData)
        {
            OnSelect?.Invoke();
        }
    }
}
