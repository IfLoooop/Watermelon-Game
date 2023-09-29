using UnityEditor;

namespace CustomScriptTemplates
{
	/// <summary>
	/// Contains the Methods for the MenuItems <br/>
	/// DON'T CHANGE ANYTHING IN HERE EXCEPT THE FIELD VALUES!
	/// </summary>
	internal static class ScriptTemplatesMenuItems
	{
		// DONT REFACTOR ANY FIELD (If you want to change their values, do it over the "Settings"-Scriptable Object)
		#region Privates
			/// <summary>
			/// The root MenuItem for all templates
			/// </summary>
			private const string ROOT_ITEM = "Assets/Create/Custom Script Templates/";
			/// <summary>
			/// IF THE ORDER DOESN'T CHANGE IMMEDIATELY IN THE MENU AFTER EDITING THE PRIORITY,              <br/>
			/// CHANGE A CHARACTER INSIDE THE "ROOT_ITEM"-STRING, RECOMPILE AND IT SHOULD BE CHANGED         <br/>
			/// AFTER THAT YOU CAN CHANGE THE "ROOT_ITEM"-STRING BACK AGAIN                                  <br/>
			/// -------------------------------------------------------------------------------------------- <br/>
			/// Priority <  - -1: Above "ScriptableObjects"         ("ScriptableObject" category)            <br/>
			/// Priority  0 -  7: Below "ScriptableObjects"         ("ScriptableObject" category)            <br/>
			/// Priority  8 - 10: Above "Folder"					("Folder" & "ScriptableObject" category) <br/>
			/// Priority 11 - 17: Above "Folder"                    ("Folder" category)                      <br/>
			/// Priority 18 - 28: Below "Folder", above "C# Script" ("Folder" category)						 <br/>
			/// Priority 29 - 70: Below "Folder", above "C# Script" (In its own category)                    <br/>
			/// Priority 71 - 80: Below "Folder", above "C# Script" ("C# Script" category)                   <br/>
			/// Priority 81 -  >: Below "C# Script"					("C# Script" category)
			/// </summary>
			private const int MENU_ITEM_PRIORITY = 80; // Order for the root MenuItem
		#endregion

		// DON'T REMOVE/RENAME/CHANGE ANYTHING IN THIS REGION (INCLUDING THE REGION NAME)!
		#region MENU_ITEMS
            /// <summary>
            /// Creates a new MonoBehaviour.
            /// </summary>
            [MenuItem(ROOT_ITEM + "MonoBehaviour", false, MENU_ITEM_PRIORITY)]
            private static void CreateMonoBehaviour()
            {
                ScriptTemplates.CreateScriptFromTemplate("NewMonoBehaviour.cs", "Assets/Plugins/Custom Script Templates/Settings (Don't import on update!)/Templates/MonoBehaviour.txt");
            }
            /// <summary>
            /// Creates a new Scriptable Object.
            /// </summary>
            [MenuItem(ROOT_ITEM + "Scriptable Object", false, MENU_ITEM_PRIORITY)]
            private static void CreateScriptableObject()
            {
                ScriptTemplates.CreateScriptFromTemplate("NewScriptableObject.cs", "Assets/Plugins/Custom Script Templates/Settings (Don't import on update!)/Templates/Scriptable Object.txt");
            }
            /// <summary>
            /// Creates a new Custom Editor.
            /// </summary>
            [MenuItem(ROOT_ITEM + "Custom Editor", false, MENU_ITEM_PRIORITY)]
            private static void CreateCustomEditor()
            {
                ScriptTemplates.CreateScriptFromTemplate("NewCustomEditor.cs", "Assets/Plugins/Custom Script Templates/Settings (Don't import on update!)/Templates/Custom Editor.txt");
            }
            /// <summary>
            /// Creates a new Interface.
            /// </summary>
            [MenuItem(ROOT_ITEM + "Interface", false, MENU_ITEM_PRIORITY)]
            private static void CreateInterface()
            {
                ScriptTemplates.CreateScriptFromTemplate("NewInterface.cs", "Assets/Plugins/Custom Script Templates/Settings (Don't import on update!)/Templates/Interface.txt");
            }
            /// <summary>
            /// Creates a new Class.
            /// </summary>
            [MenuItem(ROOT_ITEM + "Class", false, MENU_ITEM_PRIORITY)]
            private static void CreateClass()
            {
                ScriptTemplates.CreateScriptFromTemplate("NewClass.cs", "Assets/Plugins/Custom Script Templates/Settings (Don't import on update!)/Templates/Class.txt");
            }
            /// <summary>
            /// Creates a new Struct.
            /// </summary>
            [MenuItem(ROOT_ITEM + "Struct", false, MENU_ITEM_PRIORITY)]
            private static void CreateStruct()
            {
                ScriptTemplates.CreateScriptFromTemplate("NewStruct.cs", "Assets/Plugins/Custom Script Templates/Settings (Don't import on update!)/Templates/Struct.txt");
            }
            /// <summary>
            /// Creates a new Struct.
            /// </summary>
            [MenuItem(ROOT_ITEM + "Enum", false, MENU_ITEM_PRIORITY)]
            private static void CreateEnum()
            {
	            ScriptTemplates.CreateScriptFromTemplate("NewEnum.cs", "Assets/Plugins/Custom Script Templates/Settings (Don't import on update!)/Templates/Enum.txt");
            }
		#endregion
		/// <summary>
		/// Creates a new Text File. <br/>
		/// Outside the Region, because it doesn't need a Template .txt File, <br/>
		/// IF YOU DON'T NEED THIS MENU ENTRY, YOU CAN JUST DELETE THIS METHOD, OTHERWISE DON'T MOVE!
		/// </summary>
		[MenuItem(ROOT_ITEM + "Text File", false, MENU_ITEM_PRIORITY)]
		private static void CreateTextFile()
		{
			ScriptTemplates.CreateTextFileWithoutTemplate("NewTextFile.txt");
		}
	}
}