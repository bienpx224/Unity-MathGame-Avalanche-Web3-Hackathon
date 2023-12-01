using Assets.HeroEditor4D.Common.CharacterScripts;
using Assets.HeroEditor4D.Common.EditorScripts;
using UnityEditor;
using UnityEngine;

namespace Assets.HeroEditor4D.Common.Editor
{
    /// <summary>
    /// Add action buttons to LayerManager script
    /// </summary>
    [CustomEditor(typeof(LayerShift))]
    public class LayerShiftEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            var script = (LayerShift) target;

            if (GUILayout.Button("Shift"))
            {
                script.Shift();
            }
        }
    }
}