using System.Linq;
using UnityEditor;

namespace CustomScriptTemplates
{
    /// <summary>
    /// Compiles the MenuItems after the Package has been imported
    /// </summary>
    [InitializeOnLoad]
    internal class PackageImporter
    {
        static PackageImporter()
        {
            AssetDatabase.importPackageCompleted += OnImportPackageCompleted;
        }

        /// <summary>
        /// Is called after the Package has successfully been imported 
        /// </summary>
        /// <param name="_PackageName">The name of the imported Package</param>
        private static void OnImportPackageCompleted(string _PackageName)
        {
            if (_PackageName != "Custom Script Templates") return;
            
                var _scriptTemplatesMenuItemsAsset = AssetDatabase.FindAssets(string.Format("t:{0}", typeof(ScriptTemplatesSettings)), new [] { "Assets/Plugins" });
                var _scriptTemplatesMenuItemsPath = AssetDatabase.GUIDToAssetPath(_scriptTemplatesMenuItemsAsset.First());
                var _scriptTemplatesSettings = (ScriptTemplatesSettings)AssetDatabase.LoadAssetAtPath(_scriptTemplatesMenuItemsPath, typeof(ScriptTemplatesSettings));
                _scriptTemplatesSettings.CompileMenuItems();
        }
    }
}