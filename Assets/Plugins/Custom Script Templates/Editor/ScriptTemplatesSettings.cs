using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

namespace CustomScriptTemplates
{
	/// <summary>
	/// Settings for the Script-Templates
	/// </summary>
	//[CreateAssetMenu(menuName = "CustomScriptTemplates", fileName = "Settings")] // Commented out, so it won't show up in the creation Menu, if you need a new one just uncomment this line
	internal class ScriptTemplatesSettings : ScriptableObject
	{
		/// <summary>
		/// Format for the opening brace of the namespace
		/// </summary>
		internal enum BracketFormats
		{
			/// <summary>
			/// Opening brace starts in the same line as the namespace declaration
			/// </summary>
			SameLine,
			/// <summary>
			/// Opening brace starts one line after the namespace declaration
			/// </summary>
			NewLine
		}

		#region Inspector Fields
			// Namespace settings
			[Tooltip("Format for the opening bracket of the Namespace")]
			[SerializeField] private BracketFormats bracketFormat = BracketFormats.NewLine;
			[Tooltip("How many whitespaces there are between the Namespace name and the opening brace")]
			[SerializeField] private byte whitespacesBetween = 1;
			[Tooltip("How many new lines there are between the Namespace and the Class name")]
			[SerializeField] private byte newLinesBetween = 1;
			[Tooltip("Padding between the Class and the Namespace")]
			[SerializeField] private byte indentAmount = 4;
			// Menu Items settings
			[Tooltip("Name of the Root Item")]
			[SerializeField] private string menuName = "Custom Script Templates";
			[Tooltip("Position of the Root MenuItem in the \"create\"-Menu")]
			[SerializeField] private int menuItemPosition = 80;
			// Template list
			[Tooltip("List of all templates")]
			[SerializeField] private List<TextAsset> templates = new List<TextAsset>();
		#endregion

		#region Privates
			/// <summary>
			/// Singleton of "ScriptTemplatesSettings"
			/// </summary>
			private static ScriptTemplatesSettings instance;
			/// <summary>
			/// The previous name, the Root MenuItem had
			/// </summary>
			private string lastMenuName = "Custom Script Templates";
			/// <summary>
			/// The previous position, the Root MenuItem had
			/// </summary>
			private int lastMenuItemPosition = 80;
			/// <summary>
			/// Is "true" when the "ScriptTemplatesMenuItems"-Class has been edited
			/// </summary>
			private bool waitingForRecompile;
		#endregion
		
		#region Properties
			/// <summary>
			/// Singleton of "ScriptTemplatesSettings"
			/// </summary>
			private static ScriptTemplatesSettings Instance
			{
				get
				{
					if (instance != null) return instance;
					
					try
					{
						var _path = AssetDatabase.GUIDToAssetPath(AssetDatabase.FindAssets(string.Format("t:{0}", typeof(ScriptTemplatesSettings)), new [] { "Assets/Plugins" }).FirstOrDefault());
						return IsNullOrWhiteSpace(_path) ? null : instance = (ScriptTemplatesSettings)AssetDatabase.LoadAssetAtPath(_path, typeof(ScriptTemplatesSettings));
					}
					catch
					{
						throw new Exception(string.Format("<color=#ec1313>Couldn't find the \"<b>{0}</b>\"-Asset!\n" +
																 "Exception: Make sure all Plugin Files are under \"<i>Assets/Plugins</i>\"!</color>", 
																 typeof(ScriptTemplatesSettings).Name));
					}
				}
			}
			/// <summary>
			/// Format for the opening bracket of the Namespace
			/// </summary>
			internal static BracketFormats BracketFormat { get { return Instance.bracketFormat; } }
			/// <summary>
			/// How many whitespaces there are between the Namespace name and the opening brace
			/// </summary>
			internal static byte WhitespacesBetween { get { return Instance.whitespacesBetween; } }
			/// <summary>
			/// How many new lines there are between the Namespace and the Class name
			/// </summary>
			internal static byte NewLinesBetween { get { return Instance.newLinesBetween; } }
			/// <summary>
			/// Padding between the Class and the Namespace
			/// </summary>
			internal static byte IndentAmount { get { return Instance.indentAmount; } }
		#endregion
		
		/// <summary>
		/// Is called when Unity has finished recompiling the Scripts
		/// </summary>
		[UnityEditor.Callbacks.DidReloadScripts]
		private static void OnScriptsRecompiled()
		{
			if (Instance == null || !Instance.waitingForRecompile) return;
			
				Debug.Log("<color=#4abf40><size=20>Finished recompiling!</size></color>\n");
				Instance.waitingForRecompile = false;
		}
		
