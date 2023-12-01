using System.Collections.Generic;
using System.Linq;
using HeroEditor4D.Common;
using UnityEngine;

namespace Assets.HeroEditor4D.Common.CharacterScripts
{
    public class AvatarSetup : MonoBehaviour
    {
        public SpriteRenderer Head;
        public SpriteRenderer Hair;
        public List<SpriteRenderer> Ears;
        public SpriteRenderer Eyes;
        public SpriteRenderer Eyebrows;
        public SpriteRenderer Mouth;
        public SpriteRenderer Beard;
        public SpriteRenderer Helmet;

        public void Initialize(CharacterAppearance appearance, string helmetId, string spriteCollectionName)
        {
            var humanoid = spriteCollectionName == "FantasyHeroes" || spriteCollectionName == "MilitaryHeroes" || spriteCollectionName == "UndeadHeroes";
            var collection = SpriteCollection.Instances[spriteCollectionName];
            var ear = collection.Ears.Single(i => i.Name == appearance.Ears).Sprites[1];

            Head.sprite = collection.Body.Single(i => i.Name == appearance.Body).Sprites.Single(i => i.name == "FrontHead");
            Head.color = Ears[0].color = Ears[1].color = appearance.BodyColor;

            if (string.IsNullOrEmpty(appearance.Hair))
            {
                Hair.enabled = false;
            }
            else
            {
                var hair = collection.Hair.Single(i => i.Name == appearance.Hair);

                Hair.enabled = true;
                Hair.sprite = hair.Sprites[1];
                Hair.color = hair.Tags.Contains("NoPaint") ? (Color32) Color.white : appearance.HairColor;
            }

            Beard.sprite = string.IsNullOrEmpty(appearance.Beard) ? null : collection.Beard.Single(i => i.Name == appearance.Beard).Sprite;
            Beard.color = appearance.BeardColor;
            Eyes.sprite = collection.Eyes.Single(i => i.Name == appearance.Eyes).Sprite;
            Eyes.color = appearance.EyesColor;

            if (string.IsNullOrEmpty(appearance.Eyebrows))
            {
                Eyebrows.enabled = false;
            }
            else
            {
                Eyebrows.enabled = true;
                Eyebrows.sprite = collection.Eyebrows.Single(i => i.Name == appearance.Eyebrows).Sprite;
            }

            Mouth.sprite = collection.Mouth.Single(i => i.Name == appearance.Mouth).Sprite;
            Mouth.transform.localPosition = new Vector3(0, humanoid ? -0.1f : 0.25f);

            if (helmetId == null)
            {
                Helmet.enabled = false;
                Ears.ForEach(j => { j.sprite = ear; j.enabled = true; });
            }
            else
            {
                Helmet.enabled = true;

                var entry = collection.Armor.Single(i => i.Id == helmetId.Replace(".Helmet.", ".Armor."));
                var showEars = entry.Tags.Contains("ShowEars");
                var fullHair = entry.Tags.Contains("FullHair");

                Helmet.sprite = entry.Sprites.Single(i => i.name == "FrontHead");
                Ears.ForEach(j => { j.sprite = ear; j.enabled = showEars; });

                if (!fullHair)
                {
                    Hair.sprite = collection.Hair.SingleOrDefault(i => i.Name == "Default")?.Sprites[1];
                    Hair.enabled = Hair.sprite != null;
                }
            }

            Ears[0].transform.localPosition = humanoid ? new Vector3(-1f, 0.5f) : new Vector3(-0.9f, 0.7f);
            Ears[1].transform.localPosition = humanoid ? new Vector3(1f, 0.5f) : new Vector3(0.9f, 0.7f);
        }
    }
}