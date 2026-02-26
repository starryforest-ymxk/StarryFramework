using System;
using System.Collections;
using System.Linq;
using StarryFramework;
using UnityEngine;
using UnityEngine.ResourceManagement.AsyncOperations;

public class TestUI : MonoBehaviour
{
    [SerializeField] private string openUIName;
    [SerializeField] private string multiInstanceUIName;
    [SerializeField] private string defaultUIGroupName = "DefaultUIGroup";
    [SerializeField] private string secondaryUIGroupName = "PopupUIGroup";
    [SerializeField] private bool pauseCoveredUIForm = true;
    [SerializeField] private bool autoRunSmokeTests;

    private const string UpperCaseInstanceKey = "CaseKey";
    private const string LowerCaseInstanceKey = "casekey";
    private const string SerialReassignInstanceKey = "SerialReassign";
    private const string PolicyMixInstanceKey = "PolicyMix";

    private string TestAssetName => string.IsNullOrEmpty(multiInstanceUIName) ? openUIName : multiInstanceUIName;

    private void Awake()
    {
        if (!Framework.UIComponent.HasUIGroup(defaultUIGroupName))
        {
            Framework.UIComponent.AddUIGroup(defaultUIGroupName);
        }

        if (!Framework.UIComponent.HasUIGroup(secondaryUIGroupName))
        {
            Framework.UIComponent.AddUIGroup(secondaryUIGroupName);
        }
    }

    private void Start()
    {
        if (!string.IsNullOrEmpty(openUIName))
        {
            OpenWithOptions(openUIName, defaultUIGroupName, UIOpenPolicy.SingleInstanceGlobal, null);
        }

        if (autoRunSmokeTests)
        {
            StartCoroutine(RunSmokeTests());
        }
    }

    public void OpenSingleInDefaultGroup()
    {
        OpenWithOptions(TestAssetName, defaultUIGroupName, UIOpenPolicy.SingleInstanceGlobal, null);
    }

    public void OpenSingleInSecondaryGroup()
    {
        OpenWithOptions(TestAssetName, secondaryUIGroupName, UIOpenPolicy.SingleInstanceGlobal, null);
    }

    public void OpenSinglePerGroupInDefault()
    {
        OpenWithOptions(TestAssetName, defaultUIGroupName, UIOpenPolicy.SingleInstancePerGroup, null);
    }

    public void OpenSinglePerGroupInSecondary()
    {
        OpenWithOptions(TestAssetName, secondaryUIGroupName, UIOpenPolicy.SingleInstancePerGroup, null);
    }

    public void OpenMultiInDefaultUpperKey()
    {
        OpenWithOptions(TestAssetName, defaultUIGroupName, UIOpenPolicy.MultiInstanceGlobal, UpperCaseInstanceKey);
    }

    public void OpenMultiInDefaultLowerKey()
    {
        OpenWithOptions(TestAssetName, defaultUIGroupName, UIOpenPolicy.MultiInstanceGlobal, LowerCaseInstanceKey);
    }

    public void OpenMultiInSecondaryUpperKey()
    {
        OpenWithOptions(TestAssetName, secondaryUIGroupName, UIOpenPolicy.MultiInstanceGlobal, UpperCaseInstanceKey);
    }

    public void CloseTopmostByName()
    {
        UIForm topmost = Framework.UIComponent.GetTopUIForm(TestAssetName);
        if (topmost != null)
        {
            Framework.UIComponent.CloseUIForm(topmost.SerialID);
        }
    }

    public void CloseAllByName()
    {
        Framework.UIComponent.CloseAllUIForms(TestAssetName);
    }

    public void CloseAllInDefaultGroup()
    {
        Framework.UIComponent.CloseAllUIFormsInGroup(TestAssetName, defaultUIGroupName);
    }

    public void CloseAllByUpperInstanceKey()
    {
        Framework.UIComponent.CloseAllUIFormsByInstanceKey(TestAssetName, UpperCaseInstanceKey);
    }

    public void CloseAllByLowerInstanceKey()
    {
        Framework.UIComponent.CloseAllUIFormsByInstanceKey(TestAssetName, LowerCaseInstanceKey);
    }

    public void RefocusByUpperInstanceKey()
    {
        Framework.UIComponent.RefocusUIForm(TestAssetName, UpperCaseInstanceKey);
    }

    public void RefocusByLowerInstanceKey()
    {
        Framework.UIComponent.RefocusUIForm(TestAssetName, LowerCaseInstanceKey);
    }

    public void RunSerialIdReassignSmokeTest()
    {
        StartCoroutine(RunSerialIdReassignSmokeTestCoroutine());
    }

    public void RunInstanceKeyOrdinalSmokeTest()
    {
        StartCoroutine(RunInstanceKeyOrdinalSmokeTestCoroutine());
    }

    public void RunSingleInstancePerGroupSmokeTest()
    {
        StartCoroutine(RunSingleInstancePerGroupSmokeTestCoroutine());
    }

    public void RunPolicyMixSmokeTest()
    {
        StartCoroutine(RunPolicyMixSmokeTestCoroutine());
    }

