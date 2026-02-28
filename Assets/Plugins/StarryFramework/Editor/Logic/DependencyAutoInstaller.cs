using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.PackageManager;
using UnityEditor.PackageManager.Requests;
using UnityEngine;

namespace StarryFramework.Editor
{
    [InitializeOnLoad]
    internal static class DependencyAutoInstaller
    {
        private const string SessionCheckedKey = "StarryFramework.Dependencies.CheckedThisSession";

        private static readonly PackageRequirement[] RequiredPackages =
        {
            new("com.unity.addressables", "1.21.21"),
            new("com.unity.nuget.newtonsoft-json", "3.2.2")
        };

        private static ListRequest _listRequest;
        private static AddRequest _addRequest;
        private static readonly Queue<PackageRequirement> InstallQueue = new();

        private static bool _isRunning;
        private static bool _isAutoMode;

        static DependencyAutoInstaller()
        {
            EditorApplication.delayCall += AutoCheckOncePerSession;
        }

        [MenuItem("Tools/StarryFramework/Dependencies/Check And Install", priority = 20)]
        private static void CheckAndInstallFromMenu()
        {
            RunCheck(autoMode: false);
        }

        private static void AutoCheckOncePerSession()
        {
            if (Application.isBatchMode)
            {
                return;
            }

            if (SessionState.GetBool(SessionCheckedKey, false))
            {
                return;
            }

            SessionState.SetBool(SessionCheckedKey, true);
            RunCheck(autoMode: true);
        }

        private static void RunCheck(bool autoMode)
        {
            if (_isRunning)
            {
                return;
            }

            _isRunning = true;
            _isAutoMode = autoMode;
            InstallQueue.Clear();

            _listRequest = Client.List(true, false);
            EditorApplication.update += ProgressListRequest;
        }

        private static void ProgressListRequest()
        {
            if (_listRequest == null || !_listRequest.IsCompleted)
            {
                return;
            }

            EditorApplication.update -= ProgressListRequest;

            if (_listRequest.Status != StatusCode.Success)
            {
                _isRunning = false;
                Debug.LogError($"[StarryFramework] Dependency check failed: {_listRequest.Error?.message}");
                return;
            }

            HashSet<string> installed = new(StringComparer.OrdinalIgnoreCase);
            foreach (UnityEditor.PackageManager.PackageInfo package in _listRequest.Result)
            {
                installed.Add(package.name);
            }

            foreach (PackageRequirement requirement in RequiredPackages)
            {
                if (!installed.Contains(requirement.Name))
                {
                    InstallQueue.Enqueue(requirement);
                }
            }

            if (InstallQueue.Count == 0)
            {
                _isRunning = false;
                if (!_isAutoMode)
                {
                    Debug.Log("[StarryFramework] Dependencies are already satisfied.");
                }
                return;
            }

            Debug.Log($"[StarryFramework] Missing dependencies detected: {InstallQueue.Count}. Starting installation...");
            InstallNext();
        }

        private static void InstallNext()
        {
            if (InstallQueue.Count == 0)
            {
                _isRunning = false;
                AssetDatabase.Refresh();
                Debug.Log("[StarryFramework] Dependency installation completed.");
                return;
            }

            PackageRequirement requirement = InstallQueue.Dequeue();
            string packageRef = $"{requirement.Name}@{requirement.Version}";

            Debug.Log($"[StarryFramework] Installing {packageRef}");
            _addRequest = Client.Add(packageRef);
            EditorApplication.update += ProgressAddRequest;
        }

        private static void ProgressAddRequest()
        {
            if (_addRequest == null || !_addRequest.IsCompleted)
            {
                return;
            }

            EditorApplication.update -= ProgressAddRequest;

            if (_addRequest.Status == StatusCode.Success)
            {
                Debug.Log($"[StarryFramework] Installed {_addRequest.Result.name}@{_addRequest.Result.version}");
            }
            else
            {
                Debug.LogError($"[StarryFramework] Install failed: {_addRequest.Error?.message}");
            }

            InstallNext();
        }

        private readonly struct PackageRequirement
        {
            public string Name { get; }
            public string Version { get; }

            public PackageRequirement(string name, string version)
            {
                Name = name;
                Version = version;
            }
        }
    }
}
