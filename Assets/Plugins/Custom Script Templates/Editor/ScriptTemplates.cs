using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.ProjectWindowCallback;
using UnityEngine;
using Object = UnityEngine.Object;

namespace CustomScriptTemplates
{
	/// <summary>
	/// Class to create a new .cs File from a template .txt File
	/// </summary>
	internal class ScriptTemplates
	{
		#region Privates
			/// <summary>
			/// C# script icon
			/// </summary>
			private static readonly Texture2D SCRIPT_ICON = (Texture2D)EditorGUIUtility.IconContent("cs Script Icon").image;
			/// <summary>
			/// Text File icon
			/// </summary>
			private static readonly Texture2D TEXT_ICON = (Texture2D)EditorGUIUtility.IconContent("TextAsset Icon").image;
		#endregion

		/// <summary>
		/// Creates a new .cs File from a template .txt File
		/// </summary>
		/// <param name="_Name">The name of the File</param>
		/// <param name="_TemplatePath">The path to the template .txt File</param>
		internal static void CreateScriptFromTemplate(string _Name, string _TemplatePath)
		{
			ProjectWindowUtil.StartNameEditingIfProjectWindowExists
				(0, ScriptableObject.CreateInstance<CSharpFile>(), _Name, SCRIPT_ICON, _TemplatePath);
		}
		
		/// <summary>
		/// Creates a new .cs File after the name for the File was chosen
		/// </summary>
		private class CSharpFile : EndNameEditAction
		{
			public override void Action (int _InstanceId, string _PathName, string _ResourceFile)
			{
				var _object = CreateScript(_PathName, _ResourceFile);
				ProjectWindowUtil.ShowCreatedAsset(_object);
			}
		}
		
		/// <summary>
		/// Creates the .cs File
		/// </summary>
		/// <param name="_UnityPath">The path where the File is being created (starting at Unity's root folder)/param>
		/// <param name="_TemplatePath">The path to the template .txt File</param>
		/// <returns>Returns the newly created .cs File as a Unity Object</returns>
		private static Object CreateScript(string _UnityPath, string _TemplatePath)
		{
			// Checks if the template from which the .cs File should be created exists
			if (File.Exists(_TemplatePath))
			{
				var _absolutePath = Path.GetFullPath(_UnityPath);
				var _className = Path.GetFileNameWithoutExtension(_UnityPath).Replace(" ", string.Empty);
				
				File.WriteAllText(_absolutePath, AdjustTemplateContent(File.ReadAllLines(_TemplatePath), _className, SearchNextAssembly(_absolutePath)));
				AssetDatabase.ImportAsset(_UnityPath);
				
				return AssetDatabase.LoadAssetAtPath(_UnityPath, typeof(Object));
			}
			
			Debug.LogError(string.Format("The template file was not found at: {0}", _TemplatePath));
			return null;
		}

		/// <summary>
        /// Searches the next Assembly Definition File or Assembly Reference File from the Folder, where the File is being created
        /// </summary>
        /// <param name="_AbsolutePath">The absolute path of the File, where it is being created</param>
        /// <returns>Returns the Namespaces name if one is set, returns "string.Empty" if no Namespace it set or no Assembly was found</returns>
        private static string SearchNextAssembly(string _AbsolutePath)
        {
			var _namespace = string.Empty;
			
			#if UNITY_2020_2_OR_NEWER
	            // Gets the Folder, the File is in
	            var _folderPath = _AbsolutePath.Remove(_AbsolutePath.LastIndexOf('\\'));

				do
				{
	                // Searches for .asmdef or .asmref Files in the Folder
	                var _assemblies = Directory.EnumerateFiles(_folderPath, "*.asm*", SearchOption.TopDirectoryOnly).ToList();

	                if (_assemblies.Count > 0)
	                {
	                    // Absolute path to the Projects root Folder
	                    var _remove = Application.dataPath.Remove(Application.dataPath.LastIndexOf('/') + 1).Replace('/', '\\');
	                    // Path to the Assembly from the Projects root Folder
	                    var _path = _assemblies.First().Replace(_remove, "").Replace('\\', '/');
	                    var _type = AssetDatabase.GetMainAssetTypeAtPath(_path);
	                    
	                    if (_type == typeof(UnityEditorInternal.AssemblyDefinitionAsset))
	                    {
	                        var _assembly = (UnityEditorInternal.AssemblyDefinitionAsset)AssetDatabase.LoadAssetAtPath(_path, typeof(UnityEditorInternal.AssemblyDefinitionAsset));
	                        _namespace = GetRootNamespace(_assembly.text);
	                    }
	                    else if (_type == typeof(UnityEditorInternal.AssemblyDefinitionReferenceAsset))
	                    {
		                    var _assembly = (UnityEditorInternal.AssemblyDefinitionReferenceAsset)AssetDatabase.LoadAssetAtPath(_path, typeof(UnityEditorInternal.AssemblyDefinitionReferenceAsset));
		                    _namespace = GetAssemblyFromReference(_assembly.text);
	                    }
	                    
	                    break;
	                }

	                // If no Assembly File was found, go one Folder up 
	                _folderPath = _folderPath.Remove(_folderPath.LastIndexOf('\\'));
	                
	            // Do this as long as the Folder to search in, is inside the Project
	            } while (_folderPath != Application.dataPath.Remove(Application.dataPath.LastIndexOf('/')).Replace('/', '\\'));
			#endif

            return _namespace;
        }