		private void OnValidate()
		{
			CheckMenuName();
			CheckListEntries();
		}

		/// <summary>
		/// Checks if the MenuName string has not allowed characters in it and removes them
		/// </summary>
		private void CheckMenuName()
		{
			if (menuName == lastMenuName) return;

				if (menuName.Contains('"'))
					menuName = menuName.Replace("\"", string.Empty);
				if (menuName.Contains('\\'))
					menuName = menuName.Replace("\\", string.Empty);
				if (IsNullOrWhiteSpace(menuName))
					menuName = lastMenuName;
		}
		
		/// <summary>
		/// Checks if all List entries are of the right type and removes them if not
		/// </summary>
		private void CheckListEntries()
		{
			for (ushort i = 0; i < templates.Count; i++)
			{
				for (var j = i + 1; j < templates.Count; j++) if (templates[i] == templates[j]) templates[j] = null;
				
				if (templates[i] == null || templates[i].GetType() == typeof(TextAsset)) continue;
				
					Debug.Log(string.Format("<color=#ff6a00>Removing <b>{0}{1}</b>\n {2} Only <b>{3}s</b> allowed in List!</color>", 
						templates[i].name, Path.GetExtension(AssetDatabase.GetAssetPath(templates[i])), DateTime.Now.ToString("HH:mm:ss"), typeof(TextAsset).Name));
					
			}
		}

		/// <summary>
		/// Writes the Methods for the MenuItems to the Class and re-compiles the Script
		/// </summary>
		internal void CompileMenuItems()
		{
			// Find File
			var _scriptTemplatesMenuItemsAsset = AssetDatabase.FindAssets(string.Format("{0}", typeof(ScriptTemplatesMenuItems).Name), new [] { "Assets/Plugins" });
			if (_scriptTemplatesMenuItemsAsset.Length == 0)
			{
				Debug.LogError(string.Concat(string.Format("<color=#ec1313>Couldn't find the \"<b>{0}</b>\"-Script!</color>\n", typeof(ScriptTemplatesMenuItems).Name),
												    string.Format("[{0}] <color=#ec1313>Make sure all Plugin Files are under \"<i>Assets/Plugins</i>\"!</color>", DateTime.Now.ToString("HH:mm:ss"))));
				return;
			}
			var _scriptTemplatesMenuItemsPath = AssetDatabase.GUIDToAssetPath(_scriptTemplatesMenuItemsAsset.First());

			// Read File
			var _fileContent = File.ReadAllLines(_scriptTemplatesMenuItemsPath);
			short _rootItemIndex = -1;
			short _priorityIndex = -1;
			short _regionStart = -1;
			short _regionEnd = -1;
			byte _indentAmount = 0;
			
			// Find Region in File
			for (short i = 0; i < _fileContent.Length; i++)
			{
				if (menuName != lastMenuName && Regex.IsMatch(_fileContent[i], @"(string)\s{1,}(ROOT_ITEM)")) _rootItemIndex = i;
				if (menuItemPosition != lastMenuItemPosition && Regex.IsMatch(_fileContent[i], @"(int)\s{1,}(MENU_ITEM_PRIORITY)")) _priorityIndex = i;
				if (!Regex.IsMatch(_fileContent[i], @"\s{0,}(#region)\s{1,}(MENU_ITEMS)\s{0,}")) continue;

					_indentAmount = (byte)Regex.Match(_fileContent[i], @"\S").Index;
					_regionStart = i;

					for (var j = _regionStart; j < _fileContent.Length; j++)
					{
						if (!_fileContent[j].Contains("#endregion")) continue;
								
							_regionEnd = j;
							break;
					}

					break;
			}
			if (_regionStart == -1)
			{
				Debug.LogError(string.Format(
					"<color=#ec1313>Couldn't find <b>\"#region MENU_ITEMS\"</b>!\n {0}Make sure the MenuItems Methods are inside that Region!</color>", DateTime.Now.ToString("HH:mm:ss")));
				return;
			}
			
			ChangeMenuItem(_fileContent, _rootItemIndex, _priorityIndex);
			
			var _newFileContent = string.Empty;

			// Concatenates everything above the Region
			for (short i = 0; i <= _regionStart; i++)
			{
				_newFileContent = string.Concat(_newFileContent, string.Format("{0}\n", _fileContent[i]));
			}
			// Adds a MethodItem for each Template in the List
			foreach (var _template in templates.Where(_Template => _Template != null))
			{
				_newFileContent = string.Concat(_newFileContent, CreateMenuItemsMethod(_template.name, AssetDatabase.GetAssetPath(_template), _indentAmount));
			}
			// Concatenates everything below the Region
			for (var i = _regionEnd; i < _fileContent.Length; i++)
			{
				_newFileContent = string.Concat(_newFileContent, string.Format("{0}\n", _fileContent[i]));
			}
			
			// Write to File and recompile
			Debug.LogWarning("<color=#ff6a00><size=20>Writing the Code, please wait for Unity to finish recompiling!</size></color>\n");
			waitingForRecompile = true;
			File.WriteAllText(_scriptTemplatesMenuItemsPath, _newFileContent.Trim());
			AssetDatabase.ImportAsset(_scriptTemplatesMenuItemsPath);
		}
		
