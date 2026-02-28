using System;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace StarryFramework
{
    /// <summary>
    /// Shared helper base for components that own a serializable settings object
    /// and push it into a configurable manager.
    /// </summary>
    public abstract class ConfigurableComponent : BaseComponent
    {
        protected static void EnsureSettingsInstance<TSettings>(ref TSettings settings)
            where TSettings : class, new()
        {
            settings ??= new TSettings();
        }

        protected void ResolveAndApplyConfigurableSettings<TManager, TSettings>(
            ref TManager manager,
            ref TSettings settings,
            Func<TManager> managerResolver)
            where TManager : class
            where TSettings : class, new()
        {
            EnsureSettingsInstance(ref settings);
            manager ??= managerResolver?.Invoke();
            ApplyConfigurableSettings(manager, settings);
        }

        protected static void ApplyConfigurableSettings<TManager, TSettings>(TManager manager, TSettings settings)
            where TManager : class
            where TSettings : class
        {
            if (manager is IConfigurableManager configurableManager && settings is IManagerSettings managerSettings)
            {
                configurableManager.SetSettings(managerSettings);
            }
        }

#if UNITY_EDITOR
        protected static void HotApplyConfigurableSettingsInPlayMode<TManager, TSettings>(TManager manager, ref TSettings settings)
            where TManager : class
            where TSettings : class, new()
        {
            EnsureSettingsInstance(ref settings);
            if (!EditorApplication.isPlaying || manager == null) return;

            ApplyConfigurableSettings(manager, settings);
        }
#endif
    }
}
