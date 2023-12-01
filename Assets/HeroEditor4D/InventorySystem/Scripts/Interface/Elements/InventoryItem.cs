using System;
using System.Collections;
using System.Linq;
using Assets.HeroEditor4D.Common.CommonScripts;
using Assets.HeroEditor4D.InventorySystem.Scripts.Data;
using Assets.HeroEditor4D.InventorySystem.Scripts.Enums;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Assets.HeroEditor4D.InventorySystem.Scripts.Interface.Elements
{
    /// <summary>
    /// Represents inventory item and handles drag & drop operations.
    /// </summary>
    public class InventoryItem : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
    {
        public Image Icon;
        public Image Background;
        public Image Comparer;
        public Image Fragment;
        public Text Count;
        public GameObject Modificator;
        public Toggle Toggle;

        public Item Item;

        private float _clickTime;

        /// <summary>
        /// These actions should be set when inventory UI is opened.
        /// </summary>
        public static Action<Item> OnLeftClick;
		public static Action<Item> OnRightClick;
	    public static Action<Item> OnDoubleClick;
        public static Action<Item> OnMouseEnter;
        public static Action<Item> OnMouseExit;

        private Action _scheduled;

        public void OnEnable()
        {
            if (_scheduled != null)
            {
                StartCoroutine(ExecuteScheduled());
            }
        }

        public void Start()
        {
            if (Icon != null)
            {
                var collection = IconCollection.Active ?? IconCollection.Instances.First().Value;

                Icon.sprite = collection.GetIcon(Item.Params.SpriteId);

                #if TAP_HEROES
                
                TapHeroes.Scripts.Interface.Elements.ItemInfo.SetItemBackground(Item, Background);

                #endif
            }

            if (Fragment)
            {
                Fragment.SetActive(Item.Params.Type == ItemType.Fragment);
            }

            if (Toggle)
            {
                Toggle.group = GetComponentInParent<ToggleGroup>();
            }

            if (Modificator)
            {
                var mod = Item.Modifier != null && Item.Modifier.Id != ItemModifier.None;

                Modificator.SetActive(mod);

                if (mod)
                {
                    string text;

                    switch (Item.Modifier.Id)
                    {
                        case ItemModifier.LevelDown: text = "G-"; break;
                        case ItemModifier.LevelUp: text = "G+"; break;
                        default: text = Item.Modifier.Id.ToString().ToUpper()[0].ToString(); break;
                    }

                    Modificator.GetComponentInChildren<Text>().text = text;
                }
            }
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            StartCoroutine(OnPointerClickDelayed(eventData));
        }

        private IEnumerator OnPointerClickDelayed(PointerEventData eventData) // TODO: A workaround. We should wait for initializing other components.
        {
            yield return null;

            OnPointerClick(eventData.button);
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            OnMouseEnter?.Invoke(Item);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            OnMouseExit?.Invoke(Item);
        }

        public void OnPointerClick(PointerEventData.InputButton button)
        {
            if (button == PointerEventData.InputButton.Left)
            {
                OnLeftClick?.Invoke(Item);

                var delta = Mathf.Abs(Time.time - _clickTime);

                if (delta < 0.5f) // If double click.
                {
                    _clickTime = 0;
                    OnDoubleClick?.Invoke(Item);
                }
                else
                {
                    _clickTime = Time.time;
                }
            }
            else if (button == PointerEventData.InputButton.Right)
            {
                OnRightClick?.Invoke(Item);
            }
        }

        public void Select(bool selected)
        {
            if (Toggle == null) return;

            if (gameObject.activeInHierarchy || !selected)
            {
                Toggle.isOn = selected;
            }
            else
            {
                _scheduled = () => Toggle.isOn = true;
            }

            if (selected)
            {
                OnLeftClick?.Invoke(Item);
            }
        }

        private IEnumerator ExecuteScheduled()
        {
            yield return null;

            _scheduled();
            _scheduled = null;
        }
    }
}