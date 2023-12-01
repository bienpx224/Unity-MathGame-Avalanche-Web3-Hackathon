using System;
using System.Collections.Generic;
using System.Linq;
using HeroEditor4D.Common;
using HeroEditor4D.Common.Enums;
using UnityEngine;

namespace Assets.HeroEditor4D.Common.CharacterScripts
{
    /// <summary>
    /// Character presentation in editor. Contains sprites, renderers, animation and so on.
    /// </summary>
    public partial class Character : CharacterBase
    {
		//[Header("Weapons")]
		//public MeleeWeapon MeleeWeapon;
		//public BowShooting BowShooting;

	    [Header("Anchors")]
	    public Transform AnchorBody;
	    public Transform AnchorSword;
	    public Transform AnchorBow;
        public Transform AnchorFireMuzzle;

        [Header("Service")]
		public LayerManager LayerManager;

        [Header("Custom")]
        public List<Sprite> Underwear;
        public Color UnderwearColor;
        public bool ShowHelmet = true;

        /// <summary>
        /// Initializes character renderers with selected sprites.
        /// </summary>
        public override void Initialize()
        {
            //try // Disable try/catch for debugging.
            {
                TryInitialize();
            }
            //catch (Exception e)
            {
            //    Debug.LogWarningFormat("Unable to initialize character {0}: {1}", name, e.Message);
            }
        }

		/// <summary>
		/// Initializes character renderers with selected sprites.
		/// </summary>
		private void TryInitialize()
        {
            if (Expressions.All(i => i.Name != "Default") || Expressions.All(i => i.Name != "Angry") || Expressions.All(i => i.Name != "Dead"))
            {
                throw new Exception("Character must have at least 3 basic expressions: Default, Angry and Dead.");
            }

            if (ShowHelmet)
            {
                HairRenderer.sprite = Hair == null ? null : CropHair ? HairCropped : Hair;
                EarsRenderers.ForEach(i => i.enabled = !HideEars);
				HelmetRenderer.sprite = Helmet;
			}
            else
            {
                HairRenderer.sprite = Hair;
                EarsRenderers.ForEach(i => i.enabled = true);
				HelmetRenderer.sprite = null;
			}

            MapSprites(EarsRenderers, Ears);
            SetExpression(Expression);
			if (BeardRenderer != null) BeardRenderer.sprite = Beard;
			MapSprites(BodyRenderers, Body);
            HeadRenderer.sprite = Head;
            MapSprites(ArmorRenderers, Armor);
			//CapeRenderer.sprite = Cape;
			//BackRenderer.sprite = Back;
	        PrimaryWeaponRenderer.sprite = PrimaryWeapon;
            PrimaryWeaponRenderer.enabled = true;
			SecondaryWeaponRenderer.sprite = SecondaryWeapon;
            SecondaryWeaponRenderer.enabled = WeaponType == WeaponType.Paired;
			MapSprites(BowRenderers, CompositeWeapon);
            BowRenderers.ForEach(i => i.enabled = WeaponType == WeaponType.Bow);
			MapSprites(ShieldRenderers, Shield);
            ShieldRenderers.ForEach(i => i.enabled = WeaponType == WeaponType.Melee1H);
			
			if (MakeupRenderer != null) MakeupRenderer.sprite = Makeup;
            if (MaskRenderer != null) MaskRenderer.sprite = Mask;
			MapSprites(EarringsRenderers, Earrings);

			if (WeaponType == WeaponType.Crossbow)
            {
                var quiver = BowRenderers.Single(i => i.name == "Quiver");

                quiver.enabled = true;
				MapSprites(new List<SpriteRenderer> { PrimaryWeaponRenderer, quiver }, CompositeWeapon);
			}

            ApplyMaterials();
		}

		private void MapSprites(List<SpriteRenderer> spriteRenderers, List<Sprite> sprites)
        {
            spriteRenderers.ForEach(i => MapSprite(i, sprites));
        }

        private void MapSprite(SpriteRenderer spriteRenderer, List<Sprite> sprites)
        {
            spriteRenderer.sprite = sprites == null ? null : spriteRenderer.GetComponent<SpriteMapping>().FindSprite(sprites);
        }

        private void ApplyMaterials()
        {
            var renderers = ArmorRenderers.ToList();

            renderers.Add(HairRenderer);
            renderers.Add(PrimaryWeaponRenderer);
            renderers.Add(SecondaryWeaponRenderer);
            renderers.ForEach(i => i.sharedMaterial = i.color == Color.white ? DefaultMaterial : EquipmentPaintMaterial);
        }
    }
}