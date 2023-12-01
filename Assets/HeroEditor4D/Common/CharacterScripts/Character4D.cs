using System;
using System.Collections.Generic;
using System.Linq;
using Assets.HeroEditor4D.Common.CommonScripts;
using Assets.HeroEditor4D.InventorySystem.Scripts.Data;
using HeroEditor4D.Common;
using HeroEditor4D.Common.Enums;
using UnityEngine;

namespace Assets.HeroEditor4D.Common.CharacterScripts
{
	/// <summary>
	/// Controls 4 characters (for each direction).
	/// </summary>
	public class Character4D : Character4DBase
    {
		public SpriteCollection SpriteCollection => Parts[0].SpriteCollection;
        public AnimationManager AnimationManager;
        public LayerManager LayerManager;
        public Transform FireMuzzle;

        public void OnValidate()
		{
			Parts = new List<CharacterBase> { Front, Back, Left, Right };
			Parts.ForEach(i => i.BodyRenderers.ForEach(j => j.color = BodyColor));
			Parts.ForEach(i => i.EarsRenderers.ForEach(j => j.color = BodyColor));
		}

        public void Start()
        {
            var stateHandler = Animator.GetBehaviours<StateHandler>().SingleOrDefault(i => i.Name == "Death");

            if (stateHandler)
            {
                stateHandler.StateExit.AddListener(() => SetExpression("Default"));
            }

            Animator.keepAnimatorStateOnDisable = true;
        }

		public override void Initialize()
		{
			Parts.ForEach(i => i.Initialize());
		}

		public override void CopyFrom(Character4DBase character)
		{
			for (var i = 0; i < Parts.Count; i++)
			{
				Parts[i].CopyFrom(character.Parts[i]);
			}
		}

		public override string ToJson()
		{
		    return Front.ToJson();
		}

		public override void FromJson(string json, bool silent)
		{
		    Parts.ForEach(i => i.LoadFromJson(json, silent));
		}

        public Vector2 Direction { get; private set; }

		public void SetDirection(Vector2 direction)
		{
            if (Direction == direction) return;

			Direction = direction;

            if (Direction == Vector2.zero)
            {
                Parts.ForEach(i => i.SetActive(true));
                Shadows.ForEach(i => i.SetActive(true));

                Parts[0].transform.localPosition = Shadows[0].transform.localPosition = new Vector3(0, -1.25f);
                Parts[1].transform.localPosition = Shadows[1].transform.localPosition = new Vector3(0, 1.25f);
                Parts[2].transform.localPosition = Shadows[2].transform.localPosition = new Vector3(-1.5f, 0);
                Parts[3].transform.localPosition = Shadows[3].transform.localPosition = new Vector3(1.5f, 0);

                return;
            }

			Parts.ForEach(i => i.transform.localPosition = Vector3.zero);
			Shadows.ForEach(i => i.transform.localPosition = Vector3.zero);

			int index;

			if (direction == Vector2.left)
			{
				index = 2;
			}
			else if (direction == Vector2.right)
			{
				index = 3;
			}
			else if (direction == Vector2.up)
			{
				index = 1;
			}
			else if (direction == Vector2.down)
			{
				index = 0;
			}
            else
			{
				throw new NotSupportedException();
			}

			for (var i = 0; i < Parts.Count; i++)
			{
                Parts[i].SetActive(i == index);
				Shadows[i].SetActive(i == index);
			}
		}

        #region Setup Examples

        public void EquipArmor(Item item)
        {
            if (item == null) UnEquip(EquipmentPart.Armor);
            else Equip(SpriteCollection.Armor.Single(i => i.Id == item.Params.SpriteId), EquipmentPart.Armor);
        }

        public void EquipHelmet(Item item)
        {
            if (item == null) UnEquip(EquipmentPart.Helmet);
            else Equip(SpriteCollection.Armor.Single(i => i.Id == item.Params.SpriteId.Replace(".Helmet.", ".Armor.")), EquipmentPart.Helmet);
        }

        public void EquipVest(Item item)
        {
            if (item == null) UnEquip(EquipmentPart.Vest);
            else Equip(SpriteCollection.Armor.Single(i => i.Id == item.Params.SpriteId.Replace(".Vest.", ".Armor.")), EquipmentPart.Vest);
        }

        public void EquipBracers(Item item)
        {
            if (item == null) UnEquip(EquipmentPart.Bracers);
            else Equip(SpriteCollection.Armor.Single(i => i.Id == item.Params.SpriteId.Replace(".Bracers.", ".Armor.")), EquipmentPart.Bracers);
        }

        public void EquipLeggings(Item item)
        {
            if (item == null) UnEquip(EquipmentPart.Leggings);
            else Equip(SpriteCollection.Armor.Single(i => i.Id == item.Params.SpriteId.Replace(".Leggings.", ".Armor.")), EquipmentPart.Leggings);
        }

        public void EquipShield(Item item)
        {
            Equip(SpriteCollection.Shield.SingleOrDefault(i => i.Id == item.Id), EquipmentPart.Shield);
        }
        
        public void EquipMeleeWeapon1H(Item item)
        {
            Equip(SpriteCollection.MeleeWeapon1H.SingleOrDefault(i => i.Id == item.Id), EquipmentPart.MeleeWeapon1H);
        }

        public void EquipMeleeWeapon2H(Item item)
        {
            Equip(SpriteCollection.MeleeWeapon2H.SingleOrDefault(i => i.Id == item.Id), EquipmentPart.MeleeWeapon2H);
        }

        public void EquipBow(Item item)
        {
            Equip(SpriteCollection.Bow.SingleOrDefault(i => i.Id == item.Id), EquipmentPart.Bow);
        }

        public void EquipCrossbow(Item item)
        {
            Equip(SpriteCollection.Crossbow.SingleOrDefault(i => i.Id == item.Id), EquipmentPart.Crossbow);
        }

        public void EquipSecondaryFirearm(Item item)
        {
            Equip(SpriteCollection.Firearm1H.SingleOrDefault(i => i.Id == item.Id), EquipmentPart.SecondaryFirearm1H);
        }

        #endregion
    }
}