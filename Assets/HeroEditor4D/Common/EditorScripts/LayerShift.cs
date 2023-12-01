using UnityEngine;

namespace Assets.HeroEditor4D.Common.EditorScripts
{
    public class LayerShift : MonoBehaviour
    {
        public int Offset;

        public void Shift()
        {
            foreach (var spriteRenderer in GetComponentsInChildren<SpriteRenderer>())
            {
                spriteRenderer.sortingOrder += Offset;
            }
        }
    }
}