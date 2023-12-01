using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Assets.HeroEditor4D.Common.CharacterScripts;
using Assets.HeroEditor4D.Common.CommonScripts;
using Assets.HeroEditor4D.InventorySystem.Scripts.Data;
using Assets.HeroEditor4D.InventorySystem.Scripts.Interface.Elements;
using Assets.HeroEditor4D.Common.SimpleColorPicker.Scripts;
using HeroEditor4D.Common;
using HeroEditor4D.Common.Enums;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.HeroEditor4D.Common.EditorScripts
{
    /// <summary>
    /// Character editor UI and behaviour.
    /// </summary>
    public class CharacterEditor : CharacterEditorBase
    {
        [Header("Public")]
        public Transform Tabs;
        public ScrollInventory Inventory;
        public Text ItemName;

        [Header("Other")]
        public List<string> PaintParts;
        public Button PaintButton;
        public ColorPicker ColorPicker;
        public string Path;

        public Action<Item> EquipCallback;

        public Character4D Character4D => (Character4D) Character;

        private Toggle ActiveTab => Tabs.GetComponentsInChildren<Toggle>().Single(i => i.isOn);

        public void OnValidate()
        {
            if (Character == null)
            {
                Character = FindObjectOfType<Character4D>();
            }
        }

        public new void Start()
        {
            base.Start();
            OnSelectTab(true);
        }

        /// <summary>
        /// This can be used as an example for building your own inventory UI.
        /// </summary>
        public void OnSelectTab(bool value)
        {
            if (!value) return;

            Item.GetParams = null;

            Dictionary<string, SpriteGroupEntry> dict;
            Action<Item> equipAction;
            int equippedIndex;
            var tab = Tabs.GetComponentsInChildren<Toggle>().Single(i => i.isOn);

            switch (tab.name)
            {
                case "Armor":
                {
                    dict = SpriteCollection.Armor.ToDictionary(i => i.Id, i => i);
                    equipAction = item => Character4D.Equip(dict[item.Id], EquipmentPart.Armor);
                    equippedIndex = Character.Front.Armor == null ? -1 : SpriteCollection.Armor.FindIndex(i => i.Sprites.Contains(Character.Front.Armor.SingleOrDefault(j => j.name == "FrontBody")));
                    break;
                }
                case "Helmet":
                {
                    dict = SpriteCollection.Armor.ToDictionary(i => i.Id, i => i);
                    equipAction = item => Character4D.Equip(dict[item.Id], EquipmentPart.Helmet);
                    equippedIndex = SpriteCollection.Armor.FindIndex(i => i.Sprites.Contains(Character.Front.Helmet));
                    Item.GetParams = item => CreateFakeItemParams(item, dict[item.Id], ".Armor.", ".Helmet.");
                    break;
                }
                case "Vest":
                case "Bracers":
                case "Leggings":
                {
                    string part;

                    switch (tab.name)
                    {
                        case "Vest": part = "FrontBody"; break;
                        case "Bracers": part = "FrontArmL"; break;
                        case "Leggings": part = "FrontLegL"; break;
                        default: throw new NotSupportedException(tab.name);
                    }

                    dict = SpriteCollection.Armor.ToDictionary(i => i.Id, i => i);
                    equipAction = item => Character.Equip(dict[item.Id], tab.name.ToEnum<EquipmentPart>());
                    equippedIndex = Character.Front.Armor == null ? -1 : SpriteCollection.Armor.FindIndex(i => i.Sprites.Contains(Character.Front.Armor.SingleOrDefault(j => j.name == part)));
                    Item.GetParams = item => CreateFakeItemParams(item, dict[item.Id], ".Armor.", $".{tab.name}.");
                    break;
                }
                case "Shield":
                {
                    dict = SpriteCollection.Shield.ToDictionary(i => i.Id, i => i);
                    equipAction = item => Character4D.Equip(dict[item.Id], EquipmentPart.Shield);
                    equippedIndex = Character.Front.Shield == null ? -1 : SpriteCollection.Shield.FindIndex(i => i.Sprites.SequenceEqual(Character.Front.Shield));
                    break;
                }
                case "Melee1H":
                {
                    dict = SpriteCollection.MeleeWeapon1H.ToDictionary(i => i.Id, i => i);
                    equipAction = item => Character4D.Equip(dict[item.Id], EquipmentPart.MeleeWeapon1H);
                    equippedIndex = SpriteCollection.MeleeWeapon1H.FindIndex(i => i.Sprites.Contains(Character.Front.PrimaryWeapon));
                    break;
                }
                case "Melee2H":
                {
                    dict = SpriteCollection.MeleeWeapon2H.ToDictionary(i => i.Id, i => i);
                    equipAction = item => Character4D.Equip(dict[item.Id], EquipmentPart.MeleeWeapon2H);
                    equippedIndex = SpriteCollection.MeleeWeapon2H.FindIndex(i => i.Sprites.Contains(Character.Front.PrimaryWeapon));
                    break;
                }
                case "Bow":
                {
                    dict = SpriteCollection.Bow.ToDictionary(i => i.Id, i => i);
                    equipAction = item => Character4D.Equip(dict[item.Id], EquipmentPart.Bow);
                    equippedIndex = Character.Front.CompositeWeapon == null ? -1 : SpriteCollection.Bow.FindIndex(i => i.Sprites.SequenceEqual(Character.Front.CompositeWeapon));
                    break;
                }
                case "Crossbow":
                {
                    dict = SpriteCollection.Crossbow.ToDictionary(i => i.Id, i => i);
                    equipAction = item => Character4D.Equip(dict[item.Id], EquipmentPart.Crossbow);
                    equippedIndex = Character.Front.CompositeWeapon == null ? -1 : SpriteCollection.Crossbow.FindIndex(i => i.Sprites.SequenceEqual(Character.Front.CompositeWeapon));
                    break;
                }
                case "Firearm1H":
                {
                    dict = SpriteCollection.Firearm1H.ToDictionary(i => i.Id, i => i);
                    equipAction = item => Character4D.Equip(dict[item.Id], EquipmentPart.Firearm1H);
                    equippedIndex = Character.Front.SecondaryWeapon == null ? -1 : SpriteCollection.Firearm1H.FindIndex(i => i.Sprites.Contains(Character.Front.SecondaryWeapon));
                    break;
                }
                case "Firearm2H":
                {
                    dict = SpriteCollection.Firearm2H.ToDictionary(i => i.Id, i => i);
                    equipAction = item => Character4D.Equip(dict[item.Id], EquipmentPart.Firearm2H);
                    equippedIndex = Character.Front.PrimaryWeapon == null ? -1 : SpriteCollection.Firearm2H.FindIndex(i => i.Sprites.Contains(Character.Front.PrimaryWeapon));
                    break;
                }
                case "SecondaryFirearm1H":
                {
                    dict = SpriteCollection.Firearm1H.ToDictionary(i => i.Id, i => i);
                    equipAction = item => Character4D.Equip(dict[item.Id], EquipmentPart.SecondaryFirearm1H);
                    equippedIndex = Character.Front.SecondaryWeapon == null ? -1 : SpriteCollection.Firearm1H.FindIndex(i => i.Sprites.Contains(Character.Front.SecondaryWeapon));
                    break;
                }
                case "Body":
                {
                    dict = SpriteCollection.Body.ToDictionary(i => i.Id, i => i);
                    equipAction = item => Character4D.SetBody(dict[item.Id], BodyPart.Body);
                    equippedIndex = Character.Front.Body == null ? -1 : SpriteCollection.Body.FindIndex(i => i.Sprites.SequenceEqual(Character.Front.Body));
                    break;
                }
                case "Head":
                {
                    dict = SpriteCollection.Body.ToDictionary(i => i.Id, i => i);
                    equipAction = item => Character4D.SetBody(dict[item.Id], BodyPart.Head);
                    equippedIndex = Character.Front.Head == null ? -1 : SpriteCollection.Body.FindIndex(i => i.Sprites.Contains(Character.Front.Head));
                    break;
                }
                case "Ears":
                {
                    dict = SpriteCollection.Ears.ToDictionary(i => i.Id, i => i);
                    equipAction = item => Character4D.SetBody(dict[item.Id], BodyPart.Ears);
                    equippedIndex = Character.Front.Ears == null ? -1 : SpriteCollection.Ears.FindIndex(i => i.Sprites.SequenceEqual(Character.Front.Ears));
                    break;
                }
                case "Eyebrows":
                {
                    dict = SpriteCollection.Eyebrows.ToDictionary(i => i.Id, i => i);
                    equipAction = item => Character4D.SetBody(dict[item.Id], BodyPart.Eyebrows);
                    equippedIndex = SpriteCollection.Eyebrows.FindIndex(i => i.Sprites.Contains(Character.Front.Expressions[0].Eyebrows));
                    break;
                }
                case "Eyes":
                {
                    dict = SpriteCollection.Eyes.ToDictionary(i => i.Id, i => i);
                    equipAction = item => Character4D.SetBody(dict[item.Id], BodyPart.Eyes);
                    equippedIndex = SpriteCollection.Eyes.FindIndex(i => i.Sprites.Contains(Character.Front.Expressions[0].Eyes));
                    break;
                }
                case "Hair":
                {
                    dict = SpriteCollection.Hair.ToDictionary(i => i.Id, i => i);
                    equipAction = item =>Character4D.SetBody(dict[item.Id], BodyPart.Hair);
                    equippedIndex = SpriteCollection.Hair.FindIndex(i => i.Sprites.Contains(Character.Front.Hair));
                    break;
                }
                case "Beard":
                {
                    dict = SpriteCollection.Beard.ToDictionary(i => i.Id, i => i);
                    equipAction = item => Character4D.SetBody(dict[item.Id], BodyPart.Beard);
                    equippedIndex = SpriteCollection.Beard.FindIndex(i => i.Sprites.Contains(Character.Front.Beard));
                    break;
                }
                case "Mouth":
                {
                    dict = SpriteCollection.Mouth.ToDictionary(i => i.Id, i => i);
                    equipAction = item => Character4D.SetBody(dict[item.Id], BodyPart.Mouth);
                    equippedIndex = SpriteCollection.Mouth.FindIndex(i => i.Sprites.Contains(Character.Front.Expressions[0].Mouth));
                    break;
                }
                case "Makeup":
                {
                    dict = SpriteCollection.Makeup.ToDictionary(i => i.Id, i => i);
                    equipAction = item => Character4D.SetBody(dict[item.Id], BodyPart.Makeup);
                    equippedIndex = SpriteCollection.Makeup.FindIndex(i => i.Sprites.Contains(Character.Front.Makeup));
                    break;
                }
                case "Mask":
                {
                    dict = SpriteCollection.Mask.ToDictionary(i => i.Id, i => i);
                    equipAction = item => Character4D.Equip(dict[item.Id], EquipmentPart.Mask);
                    equippedIndex = SpriteCollection.Mask.FindIndex(i => i.Sprites.Contains(Character.Front.Mask));
                    break;
                }
                case "Earrings":
                {
                    dict = SpriteCollection.Earrings.ToDictionary(i => i.Id, i => i);
                    equipAction = item => Character4D.Equip(dict[item.Id], EquipmentPart.Earrings);
                    equippedIndex = Character.Front.Earrings == null ? -1 : SpriteCollection.Earrings.FindIndex(i => i.Sprites.SequenceEqual(Character.Front.Earrings));
                    break;
                }
                case "Supplies":
                {
                    dict = SpriteCollection.Supplies.ToDictionary(i => i.Id, i => i);
                    equipAction = item => { Debug.LogWarning("Supplies are present as icons only and are not displayed on a character. Can be used for inventory."); };
                    equippedIndex = -1;
                    break;
                }
                default:
                    throw new NotImplementedException(tab.name);
            }

            var items = dict.Values.Select(i => new Item(i.Id)).ToList();

            items.Insert(0, new Item("Empty"));
            dict.Add("Empty", null);

            if (Item.GetParams == null)
            {
                Item.GetParams = item => CreateFakeItemParams(item, dict[item.Id]); // We override GetParams method because we don't have a database with item params.
            }

            var collectionId = SpriteCollection.Id;

            IconCollection.Active = IconCollection.Instances.ContainsKey(collectionId) ? IconCollection.Instances[collectionId] : IconCollection.Instances["FantasyHeroes"];
            InventoryItem.OnLeftClick = item =>
            {
                equipAction?.Invoke(item);
                EquipCallback?.Invoke(item);
                ItemName.text = item.Params.SpriteId ?? "Empty";
                SetPaintButton(tab.name, item);
            };
            Inventory.Initialize(ref items, items[equippedIndex + 1], reset: true);

            var equipped = items.Count > equippedIndex + 1 ? items[equippedIndex + 1] : null;

            SetPaintButton(tab.name, equipped);
        }

        private ItemParams CreateFakeItemParams(Item item, SpriteGroupEntry sprite, string replaceable = null, string replacement = null)
        {
            var spriteId = sprite?.Id;

            if (spriteId != null && item.Id != "Empty" && replaceable != null && replacement != null)
            {
                spriteId = spriteId.Replace(replaceable, replacement);
            }

            return new ItemParams { Id = item.Id, SpriteId = spriteId, Meta = sprite == null ? null : Serializer.Serialize(sprite.Tags) };
        }

        private void SetPaintButton(string tab, Item item)
        {
            var tags = item?.Params.MetaToList() ?? new List<string>();

            PaintButton.interactable = PaintParts.Contains(tab) && !tags.Contains("NoPaint") || tags.Contains("Paint");
        }

        /// <summary>
        /// Remove all equipment and reset appearance.
        /// </summary>
        public void Reset()
        {
            Character.Parts.ForEach(i => i.ResetEquipment());
            new CharacterAppearance().Setup(Character.GetComponent<Character4D>());
        }

        /// <summary>
        /// Randomize character.
        /// </summary>
        public void Randomize()
        {
            Character.Randomize();
            OnSelectTab(true);
        }

        /// <summary>
	    /// Save character to json.
	    /// </summary>
	    public void SaveToJson()
	    {
            StartCoroutine(StandaloneFilePicker.SaveFile("Save as JSON", "", "New character", "json", Encoding.Default.GetBytes(Character.ToJson()), (success, path) => { Debug.Log(success ? $"Saved as {path}" : "Error saving."); }));
		}

		/// <summary>
		/// Load character from json.
		/// </summary>
		public void LoadFromJson()
	    {
            StartCoroutine(StandaloneFilePicker.OpenFile("Open as JSON", "", "json", (success, path, bytes) =>
            {
                if (success)
                {
                    var json = System.IO.File.ReadAllText(path);

                    Character.FromJson(json, silent: false);
                }
            }));
	    }

        #if UNITY_EDITOR

        /// <summary>
        /// Save character to prefab.
        /// </summary>
        public void Save()
        {
            var path = UnityEditor.EditorUtility.SaveFilePanel("Save character prefab (should be inside Assets folder)", Path, "New character", "prefab");

            if (path.Length > 0)
            {
                if (!path.Contains("/Assets/")) throw new Exception("Unity can save prefabs only inside Assets folder!");

                Save("Assets" + path.Replace(Application.dataPath, null));
                Path = path;
            }
		}

	    /// <summary>
		/// Load character from prefab.
		/// </summary>
		public void Load()
        {
            var path = UnityEditor.EditorUtility.OpenFilePanel("Load character prefab", Path, "prefab");

            if (path.Length > 0)
            {
                Load("Assets" + path.Replace(Application.dataPath, null));
                Path = path;
            }
		}

	    public override void Save(string path)
		{
			Character.transform.localScale = Vector3.one;

			#if UNITY_2018_3_OR_NEWER

			UnityEditor.PrefabUtility.SaveAsPrefabAsset(Character.gameObject, path);

			#else

			UnityEditor.PrefabUtility.CreatePrefab(path, Character.gameObject);

			#endif

            Debug.LogFormat("Prefab saved as {0}", path);
        }

        public override void Load(string path)
        {
			var character = UnityEditor.AssetDatabase.LoadAssetAtPath<Character4D>(path);

			Load(character);
        }

        #else

        public override void Save(string path)
        {
            throw new System.NotSupportedException();
        }

        public override void Load(string path)
        {
            throw new System.NotSupportedException();
        }

        #endif

        private Color _color;

        public void OpenColorPicker()
        {
            var currentColor = ResolveParts(ActiveTab.name).FirstOrDefault()?.color ?? Color.white;

            ColorPicker.Color = _color = currentColor;
            ColorPicker.OnColorChanged = Paint;
            ColorPicker.SetActive(true);
        }

        public void CloseColorPicker(bool apply)
        {
            if (!apply) Paint(_color);

            ColorPicker.SetActive(false);
        }

        /// <summary>
        /// Pick color and apply to sprite.
        /// </summary>
        public void Paint(Color color)
        {
            foreach (var part in ResolveParts(ActiveTab.name))
            {
                part.color = color;
                part.sharedMaterial = color == Color.white ? DefaultMaterial : ActiveTab.name == "Eyes" ? EyesPaintMaterial : EquipmentPaintMaterial;
            }

            if (ActiveTab.name == "Eyes")
            {
                foreach (var expression in Character.Parts.SelectMany(i => i.Expressions))
                {
                    if (expression.Name != "Dead") expression.EyesColor = color;
                }
            }
        }

        protected override void FeedbackTip()
	    {
			#if UNITY_EDITOR

		    var success = UnityEditor.EditorUtility.DisplayDialog("HeroView Editor", "Hi! Thank you for using my asset! I hope you enjoy making your game with it. The only thing I would ask you to do is to leave a review on the Asset Store. It would be awesome support for my asset, thanks!", "Review", "Later");
			
			RequestFeedbackResult(success);

            #endif
        }
    }
}