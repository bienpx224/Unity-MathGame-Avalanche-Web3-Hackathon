using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Assets.HeroEditor4D.Common.CommonScripts;
using Assets.HeroEditor4D.InventorySystem.Scripts.Data;
using Assets.HeroEditor4D.InventorySystem.Scripts.Enums;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.HeroEditor4D.InventorySystem.Scripts.Interface.Elements
{
    /// <summary>
    /// Scrollable item container that can display item list. Automatic vertical scrolling.
    /// </summary>
    public class ScrollInventory : ItemContainer
    {
        public ScrollRect ScrollRect;
        public GridLayoutGroup Grid;
        public GameObject ItemPrefab;
	    public GameObject ItemNoSpritesPrefab;
		public GameObject CellPrefab;
		public bool AddEmptyCells = true;
        public bool Extend;
        public bool HideCountLabels;
        public Func<Item, bool> GenericFilter; // You can override this.
        public Func<Item, int> GenericSorting = item => TypePriority.IndexOf(item.Params.Type); // You can override this.
        public Action OnRefresh;

		#if TAP_HEROES

        public TMPro.TextMeshProUGUI Gold;

        #endif

		private static readonly List<ItemType> TypePriority = new List<ItemType>
        {
            ItemType.Currency,
            ItemType.Container,
            ItemType.Booster,
            ItemType.Supply,
            ItemType.Weapon,
            ItemType.Helmet,
            ItemType.Armor,
            ItemType.Vest,
            ItemType.Bracers,
            ItemType.Leggings,
            ItemType.Shield,
            ItemType.Fragment,
            ItemType.Backpack,
            ItemType.Jewelry,
            ItemType.Loot,
            ItemType.Recipe,
            ItemType.Material
        };
        private Dictionary<Item, InventoryItem> _inventoryItems = new Dictionary<Item, InventoryItem>(); // Reusing instances to reduce Instantiate() calls.
	    private List<GameObject> _emptyCells = new List<GameObject>();
	    private bool _initialized;
        private int _hash;

        public void Initialize(ref List<Item> items, Item selected, bool reset = false)
        {
            base.Initialize(ref items, selected);

			if (reset) _hash = 0;
		}

        public void Initialize(ref List<Item> items, bool reset = false)
        {
            base.Initialize(ref items);
            ResetNormalizedPosition();

            if (reset) _hash = 0;
        }

        public void SelectItem(Item item)
        {
            if (_inventoryItems.ContainsKey(item))
            {
                _inventoryItems[item].Select(true);
            }
        }

        public bool SelectAny()
        {
            if (_inventoryItems.Count > 0)
            {
                _inventoryItems.First().Value.Select(true);
                
                return true;
            }

            return false;
        }

		public void SetTypeFilter(string input)
        {
            var type = input.ToEnum<ItemType>();

			SetTypeFilter(new List<ItemType> { type });
        }

		public void SetTypeFilter(List<ItemType> types)
        {
            GenericFilter = item => types.Contains(item.Params.Type);
			Refresh(null, force: true);
        }

        public void UnsetFilter()
        {
            GenericFilter = null;
            Refresh(null, force: true);
        }

		public override void Refresh(Item selected)
        {
            Refresh(selected, force: false);
        }

        public void Refresh(Item selected, bool force)
		{
            if (Items == null) return;

            var inventoryItems = new Dictionary<Item, InventoryItem>();
	        var emptyCells = new List<GameObject>();
			var groups = Items.OrderBy(GenericSorting).ToList().GroupBy(i => i.Params.Type);
            var items = new List<Item>();

            foreach (var group in groups)
            {
                items.AddRange(group.OrderBy(i => i.Params.Class).ThenBy(i => i.Params.Price));
            }

            if (GenericFilter != null)
            {
                items.RemoveAll(i => !GenericFilter(i));
			}

            if (!force && _initialized && _hash == GetHash(items))
            {
                var dict = new Dictionary<Item, InventoryItem>();

                for (var i = 0; i < items.Count; i++)
                {
                    var inventoryItem = _inventoryItems.ElementAt(i).Value;

                    inventoryItem.Item = items[i];
					dict.Add(items[i], inventoryItem);
				}

                _inventoryItems = dict;

                return;
            }

            foreach (var item in items)
            {
                InventoryItem inventoryItem;

	            if (_inventoryItems.ContainsKey(item))
	            {
                    inventoryItem = _inventoryItems[item];
		            inventoryItem.transform.SetAsLastSibling();
		            inventoryItems.Add(item, inventoryItem);
		            _inventoryItems.Remove(item);
				}
	            else
	            {
		            var instance = Instantiate(item.Params.SpriteId.IsEmpty() ? ItemNoSpritesPrefab : ItemPrefab, Grid.transform);
					
                    inventoryItem = instance.GetComponent<InventoryItem>();
					inventoryItem.Item = item;
					inventoryItems.Add(item, inventoryItem);
                }

                inventoryItem.Count.text = item.Count.ToString();
                inventoryItem.Count.enabled = !HideCountLabels;

                if (SelectOnRefresh) inventoryItem.Select(item == selected);
			}

			if (AddEmptyCells)
	        {
		        var columns = 0;
		        var rows = 0;

		        switch (Grid.constraint)
		        {
			        case GridLayoutGroup.Constraint.FixedColumnCount:
			        {
				        var height = Mathf.FloorToInt((ScrollRect.GetComponent<RectTransform>().rect.height + Grid.spacing.y) / (Grid.cellSize.y + Grid.spacing.y));

				        columns = Grid.constraintCount;
				        rows = Mathf.Max(height, Mathf.FloorToInt((float) items.Count / columns));

                        if (Extend) rows++;

						break;
			        }
			        case GridLayoutGroup.Constraint.FixedRowCount:
			        {
				        var width = Mathf.FloorToInt((ScrollRect.GetComponent<RectTransform>().rect.width + Grid.spacing.x) / (Grid.cellSize.x + Grid.spacing.x));

				        rows = Grid.constraintCount;
				        columns = Mathf.Max(width, Mathf.FloorToInt((float) items.Count / rows));

                        if (Extend) columns++;

						break;
			        }
		        }

		        for (var i = items.Count; i < columns * rows; i++)
		        {
			        var existing = _emptyCells.LastOrDefault();

			        if (existing != null)
			        {
				        existing.transform.SetAsLastSibling();
				        emptyCells.Add(existing);
				        _emptyCells.Remove(existing);
			        }
			        else
			        {
				        emptyCells.Add(Instantiate(CellPrefab, Grid.transform));
			        }
		        }
	        }

	        foreach (var instance in _inventoryItems.Values)
	        {
		        DestroyImmediate(instance.gameObject);
	        }

	        foreach (var instance in _emptyCells)
	        {
                DestroyImmediate(instance);
	        }

	        _inventoryItems = inventoryItems;
	        _emptyCells = emptyCells;
			_initialized = true;
		    _hash = GetHash(items);

            OnRefresh?.Invoke();

			#if TAP_HEROES

            var gold = Items.Where(i => i.Id == "Gold").Sum(i => i.Count);

			Gold?.SetText($"{gold} <sprite=0>");

            TapHeroes.Scripts.Interface.Elements.ItemComparer.Compare(_inventoryItems.Values.ToList());

            #endif
        }

        public InventoryItem FindItem(Item item)
        {
            return _inventoryItems.Values.SingleOrDefault(i => i.Item.Hash == item.Hash);
        }

        public void ResetNormalizedPosition()
        {
            if (ScrollRect.horizontal) ScrollRect.horizontalNormalizedPosition = 0;
            if (ScrollRect.vertical) ScrollRect.verticalNormalizedPosition = 1;
        }

        public IEnumerator SnapTo(RectTransform target, bool horizontal = true, bool vertical = true)
        {
            yield return null;

            Canvas.ForceUpdateCanvases();

            var pos = (Vector2) ScrollRect.transform.InverseTransformPoint(ScrollRect.content.position) - (Vector2) ScrollRect.transform.InverseTransformPoint(target.position);

            if (!horizontal) pos.x = ScrollRect.content.anchoredPosition.x;
            if (!vertical) pos.y = ScrollRect.content.anchoredPosition.y;

            ScrollRect.content.anchoredPosition = pos;
        }

        public InventoryItem FindItem(string itemId)
        {
            return _inventoryItems.Values.SingleOrDefault(i => i.Item.Id == itemId);
        }

        private static int GetHash(List<Item> items)
        {
            return string.Join(":", items.Select(i => $"{i.Id}.{i.Modifier?.Id}.{i.Modifier?.Level}.{i.Count}")).GetHashCode();
        }
    }
}