        /// <summary>
        /// Gets the Assembly Definition File from an Assembly Reference File
        /// </summary>
        /// <param name="_AssemblyText">The Text content of the Assembly File</param>
        /// <returns>Returns the Namespaces Name from the Assembly as a string, returns "string.Empty" if none is set</returns>
        private static string GetAssemblyFromReference(string _AssemblyText)
        {
            const string _REFERENCE = "\"reference\":";
            const string _GUID = "GUID:";

            if (!_AssemblyText.Contains(_REFERENCE)) return string.Empty;
            
                // Gets the reference to the Assembly File between the first and second quote, after ""reference":"
                var _firstQuote = _AssemblyText.IndexOf('"', _AssemblyText.IndexOf(_REFERENCE, StringComparison.Ordinal) + _REFERENCE.Length);
                var _secondQuote = _AssemblyText.IndexOf('"', _firstQuote + 1);
                var _reference = _AssemblyText.Substring(_firstQuote + 1, _secondQuote - _firstQuote - 1);
                var _assemblyText = string.Empty;

                // Returns if the reference isn't set in the Inspector (null)
                if (_reference.Contains(_GUID) && IsNullOrWhiteSpace(_reference.Replace(_GUID, string.Empty)))
                    return string.Empty;

				#if UNITY_2020_2_OR_NEWER
	                // Searches the Assembly File by its GUID
	                if (_reference.Contains(_GUID))
	                {
	                    _reference = _reference.Replace(_GUID, string.Empty);
	                    _assemblyText = ((UnityEditorInternal.AssemblyDefinitionAsset)AssetDatabase.LoadAssetAtPath(AssetDatabase.GUIDToAssetPath(_reference), typeof(UnityEditorInternal.AssemblyDefinitionAsset))).text;
	                }
	                // Searches the Assembly File by its name
	                else
	                {
	                    var _guid = AssetDatabase.FindAssets($"t:{nameof(UnityEditorInternal.AssemblyDefinitionAsset)} {_reference}");
	                    _assemblyText = ((UnityEditorInternal.AssemblyDefinitionAsset)AssetDatabase.LoadAssetAtPath(AssetDatabase.GUIDToAssetPath(_guid.First()), typeof(UnityEditorInternal.AssemblyDefinitionAsset))).text;
	                }
				#endif

                return GetRootNamespace(_assemblyText);
        }

        /// <summary>
        /// Gets the Root Namespace of the Assembly <br/>
        /// If none is set, returns "string.Empty"
        /// </summary>
        /// <param name="_AssemblyText">The Text content of the Assembly File</param>
        /// <returns>Returns the Namespaces Name from the Assembly as a string, returns "string.Empty" if none is set</returns>
        private static string GetRootNamespace(string _AssemblyText)
        {
            const string _ROOT_NAMESPACE = "\"rootNamespace\":";

            if (!_AssemblyText.Contains(_ROOT_NAMESPACE)) return string.Empty;
            
                // Gets the name of the Namespace between the first and second quotes, after ""rootNamespace":"
                var _firstQuote = _AssemblyText.IndexOf('"', _AssemblyText.IndexOf(_ROOT_NAMESPACE, StringComparison.Ordinal) + _ROOT_NAMESPACE.Length);
                var _secondQuote = _AssemblyText.IndexOf('"', _firstQuote + 1);
                var _namespace = _AssemblyText.Substring(_firstQuote + 1, _secondQuote - _firstQuote - 1);

                return IsNullOrWhiteSpace(_namespace) ? string.Empty : _namespace.Trim();
        }
		
