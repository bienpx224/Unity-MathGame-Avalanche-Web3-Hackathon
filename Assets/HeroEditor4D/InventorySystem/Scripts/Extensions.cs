using System.Collections.Generic;
using System.Linq;
using HeroEditor4D.Common;
using UnityEngine;

namespace Assets.HeroEditor4D.InventorySystem.Scripts
{
    public static class Extensions
    {
        public static Sprite FindSprite(this List<SpriteGroupEntry> list, string id)
        {
            var sprite = list.SingleOrDefault(i => i.Id == id)?.Sprite;

            if (sprite == null) Debug.LogWarning($"Sprite not found: {id}");

            return sprite;
        }

        public static List<Sprite> FindSprites(this List<SpriteGroupEntry> list, string id)
        {
            var sprites = list.SingleOrDefault(i => i.Id == id)?.Sprites.ToList();

            if (sprites == null) Debug.LogWarning($"Sprites not found: {id}");

            return sprites;
        }
    }
}