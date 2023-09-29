using System;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace CustomScriptTemplates
{
    /// <summary>
    /// Custom Inspector-layout for the "ScriptTemplatesSettings"-class
    /// </summary>
    [CustomEditor(typeof(ScriptTemplatesSettings))]
    internal class ScriptTemplatesSettingsEditor : Editor
    {
        #region Privates
            /// <summary>
            /// Reference to the "ScriptTemplatesSettings"-class
            /// </summary>
            private ScriptTemplatesSettings scriptTemplatesSettings;
            /// <summary>
            /// Format for the opening bracket of the Namespace
            /// </summary>
            private SerializedProperty bracketFormat;
            /// <summary>
            /// How many whitespaces there are between the Namespace name and the opening brace
            /// </summary>
            private SerializedProperty whiteSpacesBetween;
            /// <summary>
            /// How many new lines there are between the Namespace and the Class name
            /// </summary>
            private SerializedProperty newLinesBetween;
            /// <summary>
            /// Padding between the Class and the Namespace
            /// </summary>
            private SerializedProperty indentAmount;
            /// <summary>
            /// Name of the Root Item
            /// </summary>
            private SerializedProperty menuName;
            /// <summary>
            /// Position of the Root MenuItem in the \"create\"-Menu
            /// </summary>
            private SerializedProperty menuItemPosition;
            /// <summary>
            /// List of all templates
            /// </summary>
            private ReorderableList templates;
        #endregion

        #region InfoBox
            /// <summary>
            /// Style-settings for the "GUILayout.Box()"
            /// </summary>
            private GUIStyle infoBoxStyle;
            /// <summary>
            /// InfoText in the "Menu Item"-category
            /// </summary>
            private const string MENU_ITEM_INFO = "\n<size=12><b><color=#ff6a00>IMPORTANT</color></b></size>\n" +
                                                  "<size=12>The MenuItem position won't update, unless the name changes.</size>\n" +
                                                  "<size=12>So if you want to change the position, you also have to change the name and recompile.</size>\n" +
                                                  "<size=12>After that you can change the name back again.</size>\n";
        #endregion
        
        private void OnEnable()
        {
            scriptTemplatesSettings = (ScriptTemplatesSettings)target;
            
            // Fields
            bracketFormat = serializedObject.FindProperty("bracketFormat");
            whiteSpacesBetween = serializedObject.FindProperty("whitespacesBetween");
            newLinesBetween = serializedObject.FindProperty("newLinesBetween");
            indentAmount = serializedObject.FindProperty("indentAmount");
            menuName = serializedObject.FindProperty("menuName");
            menuItemPosition = serializedObject.FindProperty("menuItemPosition");
            
            // List
            templates = new ReorderableList(serializedObject, serializedObject.FindProperty("templates"), true, true, true, true)
            {
                drawHeaderCallback = _Rect => { EditorGUI.LabelField(_Rect, "Templates"); }
            };
            templates.drawElementCallback = (_Rect, _Index, _IsActive, _IsFocused) =>
                {
                    var _element = templates.serializedProperty.GetArrayElementAtIndex(_Index);
                    EditorGUI.PropertyField(new Rect(_Rect.x, _Rect.y + 2.5f, Screen.width - 50, EditorGUIUtility.singleLineHeight), _element, GUIContent.none);
                };
        }
	
        /// <summary>
        /// Initializes values for objects
        /// </summary>
        private void Initialize()
        {
            infoBoxStyle = new GUIStyle(EditorStyles.helpBox) {  richText = true, alignment = TextAnchor.MiddleCenter };
        }
        
        public override void OnInspectorGUI()
        {
            Initialize();
            serializedObject.Update();
            
            // Namespace
            EditorGUILayout.LabelField("Namespace Settings", EditorStyles.boldLabel);
            GUILayout.Label(
                bracketFormat.enumValueIndex == (byte)ScriptTemplatesSettings.BracketFormats.SameLine ? "Opening Bracket will be in the same line as the Namespace" 
                                                                                                      : "Opening Bracket will be one line below the Namespace"
                , infoBoxStyle);
            EditorGUILayout.PropertyField(bracketFormat);
            switch ((ScriptTemplatesSettings.BracketFormats)bracketFormat.enumValueIndex)
            {
                case ScriptTemplatesSettings.BracketFormats.SameLine:
                    break;
                case ScriptTemplatesSettings.BracketFormats.NewLine:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            // Enum "toggle"
            if (bracketFormat.enumValueIndex == (byte)ScriptTemplatesSettings.BracketFormats.SameLine)
            {
                EditorGUILayout.PropertyField(whiteSpacesBetween);
                EditorGUILayout.PropertyField(newLinesBetween);
                EditorGUILayout.PropertyField(indentAmount);   
            }
            GUILayout.Space(EditorGUIUtility.singleLineHeight);

            // MenuItem
            EditorGUILayout.LabelField("Menu Item Settings", EditorStyles.boldLabel);
            GUILayout.Label(MENU_ITEM_INFO, infoBoxStyle);
            EditorGUILayout.PropertyField(menuName);
            EditorGUILayout.PropertyField(menuItemPosition);
            GUILayout.Space(EditorGUIUtility.singleLineHeight);
            
            // List
            EditorGUILayout.LabelField("Templates List", EditorStyles.boldLabel);
            GUILayout.Label("Menu Items will have the same order as this List", infoBoxStyle);
            templates.DoLayoutList();
            
            serializedObject.ApplyModifiedProperties();
            
            // Button
            if (GUILayout.Button("Compile Menu Items"))
            {
                scriptTemplatesSettings.CompileMenuItems();
            }
        }
    }
}