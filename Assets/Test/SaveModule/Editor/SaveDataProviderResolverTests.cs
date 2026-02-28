using System;
using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.Events;

namespace StarryFramework.Tests.SaveModule
{
    public class SaveDataProviderResolverTests
    {
        [Test]
        public void ResolveFromTypes_SelectsHighestPriorityProvider()
        {
            ISaveDataProvider fallback = new FallbackProvider();
            Type[] types =
            {
                typeof(LowPriorityProvider),
                typeof(HighPriorityProvider),
                typeof(NoAttributeProvider)
            };

            ISaveDataProvider selected = SaveDataProviderResolver.ResolveFromTypes(types, fallback);

            Assert.IsNotNull(selected);
            Assert.AreEqual(typeof(HighPriorityProvider), selected.GetType());
        }

        [Test]
        public void ResolveFromTypes_WhenSamePriority_UsesTypeFullNameOrder()
        {
            ISaveDataProvider fallback = new FallbackProvider();
            Type[] types =
            {
                typeof(TieZProvider),
                typeof(TieAProvider)
            };

            ISaveDataProvider selected = SaveDataProviderResolver.ResolveFromTypes(types, fallback);

            Assert.IsNotNull(selected);
            Assert.AreEqual(typeof(TieAProvider), selected.GetType());
        }

        [Test]
        public void ResolveFromTypes_WhenNoCandidate_ReturnsFallback()
        {
            ISaveDataProvider fallback = new FallbackProvider();

            ISaveDataProvider selected = SaveDataProviderResolver.ResolveFromTypes(Array.Empty<Type>(), fallback);

            Assert.AreSame(fallback, selected);
        }

        [Test]
        public void ResolveFromTypes_IgnoresInvalidCandidateAndFallsBack()
        {
            ISaveDataProvider fallback = new FallbackProvider();
            Type[] types =
            {
                typeof(AttributedButNotProvider),
                typeof(NullTypeProvider)
            };

            ISaveDataProvider selected = SaveDataProviderResolver.ResolveFromTypes(types, fallback);

            Assert.AreSame(fallback, selected);
        }

        [Test]
        public void SaveManager_Awake_UsesResolvedProviderForFirstGameSettingsLoad()
        {
            PlayerPrefs.DeleteKey("Settings");
            PlayerPrefs.Save();

            SaveManager manager = new SaveManager();
            ((IManager)manager).Awake();

            object settings = manager.GameSettingsObject;

            Assert.IsNotNull(settings);
            Assert.AreNotEqual(typeof(GameSettings), settings.GetType());

            ((IManager)manager).ShutDown();
        }

        [Test]
        public void EventComponent_BoolFieldMappingAction_SetsFieldToTrue()
        {
            GameObject go = new GameObject("SaveModuleEventLinkageTest");
            EventComponent component = go.AddComponent<EventComponent>();

            try
            {
                LinkageData data = new LinkageData();

                MethodInfo initMethod = typeof(EventComponent).GetMethod("InitActionsDic", BindingFlags.Instance | BindingFlags.NonPublic);
                FieldInfo actionsField = typeof(EventComponent).GetField("triggerActions", BindingFlags.Instance | BindingFlags.NonPublic);

                Assert.IsNotNull(initMethod);
                Assert.IsNotNull(actionsField);

                initMethod.Invoke(component, new object[] { data });
                Dictionary<string, UnityAction> actions = actionsField.GetValue(component) as Dictionary<string, UnityAction>;

                Assert.IsNotNull(actions);
                Assert.IsTrue(actions.ContainsKey(nameof(LinkageData.EventFlag)));
                Assert.IsFalse(data.EventFlag);

                actions[nameof(LinkageData.EventFlag)].Invoke();
                Assert.IsTrue(data.EventFlag);
            }
            finally
            {
                FrameworkComponent.DeleteComponent(component);
                UnityEngine.Object.DestroyImmediate(go);
            }
        }

        [Serializable]
        private class DummyPlayerData
        {
        }

        [Serializable]
        private class DummyGameSettings
        {
        }

        private class LinkageData
        {
            public bool EventFlag;
            public int Count;
        }

        [SaveDataProvider(priority: 1)]
        private class LowPriorityProvider : ISaveDataProvider
        {
            public Type PlayerDataType => typeof(DummyPlayerData);
            public Type GameSettingsType => typeof(DummyGameSettings);
            public object CreateDefaultPlayerData() => new DummyPlayerData();
            public object CreateDefaultGameSettings() => new DummyGameSettings();
        }

        [SaveDataProvider(priority: 2)]
        private class HighPriorityProvider : ISaveDataProvider
        {
            public Type PlayerDataType => typeof(DummyPlayerData);
            public Type GameSettingsType => typeof(DummyGameSettings);
            public object CreateDefaultPlayerData() => new DummyPlayerData();
            public object CreateDefaultGameSettings() => new DummyGameSettings();
        }

        [SaveDataProvider(priority: 2)]
        private class TieAProvider : ISaveDataProvider
        {
            public Type PlayerDataType => typeof(DummyPlayerData);
            public Type GameSettingsType => typeof(DummyGameSettings);
            public object CreateDefaultPlayerData() => new DummyPlayerData();
            public object CreateDefaultGameSettings() => new DummyGameSettings();
        }

        [SaveDataProvider(priority: 2)]
        private class TieZProvider : ISaveDataProvider
        {
            public Type PlayerDataType => typeof(DummyPlayerData);
            public Type GameSettingsType => typeof(DummyGameSettings);
            public object CreateDefaultPlayerData() => new DummyPlayerData();
            public object CreateDefaultGameSettings() => new DummyGameSettings();
        }

        private class NoAttributeProvider : ISaveDataProvider
        {
            public Type PlayerDataType => typeof(DummyPlayerData);
            public Type GameSettingsType => typeof(DummyGameSettings);
            public object CreateDefaultPlayerData() => new DummyPlayerData();
            public object CreateDefaultGameSettings() => new DummyGameSettings();
        }

        [SaveDataProvider(priority: 1)]
        private class AttributedButNotProvider
        {
        }

        [SaveDataProvider(priority: 1)]
        private class NullTypeProvider : ISaveDataProvider
        {
            public Type PlayerDataType => null;
            public Type GameSettingsType => typeof(DummyGameSettings);
            public object CreateDefaultPlayerData() => new DummyPlayerData();
            public object CreateDefaultGameSettings() => new DummyGameSettings();
        }

        private class FallbackProvider : ISaveDataProvider
        {
            public Type PlayerDataType => typeof(DummyPlayerData);
            public Type GameSettingsType => typeof(DummyGameSettings);
            public object CreateDefaultPlayerData() => new DummyPlayerData();
            public object CreateDefaultGameSettings() => new DummyGameSettings();
        }
    }
}
