using System;
using System.IO;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace StarryFramework
{
    internal static class FrameworkPathUtility
    {
        internal const string RootMarkerFileName = "StarryFrameworkRootMarker.txt";
        private const string PathUtilityScriptSuffix = "/Runtime/Framework/Base/FrameworkPathUtility.cs";

#if UNITY_EDITOR
        private static string _cachedPluginRootPath;

        internal static string PluginRootPath
        {
            get
            {
                if (!string.IsNullOrEmpty(_cachedPluginRootPath))
                {
                    return _cachedPluginRootPath;
                }

                string[] guids = AssetDatabase.FindAssets("StarryFrameworkRootMarker t:TextAsset");
                string foundRootPath = null;
                int matchCount = 0;

                foreach (string guid in guids)
                {
                    string markerPath = AssetDatabase.GUIDToAssetPath(guid);
                    string markerName = Path.GetFileName(markerPath);
                    if (!string.Equals(markerName, RootMarkerFileName, StringComparison.Ordinal))
                    {
                        continue;
                    }

                    string rootPath = Path.GetDirectoryName(markerPath)?.Replace("\\", "/");
                    if (string.IsNullOrEmpty(rootPath))
                    {
                        continue;
                    }

                    if (!AssetDatabase.IsValidFolder($"{rootPath}/Runtime") ||
                        !AssetDatabase.IsValidFolder($"{rootPath}/Editor"))
                    {
                        continue;
                    }

                    foundRootPath = rootPath;
                    matchCount++;
                }

                if (matchCount == 1)
                {
                    _cachedPluginRootPath = foundRootPath;
                    return _cachedPluginRootPath;
                }

                if (matchCount > 1)
                {
                    throw new InvalidOperationException($"Found multiple plugin root markers '{RootMarkerFileName}'. Please keep only one.");
                }

                string[] scriptGuids = AssetDatabase.FindAssets("FrameworkPathUtility t:MonoScript");
                foreach (string scriptGuid in scriptGuids)
                {
                    string scriptPath = AssetDatabase.GUIDToAssetPath(scriptGuid);
                    if (!scriptPath.EndsWith(PathUtilityScriptSuffix, StringComparison.Ordinal))
                    {
                        continue;
                    }

                    string rootPath = scriptPath.Substring(0, scriptPath.Length - PathUtilityScriptSuffix.Length);
                    string markerPath = $"{rootPath}/{RootMarkerFileName}";
                    if (string.IsNullOrEmpty(AssetDatabase.AssetPathToGUID(markerPath)))
                    {
                        Debug.LogWarning($"[StarryFramework] Root marker not found at expected path: {markerPath}. Using script-relative root path fallback.");
                    }

                    _cachedPluginRootPath = rootPath;
                    return _cachedPluginRootPath;
                }

                throw new InvalidOperationException("Unable to locate StarryFramework plugin root path.");
            }
        }

        internal static string ResourcesFolderPath => $"{PluginRootPath}/Resources";
        internal static string FrameworkScenePath => $"{PluginRootPath}/Runtime/Scene/GameFramework.unity";
        internal static string InfoFolderPath => $"{PluginRootPath}/Info";
        internal static string ReadmePath => $"{InfoFolderPath}/README.md";
        internal static string ApiReferencePath => $"{InfoFolderPath}/API速查手册.md";
        internal static string LogoPath => $"{InfoFolderPath}/images/StarryFramework-Logo.png";

        internal static void EnsureFolderExists(string assetFolderPath)
        {
            if (AssetDatabase.IsValidFolder(assetFolderPath))
            {
                return;
            }

            string[] parts = assetFolderPath.Split('/');
            if (parts.Length == 0 || parts[0] != "Assets")
            {
                throw new InvalidOperationException($"Invalid asset folder path: {assetFolderPath}");
            }

            string current = "Assets";
            for (int i = 1; i < parts.Length; i++)
            {
                string next = $"{current}/{parts[i]}";
                if (!AssetDatabase.IsValidFolder(next))
                {
                    AssetDatabase.CreateFolder(current, parts[i]);
                }
                current = next;
            }
        }

#endif
    }
}