using System.Collections;
using System.Collections.Generic;
using StarryFramework;
using UnityEngine;

internal class FrameworkDebugger
{
    private bool canDebug = true;

    internal void SetDebugActive(bool active)
    {
        canDebug = active;
    }
    private bool CanDebug(FrameworkDebugType type)
    {
        if (FrameworkManager.Setting != null)
            return canDebug && (type <= FrameworkManager.Setting.debugType);
        return true;
    }

    internal void Log(object message)
    {
        if(!CanDebug(FrameworkDebugType.Normal))
            return;
        Debug.Log(message);
    }
    
    internal void Log(object message, Object context)
    {
        if(!CanDebug(FrameworkDebugType.Normal))
            return;
        Debug.Log(message, context);
    }

    internal void LogWarning(object message)
    {
        if(!CanDebug(FrameworkDebugType.Warning))
            return;
        Debug.LogWarning(message);
    }
    
    internal void LogWarning(object message, Object context)
    {
        if(!CanDebug(FrameworkDebugType.Warning))
            return;
        Debug.LogWarning(message, context);
    }
    
    internal void LogError(object message)
    {
        if(!CanDebug(FrameworkDebugType.Error))
            return;
        Debug.LogError(message);
    }
    
    internal void LogError(object message, Object context)
    {
        if(!CanDebug(FrameworkDebugType.Error))
            return;
        Debug.LogError(message, context);
    }
}
