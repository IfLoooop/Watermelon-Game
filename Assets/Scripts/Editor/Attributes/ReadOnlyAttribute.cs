#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace Watermelon_Game.Editor.Attributes
{
    public class ReadOnlyAttribute : PropertyAttribute
    {

    }

    [CustomPropertyDrawer(typeof(ReadOnlyAttribute))]
    internal class ReadOnlyDrawer : PropertyDrawer
    {
        public override float GetPropertyHeight(SerializedProperty _Property, GUIContent _Label)
        {
            return EditorGUI.GetPropertyHeight(_Property, _Label, true);
        }

        public override void OnGUI(Rect _Position, SerializedProperty _Property, GUIContent _Label)
        {
            GUI.enabled = false;
            EditorGUI.PropertyField(_Position, _Property, _Label, true);
            GUI.enabled = true;
        }
    }
}
#endif