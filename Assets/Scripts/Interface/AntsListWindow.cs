
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.Interface
{
    public class AntsListWindow : MonoBehaviour
    {
        [SerializeField] private GameObject ContentObject;
        [SerializeField] private AntListItem antListItemPrefab;

        private List<AntListItem> antListItems = new List<AntListItem>();

        public void AddAnt(Ant ant)
        {
            AntListItem item = Instantiate(antListItemPrefab, ContentObject.transform);
            item.Init(ant);
            antListItems.Add(item);
        }


    }
}
