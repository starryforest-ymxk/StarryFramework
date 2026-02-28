using UnityEngine;

namespace StarryFramework.Extensions
{
    internal static class ModuleRegistration
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void RegisterAudioModule()
        {
            FrameworkManager.RegisterModuleManagerType(ModuleType.Audio, typeof(AudioManager));
            FrameworkManager.RegisterModuleComponentType(ModuleType.Audio, typeof(AudioComponent));
        }
    }
}