		/// <summary>
		/// Changes the placeholder names of the template and includes hte namespace if one is set <br/>
		/// You can change this Method if you want to create your own, just make sure the arguments and return value stay the same!
		/// </summary>
		/// <param name="_TemplateContent">The content of the template .txt File</param>
		/// <param name="_ClassName">Same as the Filename (without the extension)</param>
		/// <param name="_AssemblyNamespace">Namespace of the next Assembly Definition File, "string.Empty", when no File was found</param>
		/// <returns>Returns the content for the .cs File as a string</returns>
		private static string AdjustTemplateContent(string[] _TemplateContent, string _ClassName, string _AssemblyNamespace)
		{
			const string _USING = "using ";
			const string _NAMESPACE = "#NAMESPACE#";
			const string _SCRIPT_NAME = "#SCRIPTNAME#";
			
			var _usingStatements = string.Empty;
			var _code = string.Empty;
			var _padding = string.Empty;
			var _includeNamespace = false;

			foreach (var _line in _TemplateContent)
			{
				// Using-statements
				if (_line.ToLower().Contains(_USING))
				{
					_usingStatements = string.Concat(_usingStatements, string.Format("{0}\n", _line));
				}
				// Rest
				else
				{
					// With namespace
					if (_line.Contains(_NAMESPACE))
					{
						var _thirdHash = _line.IndexOf('#', _line.IndexOf(_NAMESPACE, StringComparison.Ordinal) + _NAMESPACE.Length);
						var _namespace = string.Empty;
						
						// When there's another #-symbol after "#NAMESPACE#" (for custom Namespace)
						if (_thirdHash != -1)
						{
							// Index of first character after "#NAMESPACE#"
							var _startIndex = _line.IndexOf(_NAMESPACE, StringComparison.Ordinal) + _NAMESPACE.Length;
							// Character count between the 2nd and 3rd #-symbols
							var _length = _thirdHash - _startIndex;
							_namespace = _line.Substring(_startIndex, _length).Replace(" ", "");
						}
						// When there's no custom Namespace set in the Template
						if (_namespace == string.Empty || IsNullOrWhiteSpace(_namespace))
						{
							// Checks if there is an Assembly Definition File with a set Namespace 
							if (!IsNullOrWhiteSpace(_AssemblyNamespace))
							{
								_namespace = _AssemblyNamespace;
							}
							// Uses the Projects Root Namespace if it is set
							else
							{
								if (IsNullOrWhiteSpace(EditorSettings.projectGenerationRootNamespace))
									goto SKIP_NAMESPACE;
								
								_namespace = EditorSettings.projectGenerationRootNamespace;
							}
						}

						_includeNamespace = true;
						_code = string.Concat(_code, _line.Replace(_line, string.Format("namespace {0}\n", _namespace.Replace(" ", string.Empty))).Trim());
						switch (ScriptTemplatesSettings.BracketFormat)
						{
							case ScriptTemplatesSettings.BracketFormats.SameLine:
								_code = string.Concat(_code,																 // Concatenates the namespace to the class
										string.Concat(_padding.PadLeft(ScriptTemplatesSettings.WhitespacesBetween),			 // Adds whitespaces
										string.Concat("{\n", new string('\n', ScriptTemplatesSettings.NewLinesBetween)))); // Adds new lines
								break;
							case ScriptTemplatesSettings.BracketFormats.NewLine:
								_code = string.Concat(_code, "\n{\n");
								break;
							default:
								throw new ArgumentOutOfRangeException();
						}
						SKIP_NAMESPACE:;
					}
					// Without namespace
					else
					{
						_code = string.Concat(_code, _includeNamespace ? string.Format("{0}{1}\n", _padding.PadLeft(ScriptTemplatesSettings.IndentAmount), _line) 
																	   : string.Format("{0}\n", _line));
					}
				}
			}

			// Changes the placeholder Class-name to the Filename
			_code = _code.Replace(_SCRIPT_NAME, _ClassName);
			
			// Adds a closing bracket for the namespace to the string, if a namespace was included in the template
			return string.Concat(_usingStatements, _includeNamespace ? string.Concat(_code, "}") : _code).Trim();
		}
		
		/// <summary>
		/// Creates a new .txt File
		/// </summary>
		/// <param name="_Name">The name of the File</param>
		internal static void CreateTextFileWithoutTemplate(string _Name)
		{
			ProjectWindowUtil.StartNameEditingIfProjectWindowExists
				(0, ScriptableObject.CreateInstance<TextFile>(), _Name, TEXT_ICON, string.Empty);
		}
		
		/// <summary>
		/// Creates a new .txt File after the name for the File was chosen
		/// </summary>
		private class TextFile : EndNameEditAction
		{
			public override void Action (int _InstanceId, string _PathName, string _ResourceFile)
			{
				var _object = CreateTextFile(_PathName);
				ProjectWindowUtil.ShowCreatedAsset(_object);
			}
		}
		
		/// <summary>
		/// Creates the .txt File
		/// </summary>
		/// <param name="_UnityPath">The path where the File is being created (starting at Unity's root folder)/param>
		/// <returns>Returns the newly created .txt File as a TextAsset</returns>
		private static Object CreateTextFile(string _UnityPath)
		{
			// Needs to be written in once, otherwise it will be deleted
			File.WriteAllText(Path.GetFullPath(_UnityPath), string.Empty);
			AssetDatabase.ImportAsset(_UnityPath);
				
			return AssetDatabase.LoadAssetAtPath(_UnityPath, typeof(TextAsset));
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