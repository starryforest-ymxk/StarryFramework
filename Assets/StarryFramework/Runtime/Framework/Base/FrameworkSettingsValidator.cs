using System.Collections.Generic;

namespace StarryFramework
{
    internal enum FrameworkSettingsValidationSeverity
    {
        Warning,
        Error
    }

    internal sealed class FrameworkSettingsValidationIssue
    {
        internal FrameworkSettingsValidationSeverity Severity { get; }
        internal string Code { get; }
        internal string Message { get; }

        internal FrameworkSettingsValidationIssue(FrameworkSettingsValidationSeverity severity, string code, string message)
        {
            Severity = severity;
            Code = code;
            Message = message;
        }
    }

    internal static class FrameworkSettingsValidator
    {
        internal static List<FrameworkSettingsValidationIssue> Validate(FrameworkSettings settings)
        {
            List<FrameworkSettingsValidationIssue> issues = new();

            if (settings == null)
            {
                issues.Add(new FrameworkSettingsValidationIssue(
                    FrameworkSettingsValidationSeverity.Error,
                    "FW_SETTINGS_NULL",
                    "FrameworkSettings is null."));
                return issues;
            }

            List<ModuleType> modules = settings.modules;
            if (modules == null)
            {
                issues.Add(new FrameworkSettingsValidationIssue(
                    FrameworkSettingsValidationSeverity.Error,
                    "FW_MODULE_LIST_NULL",
                    "Module list can not be null."));
                modules = new List<ModuleType>();
            }

            HashSet<ModuleType> uniqueCheck = new();
            foreach (ModuleType module in modules)
            {
                if (!uniqueCheck.Add(module))
                {
                    issues.Add(new FrameworkSettingsValidationIssue(
                        FrameworkSettingsValidationSeverity.Error,
                        "FW_MODULE_DUPLICATE",
                        $"Duplicate module in the list: {module}."));
                    break;
                }
            }

            if (settings.InternalEventTrigger && !modules.Contains(ModuleType.Event))
            {
                issues.Add(new FrameworkSettingsValidationIssue(
                    FrameworkSettingsValidationSeverity.Error,
                    "FW_INTERNAL_EVENT_WITHOUT_EVENT_MODULE",
                    "Internal Event Trigger is enabled but Event module is not in the list."));
            }

            if (settings.StartScene != 0 && !modules.Contains(ModuleType.Scene))
            {
                issues.Add(new FrameworkSettingsValidationIssue(
                    FrameworkSettingsValidationSeverity.Error,
                    "FW_START_SCENE_WITHOUT_SCENE_MODULE",
                    "Start Scene is set but Scene module is not in the list."));
            }

            return issues;
        }
    }
}
