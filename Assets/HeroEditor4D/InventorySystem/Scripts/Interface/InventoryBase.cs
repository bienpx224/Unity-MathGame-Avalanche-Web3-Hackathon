using System;
using System.Collections.Generic;
using System.Linq;
using Assets.HeroEditor4D.Common.CommonScripts;
using Assets.HeroEditor4D.InventorySystem.Scripts.Data;
using Assets.HeroEditor4D.InventorySystem.Scripts.Enums;
using Assets.HeroEditor4D.InventorySystem.Scripts.Interface.Elements;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.HeroEditor4D.InventorySystem.Scripts.Interface
{
    /// <summary>
    /// High-level inventory interface.
    /// </summary>
    public class InventoryBase : ItemWorkspace
    {
	    public ItemCollection ItemCollection;
		public Equipment Equipment;
        public ScrollInventory Bag;
        public ScrollInventory Materials;
        public Button EquipButton;
        public Button RemoveButton;
        public Button CraftButton;
        public Button LearnButton;
        public Button UseButton;
        public Button AssembleButton;
        public AudioClip EquipSound;
        public AudioClip CraftSound;
        public AudioClip UseSound;
        public AudioSource AudioSource;
        public bool InitializeExample;

        // These callbacks can be used outside;
        public Action<Item> OnRefresh;
        public Action<Item> OnEquip;
        public Func<Item, bool> CanEquip = i => true;

        public void Start()
        {
            if (IconCollection.Active == null) IconCollection.Active = IconCollection.Instances["FantasyHeroes"];

            if (InitializeExample)
            {
                Initialize();
                Reset();
            }
        }

        /// <summary>
        /// Initialize owned items (just for example).
        /// </summary>
        public void Initialize()
        {
            InventoryItem.OnLeftClick = SelectItem;
            InventoryItem.OnRightClick = InventoryItem.OnDoubleClick = OnDoubleClick;

            var inventory = ItemCollection.UserItems.Select(i => new Item(i.Id)).ToList(); // inventory.Clear();
			var equipped = new List<Item>();

            Bag.Initialize(ref inventory);
            Equipment.Initialize(ref equipped);
		}

        public void Initialize(ref List<Item> playerItems, ref  List<Item> equippedItems, int bagSize, Action onRefresh)
        {
            InventoryItem.OnLeftClick = SelectItem;
            InventoryItem.OnRightClick = InventoryItem.OnDoubleClick = OnDoubleClick;
            Bag.Initialize(ref playerItems);
            Equipment.SetBagSize(bagSize);
            Equipment.Initialize(ref equippedItems);
            Equipment.OnRefresh = onRefresh;

            if (!Equipment.SelectAny() && !Bag.SelectAny())
            {
                ItemInfo.Reset();
            }
        }

        private void OnDoubleClick(Item item)
        {
            SelectItem(item);

            if (Equipment.Items.Contains(item))
            {
                Remove();
            }
            else if (CanEquipSelectedItem())
            {
                Equip();
            }
        }

        public void SelectItem(Item item)
        {
            SelectedItem = item;
            ItemInfo.Initialize(SelectedItem, SelectedItem.Params.Price);
            Refresh();
        }

        public void Equip()
        {
            if (!CanEquip(SelectedItem)) return;

            var equipped = SelectedItem.IsFirearm
                ? Equipment.Items.Where(i => i.IsFirearm).ToList()
                : Equipment.Items.Where(i => i.Params.Type == SelectedItem.Params.Type && !i.IsFirearm).ToList();

            if (equipped.Any())
            {
                AutoRemove(equipped, Equipment.Slots.Count(i => i.Supports(SelectedItem)));
            }

            if (SelectedItem.IsTwoHanded) AutoRemove(Equipment.Items.Where(i => i.IsShield).ToList());
            if (SelectedItem.IsShield) AutoRemove(Equipment.Items.Where(i => i.IsWeapon && i.IsTwoHanded).ToList());

            if (SelectedItem.IsFirearm) AutoRemove(Equipment.Items.Where(i => i.IsShield).ToList());
            if (SelectedItem.IsFirearm) AutoRemove(Equipment.Items.Where(i => i.IsWeapon && i.IsTwoHanded).ToList());
            if (SelectedItem.IsTwoHanded || SelectedItem.IsShield) AutoRemove(Equipment.Items.Where(i => i.IsWeapon && i.IsFirearm).ToList());

            MoveItem(SelectedItem, Bag, Equipment);
            AudioSource.PlayOneShot(EquipSound, SfxVolume);
            OnEquip?.Invoke(SelectedItem);
        }

        public void Remove()
        {
            MoveItem(SelectedItem, Equipment, Bag);
            SelectItem(SelectedItem);
            AudioSource.PlayOneShot(EquipSound, SfxVolume);
        }

        public void Craft()
        {
            var materials = MaterialList;

            if (CanCraft(materials))
            {
                materials.ForEach(i => Bag.Items.Single(j => j.Hash == i.Hash).Count -= i.Count);
                Bag.Items.RemoveAll(i => i.Count == 0);

                var itemId = SelectedItem.Params.FindProperty(PropertyId.Craft).Value;
                var existed = Bag.Items.SingleOrDefault(i => i.Id == itemId && i.Modifier == null);

                if (existed == null)
                {
                    Bag.Items.Add(new Item(itemId));
                }
                else
                {
                    existed.Count++;
                }

                Bag.Refresh(SelectedItem);
                CraftButton.interactable = CanCraft(materials);
                AudioSource.PlayOneShot(CraftSound, SfxVolume);
            }
            else
            {
                Debug.Log("No materials.");
            }
        }

        public void Learn()
        {
            // Implement your logic here!
        }

        public void Use()
        {
            Use(UseSound);
        }

        public void Use(AudioClip sound)
        {
            if (SelectedItem.Count == 1)
            {
                Bag.Items.Remove(SelectedItem);
                SelectedItem = Bag.Items.FirstOrDefault();

                if (SelectedItem == null)
                {
                    Bag.Refresh(null);
                    SelectedItem = Equipment.Items.FirstOrDefault();

                    if (SelectedItem != null)
                    {
                        Equipment.Refresh(SelectedItem);
                    }
                }
                else
                {
                    Bag.Refresh(SelectedItem);
                }
            }
            else
            {
                SelectedItem.Count--;
                Bag.Refresh(SelectedItem);
            }

            Equipment.OnRefresh?.Invoke();

            if (sound != null)
            {
                AudioSource.PlayOneShot(sound, SfxVolume);
            }
        }

        public Item Assemble()
        {
            if (SelectedItem != null && SelectedItem.Params.Type == ItemType.Fragment && SelectedItem.Count >= SelectedItem.Params.FindProperty(PropertyId.Fragments).ValueInt)
            {
                SelectedItem.Count -= SelectedItem.Params.FindProperty(PropertyId.Fragments).ValueInt;

                var crafted = new Item(SelectedItem.Params.FindProperty(PropertyId.Craft).Value);
                var existed = Bag.Items.SingleOrDefault(i => i.Hash == crafted.Hash);

                if (existed == null)
                {
                    Bag.Items.Add(crafted);
                }
                else
                {
                    existed.Count++;
                }

                if (SelectedItem.Count == 0)
                {
                    Bag.Items.Remove(SelectedItem);
                    SelectedItem = crafted;
                }

                Bag.Refresh(SelectedItem);

                return crafted;
            }

            return null;
        }

        public override void Refresh()
        {
            if (SelectedItem == null)
            {
                ItemInfo.Reset();
                EquipButton.SetActive(false);
                RemoveButton.SetActive(false);
            }
            else
            {
                var equipped = Equipment.Items.Contains(SelectedItem);

                EquipButton.SetActive(!equipped && CanEquipSelectedItem());
                RemoveButton.SetActive(equipped);
            }

            UseButton.SetActive(SelectedItem != null && CanUse());
            AssembleButton.SetActive(SelectedItem != null && SelectedItem.Params.Type == ItemType.Fragment && SelectedItem.Count >= SelectedItem.Params.FindProperty(PropertyId.Fragments).ValueInt);

            var receipt = SelectedItem != null && SelectedItem.Params.Type == ItemType.Recipe;

            if (CraftButton != null) CraftButton.SetActive(false);
            if (LearnButton != null) LearnButton.SetActive(false);

            if (receipt)
            {
                if (LearnButton == null)
                {
                    var materialSelected = !Bag.Items.Contains(SelectedItem) && !Equipment.Items.Contains(SelectedItem);

                    CraftButton.SetActive(true);
                    Materials.SetActive(materialSelected);
                    Equipment.Scheme.SetActive(!materialSelected);

                    var materials = MaterialList;

                    Materials.Initialize(ref materials);
                }
                else
                {
                    LearnButton.SetActive(true);
                }
            }

            OnRefresh?.Invoke(SelectedItem);
        }

        private List<Item> MaterialList => SelectedItem.Params.FindProperty(PropertyId.Materials).Value.Split(',').Select(i => i.Split(':')).Select(i => new Item(i[0], int.Parse(i[1]))).ToList();

        private bool CanEquipSelectedItem()
        {
            return Bag.Items.Contains(SelectedItem) && Equipment.Slots.Any(i => i.Supports(SelectedItem));
        }

        private bool CanUse()
        {
            switch (SelectedItem.Params.Type)
            {
                case ItemType.Container:
                case ItemType.Booster:
                case ItemType.Coupon:
                    return true;
                default:
                    return false;
            }
        }

        private bool CanCraft(List<Item> materials)
        {
            return materials.All(i => Bag.Items.Any(j => j.Hash == i.Hash && j.Count >= i.Count));
        }

        /// <summary>
        /// Automatically removes items if target slot is busy.
        /// </summary>
        private void AutoRemove(List<Item> items, int max = 1)
        {
            long sum = 0;

            foreach (var p in items)
            {
                sum += p.Count;
            }

            if (sum == max)
            {
                MoveItemSilent(items.LastOrDefault(i => i.Id != SelectedItem.Id) ?? items.Last(), Equipment, Bag);
            }
        }
    }
}