    public void RunAsyncDedupSmokeTest()
    {
        StartCoroutine(RunAsyncDedupSmokeTestCoroutine());
    }

    private AsyncOperationHandle<UIForm> OpenWithOptions(string assetName, string groupName, UIOpenPolicy policy, string instanceKey)
    {
        return Framework.UIComponent.OpenUIForm(new OpenUIFormOptions
        {
            AssetName = assetName,
            GroupName = groupName,
            PauseCoveredUIForm = pauseCoveredUIForm,
            OpenPolicy = policy,
            RefocusIfExists = true,
            InstanceKey = instanceKey
        });
    }

    private IEnumerator RunSmokeTests()
    {
        yield return RunSerialIdReassignSmokeTestCoroutine();
        yield return RunInstanceKeyOrdinalSmokeTestCoroutine();
        yield return RunSingleInstancePerGroupSmokeTestCoroutine();
        yield return RunPolicyMixSmokeTestCoroutine();
        yield return RunAsyncDedupSmokeTestCoroutine();
    }

    private IEnumerator RunSerialIdReassignSmokeTestCoroutine()
    {
        if (string.IsNullOrEmpty(TestAssetName))
        {
            Debug.LogError("[TestUI] SerialId smoke test skipped: test asset name is empty.");
            yield break;
        }

        AsyncOperationHandle<UIForm> firstOpen = OpenWithOptions(
            TestAssetName,
            defaultUIGroupName,
            UIOpenPolicy.MultiInstanceGlobal,
            SerialReassignInstanceKey);
        yield return firstOpen;

        UIForm firstForm = firstOpen.Result;
        if (firstForm == null)
        {
            Debug.LogError("[TestUI] SerialId smoke test failed: first open returned null.");
            yield break;
        }

        int firstSerialId = firstForm.SerialID;
        Framework.UIComponent.CloseUIForm(firstSerialId);

        yield return null;

        AsyncOperationHandle<UIForm> secondOpen = OpenWithOptions(
            TestAssetName,
            defaultUIGroupName,
            UIOpenPolicy.MultiInstanceGlobal,
            SerialReassignInstanceKey);
        yield return secondOpen;

        UIForm secondForm = secondOpen.Result;
        if (secondForm == null)
        {
            Debug.LogError("[TestUI] SerialId smoke test failed: second open returned null.");
            yield break;
        }

        int secondSerialId = secondForm.SerialID;
        if (firstSerialId == secondSerialId)
        {
            Debug.LogError($"[TestUI] SerialId smoke test failed: serial id was not reallocated ({firstSerialId}).");
        }
        else
        {
            Debug.Log($"[TestUI] SerialId smoke test passed: {firstSerialId} -> {secondSerialId}.");
        }

        Framework.UIComponent.CloseUIForm(secondSerialId);
    }

    private IEnumerator RunInstanceKeyOrdinalSmokeTestCoroutine()
    {
        if (string.IsNullOrEmpty(TestAssetName))
        {
            Debug.LogError("[TestUI] InstanceKey smoke test skipped: test asset name is empty.");
            yield break;
        }

        AsyncOperationHandle<UIForm> openUpper = OpenWithOptions(
            TestAssetName,
            defaultUIGroupName,
            UIOpenPolicy.MultiInstanceGlobal,
            UpperCaseInstanceKey);
        AsyncOperationHandle<UIForm> openLower = OpenWithOptions(
            TestAssetName,
            defaultUIGroupName,
            UIOpenPolicy.MultiInstanceGlobal,
            LowerCaseInstanceKey);

        yield return openUpper;
        yield return openLower;

        UIForm[] forms = Framework.UIComponent.GetUIForms(TestAssetName);
        int upperCount = forms.Count(form => string.Equals(form.InstanceKey, UpperCaseInstanceKey, StringComparison.Ordinal));
        int lowerCount = forms.Count(form => string.Equals(form.InstanceKey, LowerCaseInstanceKey, StringComparison.Ordinal));

        if (upperCount > 0 && lowerCount > 0)
        {
            Debug.Log($"[TestUI] InstanceKey Ordinal smoke test passed: upper={upperCount}, lower={lowerCount}.");
        }
        else
        {
            Debug.LogError($"[TestUI] InstanceKey Ordinal smoke test failed: upper={upperCount}, lower={lowerCount}.");
        }

        Framework.UIComponent.CloseAllUIFormsByInstanceKey(TestAssetName, UpperCaseInstanceKey);
        UIForm[] remainingForms = Framework.UIComponent.GetUIForms(TestAssetName);
        int remainingLowerCount = remainingForms.Count(form => string.Equals(form.InstanceKey, LowerCaseInstanceKey, StringComparison.Ordinal));

        if (remainingLowerCount > 0)
        {
            Debug.Log($"[TestUI] InstanceKey isolation smoke test passed: lower-case instances remained ({remainingLowerCount}).");
        }
        else
        {
            Debug.LogError("[TestUI] InstanceKey isolation smoke test failed: lower-case instances were unexpectedly removed.");
        }

        Framework.UIComponent.CloseAllUIForms(TestAssetName);
    }

