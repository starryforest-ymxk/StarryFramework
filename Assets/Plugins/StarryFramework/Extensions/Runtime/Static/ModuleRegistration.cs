using UnityEngine;

namespace StarryFramework.Extentions
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
