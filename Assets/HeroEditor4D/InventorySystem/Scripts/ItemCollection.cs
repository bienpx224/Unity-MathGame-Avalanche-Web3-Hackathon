using System.Collections.Generic;
using System.Linq;
using Assets.HeroEditor4D.Common.CommonScripts;
using Assets.HeroEditor4D.InventorySystem.Scripts.Data;
using UnityEngine;

namespace Assets.HeroEditor4D.InventorySystem.Scripts
{
	public class ItemCollection : MonoBehaviour
	{
		[Header("Required")]
		public IconCollection IconCollection;
		public List<ItemParams> UserItems;
		public List<ItemParams> GeneratedItems;
        public Dictionary<string, ItemParams> Dict;

		public static ItemCollection Instance;

        public void Awake()
        {
            Instance = this;

            if (Dict == null)
            {
                Initialize();
            }
        }

        public void OnValidate()
        {
            SmartSetup();
        }

        public void Initialize()
        {
            Dict = UserItems.Union(GeneratedItems).ToDictionary(i => i.Id, i => i);
        }

        private void SmartSetup()
        {
            foreach (var item in UserItems)
            {
                if (item.Id.IsEmpty()) continue;

                if (item.SpriteId.IsEmpty())
                {
                    var variants = IconCollection.Icons.Where(i => i.Name == item.Id).ToList();

                    if (variants.Count == 1)
                    {
                        item.SpriteId = variants[0].Id;
                        Debug.Log($"{item.Id} icon path resolved!");
                    }
                    else
                    {
                        Debug.LogWarning($"Suggestions for {item.Id}: " + string.Join(", ", variants.Select(i => $"{i.Path}")));
                    }
                }
                else if (IconCollection.Icons.All(i => i.Id != item.SpriteId))
                {
                    Debug.LogWarning($"Icon not found for {item.SpriteId}!");
                }
            }
        }
	}
}