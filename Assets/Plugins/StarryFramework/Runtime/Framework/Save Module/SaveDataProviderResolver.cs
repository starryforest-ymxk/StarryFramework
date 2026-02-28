using System;
using System.Collections.Generic;
using System.Reflection;

namespace StarryFramework
{
    internal static class SaveDataProviderResolver
    {
        private sealed class ProviderCandidate
        {
            internal Type ProviderType;
            internal int Priority;
            internal ISaveDataProvider Provider;
        }

        internal static ISaveDataProvider Resolve(ISaveDataProvider fallbackProvider)
        {
            List<Type> allTypes = CollectAllAssemblyTypes();
            return ResolveFromTypes(allTypes, fallbackProvider);
        }

        internal static ISaveDataProvider ResolveFromTypes(IEnumerable<Type> candidateTypes, ISaveDataProvider fallbackProvider)
        {
            if (fallbackProvider == null)
            {
                FrameworkManager.Debugger.LogError("内置 SaveDataProvider 不可为空。");
                return null;
            }

            List<ProviderCandidate> candidates = CollectCandidates(candidateTypes);
            if (candidates.Count == 0)
            {
                FrameworkManager.Debugger.LogWarning("未发现带 [SaveDataProvider] 特性的 Provider，已回退内置数据提供器。");
                return fallbackProvider;
            }

            candidates.Sort(CompareCandidate);
            ProviderCandidate selected = candidates[0];
            if (selected.Provider == null)
            {
                FrameworkManager.Debugger.LogError("自动发现的 SaveDataProvider 实例为空，已回退内置数据提供器。");
                return fallbackProvider;
            }

            List<string> samePriorityProviders = new List<string>();
            foreach (ProviderCandidate candidate in candidates)
            {
                if (candidate.Priority != selected.Priority)
                {
                    break;
                }

                samePriorityProviders.Add(candidate.ProviderType.FullName);
            }

            if (samePriorityProviders.Count > 1)
            {
                FrameworkManager.Debugger.LogWarning(
                    $"检测到多个同优先级 SaveDataProvider（Priority={selected.Priority}），已按类型全名字典序选中：{selected.ProviderType.FullName}。候选：{string.Join(", ", samePriorityProviders)}");
            }

            return selected.Provider;
        }

        private static List<Type> CollectAllAssemblyTypes()
        {
            List<Type> types = new List<Type>();
            Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
            foreach (Assembly assembly in assemblies)
            {
                if (assembly == null || assembly.IsDynamic)
                {
                    continue;
                }

                string assemblyName = assembly.GetName().Name;
                if (!string.IsNullOrEmpty(assemblyName) &&
                    assemblyName.EndsWith(".Tests", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                try
                {
                    types.AddRange(assembly.GetTypes());
                }
                catch (ReflectionTypeLoadException exception)
                {
                    if (exception.Types != null)
                    {
                        types.AddRange(exception.Types);
                    }
                }
                catch (Exception exception)
                {
                    FrameworkManager.Debugger.LogWarning($"扫描程序集失败，已跳过。程序集: {assembly.FullName}，原因: {exception.Message}");
                }
            }

            return types;
        }

        private static List<ProviderCandidate> CollectCandidates(IEnumerable<Type> sourceTypes)
        {
            List<ProviderCandidate> candidates = new List<ProviderCandidate>();
            if (sourceTypes == null)
            {
                return candidates;
            }

            foreach (Type type in sourceTypes)
            {
                if (type == null || type.IsAbstract || type.IsInterface || type.IsGenericTypeDefinition)
                {
                    continue;
                }

                SaveDataProviderAttribute attribute = type.GetCustomAttribute<SaveDataProviderAttribute>(false);
                if (attribute == null)
                {
                    continue;
                }

                if (!typeof(ISaveDataProvider).IsAssignableFrom(type))
                {
                    FrameworkManager.Debugger.LogError(
                        $"类型 {type.FullName} 带有 [SaveDataProvider]，但未实现 ISaveDataProvider，已忽略。");
                    continue;
                }

                if (type.GetConstructor(Type.EmptyTypes) == null)
                {
                    FrameworkManager.Debugger.LogError(
                        $"类型 {type.FullName} 缺少无参构造，无法自动实例化 SaveDataProvider，已忽略。");
                    continue;
                }

                ISaveDataProvider provider;
                try
                {
                    provider = Activator.CreateInstance(type) as ISaveDataProvider;
                }
                catch (Exception exception)
                {
                    FrameworkManager.Debugger.LogError(
                        $"实例化 SaveDataProvider 失败。类型: {type.FullName}，原因: {exception.Message}");
                    continue;
                }

                if (provider == null)
                {
                    FrameworkManager.Debugger.LogError($"实例化 SaveDataProvider 结果为空。类型: {type.FullName}");
                    continue;
                }

                if (provider.PlayerDataType == null || provider.GameSettingsType == null)
                {
                    FrameworkManager.Debugger.LogError(
                        $"SaveDataProvider 返回了空类型。类型: {type.FullName}，已忽略。");
                    continue;
                }

                candidates.Add(new ProviderCandidate
                {
                    ProviderType = type,
                    Priority = attribute.Priority,
                    Provider = provider
                });
            }

            return candidates;
        }

        private static int CompareCandidate(ProviderCandidate left, ProviderCandidate right)
        {
            if (ReferenceEquals(left, right))
            {
                return 0;
            }

            if (left == null)
            {
                return 1;
            }

            if (right == null)
            {
                return -1;
            }

            int priorityCompare = right.Priority.CompareTo(left.Priority);
            if (priorityCompare != 0)
            {
                return priorityCompare;
            }

            string leftName = left.ProviderType?.FullName ?? string.Empty;
            string rightName = right.ProviderType?.FullName ?? string.Empty;
            return string.Compare(leftName, rightName, StringComparison.Ordinal);
        }
    }
}
