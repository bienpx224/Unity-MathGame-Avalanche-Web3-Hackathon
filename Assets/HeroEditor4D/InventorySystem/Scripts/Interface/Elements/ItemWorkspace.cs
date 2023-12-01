﻿using System;
using System.Linq;
using Assets.HeroEditor4D.InventorySystem.Scripts.Data;
using UnityEngine;

namespace Assets.HeroEditor4D.InventorySystem.Scripts.Interface.Elements
{
    /// <summary>
    /// Abstract item workspace. It can be shop or player inventory. Items can be managed here (selected, moved and so on).
    /// </summary>
    public abstract class ItemWorkspace : MonoBehaviour
    {
        public ItemInfo ItemInfo;

        public static float SfxVolume = 1;

        public Item SelectedItem { get; protected set; }
        
        public abstract void Refresh();

        protected void Reset()
        {
            SelectedItem = null;
            ItemInfo.Reset();
        }

        protected void MoveItem(Item item, ItemContainer from, ItemContainer to, int amount = 1, string currencyId = null)
        {
            MoveItemSilent(item, from, to, amount);
            
            var moved = to.Items.Last(i => i.Hash == item.Hash);

            if (from.Expanded)
            {
                SelectedItem = from.Items.LastOrDefault(i => i.Hash == item.Hash) ?? moved;
            }
            else
            {
                if (item.Count == 0)
                {
                    SelectedItem = currencyId == null ? moved : from.Items.Single(i => i.Id == currencyId);
                }
            }

            Refresh();
            from.Refresh(SelectedItem);
            to.Refresh(SelectedItem);
        }

        public void MoveItemSilent(Item item, ItemContainer from, ItemContainer to, int amount = 1)
        {
            if (item.Count <= 0) throw new ArgumentException("item.Count <= 0");
            if (amount <= 0) throw new ArgumentException("amount <= 0");
            if (item.Count < amount) throw new ArgumentException("item.Count < amount");

            if (to.Expanded)
            {
                to.Items.Add(new Item(item.Id, item.Modifier, amount));
            }
            else
            {
                var targets = to.Items.Where(i => i.Hash == item.Hash).ToList();

                switch (targets.Count)
                {
                    case 0:
                        to.Items.Add(new Item(item.Id, item.Modifier, amount));
                        break;
                    case 1:
                        targets[0].Count += amount;
                        break;
                    default:
                        throw new Exception($"Unable to move item silently: {item.Id} ({item.Hash}).");
                }
            }

            var moved = to.Items.Last(i => i.Hash == item.Hash);

            if (from.Expanded)
            {
                from.Items.Remove(item);
            }
            else
            {
                item.Count -= amount;

                if (item.Count == 0)
                {
                    from.Items.Remove(item);
                }
            }
        }
    }
}