    private IEnumerator RunSingleInstancePerGroupSmokeTestCoroutine()
    {
        if (string.IsNullOrEmpty(TestAssetName))
        {
            Debug.LogError("[TestUI] SingleInstancePerGroup smoke test skipped: test asset name is empty.");
            yield break;
        }

        Framework.UIComponent.CloseAllUIForms(TestAssetName);
        yield return null;

        AsyncOperationHandle<UIForm> openInDefault = OpenWithOptions(
            TestAssetName,
            defaultUIGroupName,
            UIOpenPolicy.SingleInstancePerGroup,
            null);
        AsyncOperationHandle<UIForm> openInSecondary = OpenWithOptions(
            TestAssetName,
            secondaryUIGroupName,
            UIOpenPolicy.SingleInstancePerGroup,
            null);

        yield return openInDefault;
        yield return openInSecondary;

        UIForm[] forms = Framework.UIComponent.GetUIForms(TestAssetName);
        bool hasDefault = forms.Any(form => form.UIGroup != null && form.UIGroup.Name == defaultUIGroupName);
        bool hasSecondary = forms.Any(form => form.UIGroup != null && form.UIGroup.Name == secondaryUIGroupName);

        if (forms.Length >= 2 && hasDefault && hasSecondary)
        {
            Debug.Log($"[TestUI] SingleInstancePerGroup smoke test passed: total={forms.Length}.");
        }
        else
        {
            Debug.LogError($"[TestUI] SingleInstancePerGroup smoke test failed: total={forms.Length}, hasDefault={hasDefault}, hasSecondary={hasSecondary}.");
        }

        Framework.UIComponent.CloseAllUIForms(TestAssetName);
    }

    private IEnumerator RunPolicyMixSmokeTestCoroutine()
    {
        if (string.IsNullOrEmpty(TestAssetName))
        {
            Debug.LogError("[TestUI] Policy mix smoke test skipped: test asset name is empty.");
            yield break;
        }

        Framework.UIComponent.CloseAllUIForms(TestAssetName);
        yield return null;

        AsyncOperationHandle<UIForm> singleGlobalOpen = OpenWithOptions(
            TestAssetName,
            defaultUIGroupName,
            UIOpenPolicy.SingleInstanceGlobal,
            null);
        yield return singleGlobalOpen;

        AsyncOperationHandle<UIForm> multiOpen = OpenWithOptions(
            TestAssetName,
            secondaryUIGroupName,
            UIOpenPolicy.MultiInstanceGlobal,
            PolicyMixInstanceKey);
        yield return multiOpen;

        UIForm[] forms = Framework.UIComponent.GetUIForms(TestAssetName);
        bool hasDefault = forms.Any(form => form.UIGroup != null && form.UIGroup.Name == defaultUIGroupName);
        bool hasSecondary = forms.Any(form => form.UIGroup != null && form.UIGroup.Name == secondaryUIGroupName);

        if (forms.Length >= 2 && hasDefault && hasSecondary)
        {
            Debug.Log($"[TestUI] Policy mix smoke test passed: total={forms.Length}.");
        }
        else
        {
            Debug.LogError($"[TestUI] Policy mix smoke test failed: total={forms.Length}, hasDefault={hasDefault}, hasSecondary={hasSecondary}.");
        }

        Framework.UIComponent.CloseAllUIForms(TestAssetName);
    }

    private IEnumerator RunAsyncDedupSmokeTestCoroutine()
    {
        if (string.IsNullOrEmpty(TestAssetName))
        {
            Debug.LogError("[TestUI] Async dedup smoke test skipped: test asset name is empty.");
            yield break;
        }

        Framework.UIComponent.CloseAllUIForms(TestAssetName);
        yield return null;

        AsyncOperationHandle<UIForm> openA = OpenWithOptions(
            TestAssetName,
            defaultUIGroupName,
            UIOpenPolicy.SingleInstanceGlobal,
            null);
        AsyncOperationHandle<UIForm> openB = OpenWithOptions(
            TestAssetName,
            defaultUIGroupName,
            UIOpenPolicy.SingleInstanceGlobal,
            null);

        yield return openA;
        yield return openB;

        UIForm formA = openA.Result;
        UIForm formB = openB.Result;
        UIForm[] forms = Framework.UIComponent.GetUIForms(TestAssetName);

        bool sameHandle = openA.Equals(openB);
        bool sameInstance = formA != null && formB != null && formA.SerialID == formB.SerialID;
        bool oneOpened = forms.Length == 1;

        if ((sameHandle || sameInstance) && oneOpened)
        {
            Debug.Log($"[TestUI] Async dedup smoke test passed: sameHandle={sameHandle}, sameInstance={sameInstance}, activeCount={forms.Length}.");
        }
        else
        {
            Debug.LogError($"[TestUI] Async dedup smoke test failed: sameHandle={sameHandle}, sameInstance={sameInstance}, activeCount={forms.Length}.");
        }

        Framework.UIComponent.CloseAllUIForms(TestAssetName);
    }
}
