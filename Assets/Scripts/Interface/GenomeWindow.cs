using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts
{
    class GenomeWindow : MonoBehaviour
    {
        [SerializeField] private GeneSlider GeneSliderPrefab;
        [SerializeField] private Transform SlidersContainer;

        public static GenomeWindow Instance { get; private set; }

        void Awake()
        {
            Instance = this;
            Hide();
        }

        public void Init(Genome genome)
        {
            Clear();

            GeneSlider geneSlider;
            foreach (var trait in genome.Traits)
            {
                geneSlider = Instantiate(GeneSliderPrefab, SlidersContainer.transform);
                geneSlider.Init(trait.Value);
            }
        }

        public void Clear()
        {
            foreach (Transform slider in SlidersContainer)
            {
                Destroy(slider.gameObject);
            }   
        }

        internal void ShowAnt(Ant ant)
        {
            Clear();
            Init(ant.genome);
            gameObject.SetActive(true);
        }

        public void Hide()
        {
            Clear();
            gameObject.SetActive(false);
        }
    }
}