		/// <summary>
		/// Writes a Method with the "MenuItem"-Attribute to a string
		/// </summary>
		/// <param name="_Name">The name of the Template (will also be the Class/MenuItem name)</param>
		/// <param name="_Path">The Path to the Template .txt File</param>
		/// <param name="_IndentAmount">Padding between the start of the Region and the Border</param>
		/// <returns>Returns the Method code in a string</returns>
		private static string CreateMenuItemsMethod(string _Name, string _Path, byte _IndentAmount)
		{
			var _method = string.Empty;

			_method = string.Concat(_method, string.Format("{0}/// <summary>\n", "".PadLeft((_IndentAmount + 1) * 4)));
			_method = string.Concat(_method, string.Format("{0}/// Creates a new {1}.\n", "".PadLeft((_IndentAmount + 1) * 4), _Name));
			_method = string.Concat(_method, string.Format("{0}/// </summary>\n", "".PadLeft((_IndentAmount + 1) * 4)));
			_method = string.Concat(_method, string.Format("{0}[{1}(ROOT_ITEM + \"{2}\", {3}, MENU_ITEM_PRIORITY)]\n", "".PadLeft((_IndentAmount + 1) * 4), typeof(MenuItem).Name, _Name, false.ToString().ToLower()));
			_method = string.Concat(_method, string.Format("{0}private static {1} Create{2}()\n", "".PadLeft((_IndentAmount + 1) * 4), typeof(void).Name.ToLower(), _Name.Replace(" ", "")));
			_method = string.Concat(_method, string.Concat(string.Format("{0}", "".PadLeft((_IndentAmount + 1) * 4)), "{\n"));
			_method = string.Concat(_method, string.Format("{0}{1}.CreateScriptFromTemplate(\"New{2}.cs\", \"{3}\");\n", "".PadLeft((_IndentAmount + 2) * 4), typeof(ScriptTemplates).Name, _Name.Replace(" ", ""), _Path));
			_method = string.Concat(_method, string.Concat(string.Format("{0}", "".PadLeft((_IndentAmount + 1) * 4)), "}\n"));
			
			return _method;
		}

		/// <summary>
		/// Changes the Name and/or Position of the MenuItem
		/// </summary>
		/// <param name="_FileContent">The content of the File that contains all MenuItems</param>
		/// <param name="_RootItemIndex">Line index in the File, of the MenuItem name</param>
		/// <param name="_PriorityIndex">Line index in the File, of the MenuItem position</param>
		private void ChangeMenuItem(IList<string> _FileContent, short _RootItemIndex, short _PriorityIndex)
		{
			if (_RootItemIndex != -1)
			{
				// Gets the value of the string
				var _firstQuote = _FileContent[_RootItemIndex].IndexOf('"');
				var _secondQuote = _FileContent[_RootItemIndex].IndexOf('"', _firstQuote + 1);
				var _string = _FileContent[_RootItemIndex].Substring(_firstQuote + 1, _secondQuote - _firstQuote - 1);
				_FileContent[_RootItemIndex] = _FileContent[_RootItemIndex].Replace(_string, string.Format("Assets/Create/{0}/", menuName.Trim()));
				lastMenuName = menuName;
			}
			if (_PriorityIndex != -1)
			{
				// Gets the value of the int
				var _equals = _FileContent[_PriorityIndex].IndexOf('=');
				var _semicolon = _FileContent[_PriorityIndex].IndexOf(';', _equals + 1);
				var _int = _FileContent[_PriorityIndex].Substring(_equals + 1, _semicolon - _equals - 1);
				_FileContent[_PriorityIndex] = _FileContent[_PriorityIndex].Replace(_int, string.Format(" {0}", menuItemPosition.ToString()));
				lastMenuItemPosition = menuItemPosition;	
			}
		}
		
		/// <summary>
		/// Returns true if the string is null, empty, or contains only whitespaces
		/// </summary>
		/// <param name="_Value">The string to check</param>
		private static bool IsNullOrWhiteSpace(string _Value)
		{
			return _Value == null || _Value.All(char.IsWhiteSpace);
		}
	}
}