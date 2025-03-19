using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using System;
using System.Linq;
using Object = UnityEngine.Object;

namespace StarryFramework.Editor
{
    static class EditorUIHelper
    {
        public static void UseVerticalLayout(this UnityEditor.Editor e, Action action, GUIStyle style)
        {
            EditorGUILayout.BeginVertical(style);
            action();
            EditorGUILayout.EndVertical();
        }

        public static void UseButton(this UnityEditor.Editor e, MethodInfo m)
        {
            if (GUILayout.Button(m.Name))
            {
                m.Invoke(e.target, null);
            }
        }
    }


    static class StyleFramework
    {
        public static GUIStyle box;
        public static GUIStyle boxChild;
        public static GUIStyle foldout;
        public static GUIStyle button;
        public static GUIStyle text;

        static StyleFramework()
        {
            bool pro = EditorGUIUtility.isProSkin;

            var uiTex_in = Resources.Load<Texture2D>("IN foldout focus-6510");
            var uiTex_in_on = Resources.Load<Texture2D>("IN foldout focus on-5718");

            var c_on = pro ? Color.white : new Color(51 / 255f, 102 / 255f, 204 / 255f, 1);

            button = new GUIStyle(EditorStyles.miniButton);
            button.font = Font.CreateDynamicFontFromOSFont(new[] { "Terminus (TTF) for Windows", "Calibri" }, 17);

            text = new GUIStyle(EditorStyles.label);
            text.richText = true;
            text.contentOffset = new Vector2(0, 5);
            text.font = Font.CreateDynamicFontFromOSFont(new[] { "Terminus (TTF) for Windows", "Calibri" }, 14);

            foldout = new GUIStyle(EditorStyles.foldout);

            foldout.overflow = new RectOffset(-10, 0, 3, 0);
            foldout.padding = new RectOffset(25, 0, -3, 0);

            foldout.active.textColor = c_on;
            foldout.active.background = uiTex_in;
            foldout.onActive.textColor = c_on;
            foldout.onActive.background = uiTex_in_on;

            foldout.focused.textColor = c_on;
            foldout.focused.background = uiTex_in;
            foldout.onFocused.textColor = c_on;
            foldout.onFocused.background = uiTex_in_on;

            foldout.hover.textColor = c_on;
            foldout.hover.background = uiTex_in;

            foldout.onHover.textColor = c_on;
            foldout.onHover.background = uiTex_in_on;

            box = new GUIStyle(GUI.skin.box);
            box.padding = new RectOffset(10, 0, 10, 0);

            boxChild = new GUIStyle(GUI.skin.box);
            boxChild.active.textColor = c_on;
            boxChild.active.background = uiTex_in;
            boxChild.onActive.textColor = c_on;
            boxChild.onActive.background = uiTex_in_on;

            boxChild.focused.textColor = c_on;
            boxChild.focused.background = uiTex_in;
            boxChild.onFocused.textColor = c_on;
            boxChild.onFocused.background = uiTex_in_on;

            EditorStyles.foldout.active.textColor = c_on;
            EditorStyles.foldout.active.background = uiTex_in;
            EditorStyles.foldout.onActive.textColor = c_on;
            EditorStyles.foldout.onActive.background = uiTex_in_on;

            EditorStyles.foldout.focused.textColor = c_on;
            EditorStyles.foldout.focused.background = uiTex_in;
            EditorStyles.foldout.onFocused.textColor = c_on;
            EditorStyles.foldout.onFocused.background = uiTex_in_on;

            EditorStyles.foldout.hover.textColor = c_on;
            EditorStyles.foldout.hover.background = uiTex_in;

            EditorStyles.foldout.onHover.textColor = c_on;
            EditorStyles.foldout.onHover.background = uiTex_in_on;
        }


        public static string FirstLetterToUpperCase(this string s)
        {
            if (string.IsNullOrEmpty(s))
                return string.Empty;

            var a = s.ToCharArray();
            a[0] = char.ToUpper(a[0]);
            return new string(a);
        }

        /// <summary>
        /// 返回类型的继承树，父类在前，子类在后
        /// </summary>
        /// <param Name="t">输入类型</param>
        /// <returns>继承关系列表</returns>
        public static IList<Type> GetTypeTree(this Type t)
        {
            var types = new List<Type>();
            while (t.BaseType != null)
            {
                types.Add(t);
                t = t.BaseType;
            }
            types.Reverse();

            return types;
        }
    }

    static class EditorTypes
    {
        public static Dictionary<int, List<FieldInfo>> fields = new Dictionary<int, List<FieldInfo>>(FastComparable.Default);

        public static int Get(Object target, out List<FieldInfo> objectFields)
        {
            //var t = target.GetType();
            //var hash = t.GetHashCode();

            //if (!fields.TryGetValue(hash, out objectFields))
            //{
            //	var typeTree = t.GetTypeTree();
            //	objectFields = target.GetType()
            //			.GetFields(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)
            //			.OrderByDescending(x => typeTree.IndexOf(x.DeclaringType))
            //			.ToList();
            //	fields.Add(hash, objectFields);
            //}

            //return objectFields.Count;


            var t = target.GetType();

            var bindingFlags = BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;
            objectFields = GetMembersInclPrivateBase(t, bindingFlags).ToList();

            return objectFields.Count;


        }

        public static FieldInfo[] GetMembersInclPrivateBase(Type t, BindingFlags flags)
        {
            var memberList = new List<FieldInfo>();
            var typeTree = t.GetTypeTree();
            foreach (var a in typeTree)
                memberList.AddRange(a.GetFields(flags));
            return memberList.ToArray();
        }
    }


    class FastComparable : IEqualityComparer<int>
    {
        public static FastComparable Default = new FastComparable();

        public bool Equals(int x, int y)
        {
            return x == y;
        }

        public int GetHashCode(int obj)
        {
            return obj.GetHashCode();
        }
    }


    [InitializeOnLoad]
    public static class EditorFramework
    {
        internal static bool needToRepaint;

        internal static Event currentEvent;
        internal static float t;

        static EditorFramework()
        {
            EditorApplication.update += Updating;
        }


        static void Updating()
        {
            CheckMouse();

            if (needToRepaint)
            {
                t += Time.deltaTime;

                if (t >= 0.3f)
                {
                    t -= 0.3f;
                    needToRepaint = false;
                }
            }
        }

        static void CheckMouse()
        {
            var ev = currentEvent;
            if (ev == null) return;

            if (ev.type == EventType.MouseMove)
                needToRepaint = true;
        }
    }
}
