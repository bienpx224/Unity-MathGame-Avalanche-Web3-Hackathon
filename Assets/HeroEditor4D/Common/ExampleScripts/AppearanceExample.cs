using System.Linq;
using Assets.HeroEditor4D.Common.CharacterScripts;
using HeroEditor4D.Common.Enums;
using UnityEngine;

namespace Assets.HeroEditor4D.Common.ExampleScripts
{
    /// <summary>
    /// An example of how to change character's appearance.
    /// </summary>
    public class AppearanceExample : MonoBehaviour
    {
        public CharacterAppearance Appearance = new CharacterAppearance();
        public Character4D Character;
        public AvatarSetup AvatarSetup;

        public void Start()
        {
            Refresh();
        }

        public void Refresh()
        {
            Appearance.Setup(Character);

            var helmetId = Character.SpriteCollection.Armor.SingleOrDefault(i => i.Sprites.Contains(Character.Front.Helmet))?.Id;

            AvatarSetup.Initialize(Appearance, helmetId, Character.SpriteCollection.Id);
        }

        public void SetRandomAppearance()
        {
            // Way 1: use Appearance class

            Appearance.Hair = Random.Range(0, 3) == 0 ? null : Character.SpriteCollection.Hair[Random.Range(0, Character.SpriteCollection.Hair.Count)].Name;
            Appearance.HairColor = new Color(Random.Range(0, 1f), Random.Range(0, 1f), Random.Range(0, 1f));
            Appearance.Eyebrows = Character.SpriteCollection.Eyebrows[Random.Range(0, Character.SpriteCollection.Eyebrows.Count)].Name;
            Appearance.Eyes = Character.SpriteCollection.Eyes[Random.Range(0, Character.SpriteCollection.Eyes.Count)].Name;
            Appearance.EyesColor = new Color(Random.Range(0, 1f), Random.Range(0, 1f), Random.Range(0, 1f));
            Appearance.Mouth = Character.SpriteCollection.Mouth[Random.Range(0, Character.SpriteCollection.Mouth.Count)].Name;
            Appearance.Beard = Random.Range(0, 3) == 0 ? Character.SpriteCollection.Beard[Random.Range(0, Character.SpriteCollection.Beard.Count)].Name : null;
            
            Refresh();

            // Way 2: use Character.SetBody

            //SetRandomHair();
            //SetRandomEyebrows();
            //SetRandomEyes();
            //SetRandomMouth();
        }

        public void ResetAppearance()
        {
            Appearance = new CharacterAppearance();
            Refresh();
        }

        public void SetRandomHair()
        {
            var randomIndex = Random.Range(0, Character.SpriteCollection.Hair.Count);
            var randomItem = Character.SpriteCollection.Hair[randomIndex];
            var randomColor = new Color(Random.Range(0, 1f), Random.Range(0, 1f), Random.Range(0, 1f));

            Character.SetBody(randomItem, BodyPart.Hair, randomColor);
        }

        public void SetRandomEyebrows()
        {
            var randomIndex = Random.Range(0, Character.SpriteCollection.Eyebrows.Count);
            var randomItem = Character.SpriteCollection.Eyebrows[randomIndex];

            Character.SetBody(randomItem, BodyPart.Eyebrows);
        }

        public void SetRandomEyes()
        {
            var randomIndex = Random.Range(0, Character.SpriteCollection.Eyes.Count);
            var randomItem = Character.SpriteCollection.Eyes[randomIndex];
            var randomColor = new Color(Random.Range(0, 1f), Random.Range(0, 1f), Random.Range(0, 1f));

            Character.SetBody(randomItem, BodyPart.Eyes, randomColor);
        }

        public void SetRandomMouth()
        {
            var randomIndex = Random.Range(0, Character.SpriteCollection.Mouth.Count);
            var randomItem = Character.SpriteCollection.Mouth[randomIndex];

            Character.SetBody(randomItem, BodyPart.Mouth);
        }
    }
}