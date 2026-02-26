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
        internal string SuggestedFix { get; }

        internal FrameworkSettingsValidationIssue(
            FrameworkSettingsValidationSeverity severity,
            string code,
            string message,
            string suggestedFix = null)
        {
            Severity = severity;
            Code = code;
            Message = message;
            SuggestedFix = suggestedFix;
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
                    "FrameworkSettings is null.",
                    "Select an existing FrameworkSettings asset or create one from the Settings window."));
                return issues;
            }

            List<ModuleType> modules = settings.modules;
            if (modules == null)
            {
                issues.Add(new FrameworkSettingsValidationIssue(
                    FrameworkSettingsValidationSeverity.Error,
                    "FW_MODULE_LIST_NULL",
                    "Module list can not be null.",
                    "Recreate the settings asset with defaults or initialize the Modules List in FrameworkSettings."));
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
                        $"Duplicate module in the list: {module}.",
                        $"Remove duplicate '{module}' entries and keep only one instance in the Modules List."));
                    break;
                }
            }

            if (settings.InternalEventTrigger && !modules.Contains(ModuleType.Event))
            {
                issues.Add(new FrameworkSettingsValidationIssue(
                    FrameworkSettingsValidationSeverity.Error,
                    "FW_INTERNAL_EVENT_WITHOUT_EVENT_MODULE",
                    "Internal Event Trigger is enabled but Event module is not in the list.",
                    "Enable the Event module in Modules List, or disable Internal Event Trigger."));
            }

            if (settings.StartScene != 0 && !modules.Contains(ModuleType.Scene))
            {
                issues.Add(new FrameworkSettingsValidationIssue(
                    FrameworkSettingsValidationSeverity.Error,
                    "FW_START_SCENE_WITHOUT_SCENE_MODULE",
                    "Start Scene is set but Scene module is not in the list.",
                    "Enable the Scene module in Modules List, or reset Start Scene to the framework scene."));
            }

            return issues;
        }
    }
}
