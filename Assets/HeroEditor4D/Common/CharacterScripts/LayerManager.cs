using System;
using System.Collections.Generic;
using System.Linq;
using HeroEditor4D.Common;
using UnityEngine;
using UnityEngine.Rendering;

namespace Assets.HeroEditor4D.Common.CharacterScripts
{
    /// <summary>
    /// Used to order sprite layers (character parts).
    /// </summary>
    public class LayerManager : MonoBehaviour
    {
        /// <summary>
        /// Full list of sprites
        /// </summary>
        public List<SpriteRenderer> Sprites;

		/// <summary>
		/// SortingGroup can be used when you have multiple characters on scene.
		/// </summary>
		public SortingGroup SortingGroup;

		/// <summary>
		/// Two different characters must have different offsets (0 and 1000 for example).
		/// </summary>
		public int SortingOrderOffset;

        /// <summary>
        /// Step between two sprites (layers).
        /// </summary>
        public int SortingOrderStep = 5;

        /// <summary>
        /// Step between two sprites (layers).
        /// </summary>
        public float ZStep = 0.00001f;

        public LayerManager CopyTo;

        /// <summary>
        /// Set layers order by Sorting Order.
        /// </summary>
        public void SetOrderBySortingOrder()
        {
            for (var i = 0; i < Sprites.Count; i++)
            {
                Sprites[i].sortingOrder = SortingOrderStep * i + SortingOrderOffset;
                SetLocalZ(Sprites[i], 0);
            }

            #if UNITY_EDITOR
            UnityEditor.EditorUtility.SetDirty(this);
            #endif
        }

        /// <summary>
        /// Set layers order by Z coortidate.
        /// </summary>
        public void SetOrderByZCoordinate()
        {
            Debug.LogWarning("Note: you may need to disable hair masks to avoid hair operlapping issues! In current Unity version [2017] masks are applied to all sprites by Sorting Order.");

            for (var j = 0; j < 10; j++) // Workaround for nested structure for setting Z world coordinate.
            for (var i = 0; i < Sprites.Count; i++)
            {
                Sprites[i].sortingOrder = 10;
                SetZ(Sprites[i], -i * ZStep);
            }

            #if UNITY_EDITOR
            UnityEditor.EditorUtility.SetDirty(this);
            #endif
        }

        /// <summary>
        /// Read ordered sprite list by Sorting Order.
        /// </summary>
        public void ReadCurrentOrderBySortingOrder()
        {
            Sprites = GetComponentsInChildren<SpriteRenderer>(true).OrderBy(i => i.sortingOrder).ToList();
        }

        /// <summary>
        /// Read ordered sprite list by Z coortidate.
        /// </summary>
        public void ReadCurrentOrderByZCoortidate()
        {
            Sprites = GetComponentsInChildren<SpriteRenderer>(true).OrderBy(i => -i.transform.position.z).ToList();
        }

	    public void SetSortingGroupOrder(int index)
	    {
		    SortingGroup.sortingOrder = index;
	    }

		private static void SetZ(SpriteRenderer spriteRenderer, float z)
        {
            var p = spriteRenderer.transform.position;

            p.z = z;

            spriteRenderer.transform.position = p;
        }

        private static void SetLocalZ(SpriteRenderer spriteRenderer, float z)
        {
            var p = spriteRenderer.transform.localPosition;

            p.z = 0;

            spriteRenderer.transform.localPosition = p;
        }

        public void CopyOrder()
        {
            if (CopyTo == null) throw new ArgumentNullException(nameof(CopyTo));

            foreach (var sprite in CopyTo.Sprites)
            {
                sprite.sortingOrder = Sprites.Single(i => i.name == sprite.name && GetSpriteRendererPath(i) == GetSpriteRendererPath(sprite)).sortingOrder;
            }

            Debug.Log("Copied!");
        }

        private string GetSpriteRendererPath(SpriteRenderer spriteRenderer)
        {
            var path = spriteRenderer.name;
            var t = spriteRenderer.transform;

            while (t.parent != null && t.parent.GetComponent<Character4D>() == null)
            {
                path = t.parent.name + "/" + path;
                t = t.parent;
            }

            return path;
        }
    }
}