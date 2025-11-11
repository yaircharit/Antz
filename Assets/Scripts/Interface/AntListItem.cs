using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.Interface
{
    public class AntListItem : MonoBehaviour
    {
        [SerializeField] private TMP_Text antNameText;
        [SerializeField] private RawImage antStatus;
        [SerializeField] private Slider healthBar;
        [SerializeField] private Slider energyBar;
        [SerializeField] private Button moreButton;

        private Ant ant;

        internal void Init(Ant ant)
        {
            this.ant = ant;

            antNameText.text = ant.Name;
            healthBar.maxValue = ant.maxHealth;
            healthBar.value = ant.currentHealth;
            energyBar.maxValue = ant.maxEnergy;
            energyBar.value = ant.currentEnergy;

            ant.OnEnergyChanged += UpdateEnergy;
            ant.OnHealthChanged += UpdateHealth;
            //ant.OnDestroyed += () => Destroy();
        }

        public void UpdateHealth(float currentHealth)
        {
            healthBar.value = currentHealth;
        }

        public void UpdateEnergy(float currentEnergy)
        {
            energyBar.value = currentEnergy;
        }

        private void OnMouseDown()
        {
            ant.Select();
        }
    }
}
