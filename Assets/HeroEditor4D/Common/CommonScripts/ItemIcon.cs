using System;
using UnityEngine;

namespace Assets.HeroEditor4D.Common.CommonScripts
{
	[Serializable]
    public class ItemIcon
    {
        public string Name;
        public string Collection;
        public string Id;
        public string Path;
        public Sprite Sprite;

        public ItemIcon(string edition, string collection, string type, string name, string path, Sprite sprite)
        {
            Id = $"{edition}.{collection}.{type}.{name}";
            Name = name;
            Collection = collection;
            Path = path;
            Sprite = sprite;
        }
    }
}