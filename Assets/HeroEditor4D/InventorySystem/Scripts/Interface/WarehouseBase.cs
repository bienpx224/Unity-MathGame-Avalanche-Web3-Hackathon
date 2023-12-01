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
    public class WarehouseBase : ItemWorkspace
    {
        public ScrollInventory Bag;
        public ScrollInventory Storage;
        public InputField AmountInput;
        public Button PutButton;
        public Button TakeButton;
        public AudioClip MoveSound;
        public AudioSource AudioSource;
        public int Amount;

        public void Initialize(ref List<Item> playerItems, ref  List<Item> storageItems)
        {
            InventoryItem.OnLeftClick = SelectItem;
            InventoryItem.OnRightClick = InventoryItem.OnDoubleClick = OnDoubleClick;
            Bag.Initialize(ref playerItems);
            Storage.Initialize(ref storageItems);

            if (!Bag.SelectAny() && !Storage.SelectAny())
            {
                ItemInfo.Reset();
            }
        }

        private void OnDoubleClick(Item item)
        {
            SelectItem(item);

            if (Bag.Items.Contains(item))
            {
                Take();
            }
            else if (CanMoveSelectedItem())
            {
                Put();
            }
        }

        public void SelectItem(Item item)
        {
            SelectedItem = item;
            SetAmount(1);
            ItemInfo.Initialize(SelectedItem, SelectedItem.Params.Price * Amount);
            Refresh();
        }

        public void Put()
        {
            if (!CanMoveSelectedItem()) return;

            MoveItem(SelectedItem, Bag, Storage, Amount);
            SelectItem(SelectedItem);
            AudioSource.PlayOneShot(MoveSound, SfxVolume);
        }

        public void Take()
        {
            if (!CanMoveSelectedItem()) return;

            MoveItem(SelectedItem, Storage, Bag, Amount);
            SelectItem(SelectedItem);
            AudioSource.PlayOneShot(MoveSound, SfxVolume);
        }

        public override void Refresh()
        {
            if (SelectedItem == null)
            {
                ItemInfo.Reset();
                PutButton.SetActive(false);
                TakeButton.SetActive(false);
            }
            else
            {
                var stored = Storage.Items.Contains(SelectedItem);

                PutButton.SetActive(!stored && CanMoveSelectedItem());
                TakeButton.SetActive(stored && CanMoveSelectedItem());
            }
        }

        public void SetMinAmount()
        {
            SetAmount(1);
        }

        public void IncAmount(int value)
        {
            SetAmount(Amount + value);
        }

        public void SetMaxAmount()
        {
            SetAmount(SelectedItem.Count);
        }

        public void OnAmountChanged(string value)
        {
            if (value.IsEmpty()) return;

            SetAmount(int.Parse(value));
        }

        public void OnAmountEndEdit(string value)
        {
            if (value.IsEmpty())
            {
                SetAmount(1);
            }
        }

        public void Drop()
        {
            foreach (var item in Bag.Items.ToList())
            {
                if (item.Params.Type != ItemType.Currency && !item.Params.Tags.Contains(ItemTag.Quest))
                {
                    #if TAP_HEROES

                    if (item.Params.Class == ItemClass.Gunpowder) continue;

                    #endif

                    MoveItem(item, Bag, Storage, item.Count);
                }
            }

            AudioSource.PlayOneShot(MoveSound, SfxVolume);
        }

        private void SetAmount(int amount)
        {
            Amount = Mathf.Max(1, Mathf.Min(SelectedItem.Count, amount));
            AmountInput?.SetTextWithoutNotify(Amount.ToString());
            ItemInfo.UpdatePrice(SelectedItem, SelectedItem.Params.Price * Amount, false);
        }

        private bool CanMoveSelectedItem()
        {
            return SelectedItem.Params.Type != ItemType.Currency && !SelectedItem.Params.Tags.Contains(ItemTag.Quest);
        }
    }
}