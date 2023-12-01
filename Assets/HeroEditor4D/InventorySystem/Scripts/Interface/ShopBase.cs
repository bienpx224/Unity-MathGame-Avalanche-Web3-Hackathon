using System;
using System.Collections.Generic;
using System.Linq;
using Assets.HeroEditor4D.Common.CharacterScripts;
using Assets.HeroEditor4D.Common.CommonScripts;
using Assets.HeroEditor4D.InventorySystem.Scripts.Data;
using Assets.HeroEditor4D.InventorySystem.Scripts.Enums;
using Assets.HeroEditor4D.InventorySystem.Scripts.Interface.Elements;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.HeroEditor4D.InventorySystem.Scripts.Interface
{
    /// <summary>
    /// High-level shop interface.
    /// </summary>
    public class ShopBase : ItemWorkspace
    {
        public ScrollInventory Trader;
        public ScrollInventory Bag;
        public InputField AmountInput;
        public Button BuyButton;
        public Button SellButton;
        public AudioSource AudioSource;
        public AudioClip TradeSound;
        public AudioClip NoMoney;
        public Character Dummy;
        public bool ExampleInitialize;

        public string CurrencyId = "Gold";
	    public int Amount;

        // These callbacks can be used outside;
        public Action<Item> OnRefresh;
        public Action<Item> OnBuy;
        public Action<Item> OnSell;

        public virtual bool CanBuy(Item item) // Override this function to fit your needs!
        {
            return true;
        }

        public virtual bool CanSell(Item item) // Override this function to fit your needs!
        {
            return true;
        }

        public virtual int GetPrice(Item item) // Override this function to fit your needs!
        {
            var trader = Trader.Items.Contains(item);
            var price = item.Params.Price * Amount;

            if (trader)
            {
                price *= GetTraderMarkup(item);
            }

            return price;
        }

        public static int GetTraderMarkup(Item item) // Override this function to fit your needs!
        {
            if (item.Params.Rarity > ItemRarity.Common) return 2;

            switch (item.Params.Type)
            {
                case ItemType.Weapon:
                case ItemType.Armor:
                case ItemType.Helmet:
                case ItemType.Shield:
                case ItemType.Backpack: return 3;
                default: return 2;
            }
        }

        public void Start()
        {
            if (IconCollection.Active == null) IconCollection.Active = IconCollection.Instances["FantasyHeroes"];

            if (ExampleInitialize)
            {
                Initialize();
            }
        }

        public void Subscribe()
        {
            InventoryItem.OnLeftClick = SelectItem;
            InventoryItem.OnRightClick = InventoryItem.OnDoubleClick = item => { SelectItem(item); if (Trader.Items.Contains(item)) Buy(); else Sell(); };
        }

        /// <summary>
        /// Initialize owned items and trader items (just for example).
        /// </summary>
        public void Initialize()
        {
            var inventory = new List<Item> { new Item(CurrencyId, 10000) };
            var shop = ItemCollection.Instance.Dict.Values.Select(i => new Item(i.Id, 2)).ToList();

            shop.Single(i => i.Id == CurrencyId).Count = 99999;

            Subscribe();
            Trader.Initialize(ref shop);
            Bag.Initialize(ref inventory);

            if (!Trader.SelectAny() && !Bag.SelectAny())
            {
                ItemInfo.Reset();
            }
        }

        public void Initialize(ref List<Item> traderItems, ref List<Item> playerItems)
        {
            Subscribe();
            Trader.Initialize(ref traderItems);
            Bag.Initialize(ref playerItems);

            if (!Trader.SelectAny() && !Bag.SelectAny())
            {
                ItemInfo.Reset();
            }
        }

	    public void SelectItem(Item item)
        {
            SelectedItem = item;
            SetAmount(1);
            ItemInfo.Initialize(SelectedItem, GetPrice(SelectedItem), Trader.Items.Contains(item));
            Refresh();
        }

        public void Buy()
        {
			if (!BuyButton.gameObject.activeSelf || !BuyButton.interactable || !CanBuy(SelectedItem)) return;

            var item = SelectedItem;
            var price = GetPrice(item);

            if (GetCurrency(Bag, CurrencyId) < price)
            {
                #if TAP_HEROES

                TapHeroes.Scripts.Interface.Popup.Instance.ShowMessage(SimpleLocalization.LocalizationManager.Localize("Common.NoFunds", "[" + TapHeroes.Scripts.Extensions.GetLocalizedName(new Item(CurrencyId).Params) + "]"), CurrencyId, NoMoney);
                
                #else

                Debug.LogWarning("You don't have enough gold!");
                AudioSource.PlayOneShot(NoMoney, SfxVolume);

                #endif

                return;
            }

            OnBuy?.Invoke(item);
            AddMoney(Bag, -price, CurrencyId);
			AddMoney(Trader, price, CurrencyId);
            MoveItem(item, Trader, Bag, Amount, currencyId: CurrencyId);
            AudioSource.PlayOneShot(TradeSound, SfxVolume);

            #if TAP_HEROES
            TapHeroes.Scripts.Interface.Tutorial.Instance.OnBuyItem(item.Id); // TODO: Create OnBuyCallback;
            #endif
        }

        public void Sell()
        {
	        if (!SellButton.gameObject.activeSelf || !SellButton.interactable || !CanSell(SelectedItem)) return;

            var price = GetPrice(SelectedItem);

            if (GetCurrency(Trader, CurrencyId) < price)
            {
                #if TAP_HEROES

                TapHeroes.Scripts.Interface.Popup.Instance.ShowMessage(SimpleLocalization.LocalizationManager.Localize("Common.NoFunds", "[" + TapHeroes.Scripts.Extensions.GetLocalizedName(new Item(CurrencyId).Params) + "]"), CurrencyId, NoMoney);

                #else

                Debug.LogWarning("Trader doesn't have enough gold!");
                AudioSource.PlayOneShot(NoMoney, SfxVolume);

                #endif

                return;
            }

            OnSell?.Invoke(SelectedItem);
            AddMoney(Bag, price, CurrencyId);
            AddMoney(Trader, -price, CurrencyId);
            MoveItem(SelectedItem, Bag, Trader, Amount, currencyId: CurrencyId);
            AudioSource.PlayOneShot(TradeSound, SfxVolume);
        }

        public override void Refresh()
        {
            if (SelectedItem == null)
            {
                ItemInfo.Reset();
                BuyButton.SetActive(false);
                SellButton.SetActive(false);
            }
            else
            {
                if (Trader.Items.Contains(SelectedItem))
                {
                    InitBuy();
                }
                else if (Bag.Items.Contains(SelectedItem))
                {
                    InitSell();
                }
                else if (Trader.Items.Any(i => i.Hash == SelectedItem.Hash))
                {
                    InitBuy();
                }
                else if (Bag.Items.Any(i => i.Hash == SelectedItem.Hash))
                {
                    InitSell();
                }
            }

            OnRefresh?.Invoke(SelectedItem);
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

        private void SetAmount(int amount)
        {
            Amount = Mathf.Max(1, Mathf.Min(SelectedItem.Count, amount));
            AmountInput?.SetTextWithoutNotify(Amount.ToString());
            ItemInfo.UpdatePrice(SelectedItem, GetPrice(SelectedItem), Trader.Items.Contains(SelectedItem));
        }

        private void InitBuy()
        {
            BuyButton.SetActive(SelectedItem.Params.Type != ItemType.Currency && SelectedItem.Count > 0 && !SelectedItem.Params.Tags.Contains(ItemTag.NotForSale) && !SelectedItem.Params.Tags.Contains(ItemTag.Quest) && CanBuy(SelectedItem));
            SellButton.SetActive(false);
            //BuyButton.interactable = GetCurrency(Bag, CurrencyId) >= SelectedItem.Params.Price;
        }

        private void InitSell()
        {
            BuyButton.SetActive(false);
            SellButton.SetActive(SelectedItem.Count > 0 && !SelectedItem.Params.Tags.Contains(ItemTag.NotForSale) && !SelectedItem.Params.Tags.Contains(ItemTag.Quest) && SelectedItem.Id != CurrencyId && CanSell(SelectedItem));
            //SellButton.interactable = GetCurrency(Trader, CurrencyId) >= SelectedItem.Params.Price;
        }

        public static long GetCurrency(ItemContainer bag, string currencyId)
        {
            var currency = bag.Items.SingleOrDefault(i => i.Id == currencyId);

            return currency?.Count ?? 0;
        }

        private static void AddMoney(ItemContainer inventory, int value, string currencyId)
        {
            var currency = inventory.Items.SingleOrDefault(i => i.Id == currencyId);

            if (currency == null)
            {
                inventory.Items.Insert(0, new Item(currencyId, value));
            }
            else
            {
                currency.Count += value;

                if (currency.Count == 0)
                {
                    inventory.Items.Remove(currency);
                }
            }
        }
    }
}