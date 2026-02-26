using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace StarryFramework.Editor
{
    [CustomEditor(typeof(ResourceComponent))]
    public class ResourceComponentInspector : FrameworkInspector
    {
        private bool showResourcesAssets = true;
        private bool showAddressablesAssets = true;
        private string searchFilter = "";
        private ResourceSortMode sortMode = ResourceSortMode.LoadTime;
        private bool sortDescending = true;

        private enum ResourceSortMode
        {
            Name,
            RefCount,
            LoadTime,
            MemorySize,
            Type
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            if (!EditorApplication.isPlaying)
            {
                EditorGUILayout.HelpBox("Available only in Play Mode.", MessageType.Info);
                return;
            }

            ResourceComponent r = (ResourceComponent)target;

            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("Current Load Operation", EditorStyles.boldLabel);
            EditorGUI.indentLevel++;
            
            EditorGUILayout.LabelField("Target Resource Type", r.TargetType == null ? "Null" : r.TargetType.ToString());
            EditorGUILayout.LabelField("Target Resource Path", r.ResourcePath);
            
            GUIStyle stateStyle = new GUIStyle(EditorStyles.label);
            if (r.State == LoadState.Failed)
            {
                stateStyle.normal.textColor = Color.red;
            }
            else if (r.State == LoadState.Loading)
            {
                stateStyle.normal.textColor = Color.yellow;
            }
            else if (r.State == LoadState.Completed)
            {
                stateStyle.normal.textColor = Color.green;
            }
            
            EditorGUILayout.LabelField("Load State", r.State.ToString(), stateStyle);
            
            if (r.State == LoadState.Failed && !string.IsNullOrEmpty(r.LastError))
            {
                EditorGUILayout.HelpBox(r.LastError, MessageType.Error);
            }
            
            EditorGUILayout.LabelField("Load Progress", r.Progress.ToString("F2"));
            var rect = GUILayoutUtility.GetRect(18, 18, "TextField");
            
            Color progressColor = r.State == LoadState.Failed ? Color.red : 
                                  r.State == LoadState.Completed ? Color.green : 
                                  Color.cyan;
            
            Rect colorRect = new Rect(rect.x, rect.y, rect.width * r.Progress, rect.height);
            EditorGUI.DrawRect(colorRect, progressColor * 0.5f);
            EditorGUI.ProgressBar(rect, r.Progress, $"{r.Progress * 100:F2}%");
            
            EditorGUI.indentLevel--;

            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("Active Async Operations", EditorStyles.boldLabel);
            EditorGUI.indentLevel++;
            
            int activeOpCount = r.GetActiveOperationCount();
            EditorGUILayout.LabelField("Active Operation Count", activeOpCount.ToString());
            
            if (activeOpCount > 0)
            {
                EditorGUILayout.Space(5);
                var activeOps = r.GetAllActiveOperations();
                
                foreach (var kvp in activeOps)
                {
                    EditorGUILayout.BeginVertical("box");
                    
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField("Addr:", GUILayout.Width(40));
                    EditorGUILayout.LabelField(kvp.Value.Address);
                    EditorGUILayout.EndHorizontal();
                    
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField("Type:", GUILayout.Width(40));
                    EditorGUILayout.LabelField(kvp.Value.AssetType?.Name ?? "Unknown");
                    EditorGUILayout.EndHorizontal();
                    
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField("State:", GUILayout.Width(40));
                    GUIStyle opStateStyle = new GUIStyle(EditorStyles.label);
                    if (kvp.Value.State == LoadState.Failed)
                        opStateStyle.normal.textColor = Color.red;
                    else if (kvp.Value.State == LoadState.Loading)
                        opStateStyle.normal.textColor = Color.yellow;
                    EditorGUILayout.LabelField(kvp.Value.State.ToString(), opStateStyle);
                    EditorGUILayout.EndHorizontal();
                    
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField("Prog:", GUILayout.Width(40));
                    var progressRect = GUILayoutUtility.GetRect(200, 18);
                    EditorGUI.ProgressBar(progressRect, kvp.Value.Progress, $"{kvp.Value.Progress * 100:F1}%");
                    EditorGUILayout.EndHorizontal();
                    
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField("Time:", GUILayout.Width(40));
                    EditorGUILayout.LabelField($"{kvp.Value.ElapsedTime:F2}s");
                    EditorGUILayout.EndHorizontal();
                    
                    if (!string.IsNullOrEmpty(kvp.Value.ErrorMessage))
                    {
                        EditorGUILayout.HelpBox(kvp.Value.ErrorMessage, MessageType.Error);
                    }
                    
                    EditorGUILayout.EndVertical();
                    EditorGUILayout.Space(3);
                }
            }
            
            EditorGUI.indentLevel--;

            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("Resource Cache Stats", EditorStyles.boldLabel);
            EditorGUI.indentLevel++;
            
            int loadedCount = r.GetLoadedAssetCount();
            long totalMemory = r.GetTotalMemorySize();
            
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Loaded Resource Count", loadedCount.ToString(), GUILayout.Width(200));
            EditorGUILayout.LabelField($"Total Memory: {FormatBytes(totalMemory)}", EditorStyles.boldLabel);
            EditorGUILayout.EndHorizontal();
            
            var resourcesCount = r.GetResourcesByType(ResourceSourceType.Resources).Count;
            var addressablesCount = r.GetResourcesByType(ResourceSourceType.Addressables).Count;
            
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField($"Resources Assets: {resourcesCount}", GUILayout.Width(200));
            EditorGUILayout.LabelField($"Addressables Assets: {addressablesCount}");
            EditorGUILayout.EndHorizontal();
            
            if (loadedCount > 0)
            {
                EditorGUILayout.Space(10);
                
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Filters", EditorStyles.boldLabel);
                EditorGUILayout.EndHorizontal();
                
                EditorGUILayout.BeginHorizontal();
                showResourcesAssets = EditorGUILayout.ToggleLeft("Resources", showResourcesAssets, GUILayout.Width(100));
                showAddressablesAssets = EditorGUILayout.ToggleLeft("Addressables", showAddressablesAssets, GUILayout.Width(120));
                EditorGUILayout.EndHorizontal();
                
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Search:", GUILayout.Width(50));
                searchFilter = EditorGUILayout.TextField(searchFilter);
                if (GUILayout.Button("Clear", GUILayout.Width(50)))
                {
                    searchFilter = "";
                }
                EditorGUILayout.EndHorizontal();
                
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Sort:", GUILayout.Width(50));
                sortMode = (ResourceSortMode)EditorGUILayout.EnumPopup(sortMode, GUILayout.Width(100));
                sortDescending = EditorGUILayout.ToggleLeft("Desc", sortDescending, GUILayout.Width(60));
                EditorGUILayout.EndHorizontal();
                
                EditorGUILayout.Space(5);
                
                var loadedAssets = r.GetAllLoadedAssets();
                var filteredAssets = FilterAndSortAssets(loadedAssets);
                
                EditorGUILayout.LabelField($"Visible Assets: {filteredAssets.Count}/{loadedCount}", EditorStyles.miniLabel);
                EditorGUILayout.Space(3);
                
                foreach (var kvp in filteredAssets)
                {
                    EditorGUILayout.BeginVertical("box");
                    
                    EditorGUILayout.BeginHorizontal();
                    
                    Color sourceColor = kvp.Value.SourceType == ResourceSourceType.Resources ? 
                        new Color(0.5f, 0.8f, 1f) : new Color(1f, 0.8f, 0.5f);
                    
                    GUIStyle sourceStyle = new GUIStyle(EditorStyles.miniLabel);
                    sourceStyle.normal.textColor = sourceColor;
                    sourceStyle.fontStyle = FontStyle.Bold;
                    
                    EditorGUILayout.LabelField(kvp.Value.SourceType.ToString(), sourceStyle, GUILayout.Width(100));
                    EditorGUILayout.LabelField(kvp.Value.AssetType?.Name ?? "Unknown", EditorStyles.miniLabel, GUILayout.Width(100));
                    
                    EditorGUILayout.EndHorizontal();
                    
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField(kvp.Key, GUILayout.MinWidth(250));
                    EditorGUILayout.EndHorizontal();
                    
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField($"Refs: {kvp.Value.RefCount}", GUILayout.Width(70));
                    EditorGUILayout.LabelField($"Time: {kvp.Value.LoadTime:HH:mm:ss}", GUILayout.Width(100));
                    EditorGUILayout.LabelField($"Memory: {FormatBytes(kvp.Value.GetMemorySize())}", GUILayout.Width(100));
                    
                    if (GUILayout.Button("Release", GUILayout.Width(50)))
                    {
                        r.ReleaseResource(kvp.Key);
                    }
                    EditorGUILayout.EndHorizontal();
                    
                    EditorGUILayout.EndVertical();
                    EditorGUILayout.Space(2);
                }
                
                EditorGUILayout.Space(5);
                
                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button("Release All Resources"))
                {
                    var resAssets = r.GetResourcesByType(ResourceSourceType.Resources);
                    foreach (var kvp in resAssets)
                    {
                        r.ReleaseResource(kvp.Key);
                    }
                }
                
                if (GUILayout.Button("Release All Addressables"))
                {
                    r.ReleaseAllAddressableHandles();
                }
                EditorGUILayout.EndHorizontal();
            }
            
            EditorGUI.indentLevel--;

            Repaint();
        }

        private List<KeyValuePair<string, ResourceRefInfo>> FilterAndSortAssets(Dictionary<string, ResourceRefInfo> assets)
        {
            var filtered = assets.Where(kvp =>
            {
                if (!showResourcesAssets && kvp.Value.SourceType == ResourceSourceType.Resources)
                    return false;
                if (!showAddressablesAssets && kvp.Value.SourceType == ResourceSourceType.Addressables)
                    return false;
                if (!string.IsNullOrEmpty(searchFilter) && !kvp.Key.ToLower().Contains(searchFilter.ToLower()))
                    return false;
                return true;
            }).ToList();

            switch (sortMode)
            {
                case ResourceSortMode.Name:
                    filtered = sortDescending ? 
                        filtered.OrderByDescending(kvp => kvp.Key).ToList() : 
                        filtered.OrderBy(kvp => kvp.Key).ToList();
                    break;
                case ResourceSortMode.RefCount:
                    filtered = sortDescending ? 
                        filtered.OrderByDescending(kvp => kvp.Value.RefCount).ToList() : 
                        filtered.OrderBy(kvp => kvp.Value.RefCount).ToList();
                    break;
                case ResourceSortMode.LoadTime:
                    filtered = sortDescending ? 
                        filtered.OrderByDescending(kvp => kvp.Value.LoadTime).ToList() : 
                        filtered.OrderBy(kvp => kvp.Value.LoadTime).ToList();
                    break;
                case ResourceSortMode.MemorySize:
                    filtered = sortDescending ? 
                        filtered.OrderByDescending(kvp => kvp.Value.GetMemorySize()).ToList() : 
                        filtered.OrderBy(kvp => kvp.Value.GetMemorySize()).ToList();
                    break;
                case ResourceSortMode.Type:
                    filtered = sortDescending ? 
                        filtered.OrderByDescending(kvp => kvp.Value.AssetType?.Name ?? "").ToList() : 
                        filtered.OrderBy(kvp => kvp.Value.AssetType?.Name ?? "").ToList();
                    break;
            }

            return filtered;
        }

        private string FormatBytes(long bytes)
        {
            if (bytes < 1024)
                return $"{bytes} B";
            else if (bytes < 1024 * 1024)
                return $"{bytes / 1024f:F2} KB";
            else if (bytes < 1024 * 1024 * 1024)
                return $"{bytes / (1024f * 1024f):F2} MB";
            else
                return $"{bytes / (1024f * 1024f * 1024f):F2} GB";
        }
    }
}

