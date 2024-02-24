#if !PULNI_NO_DBG

using System;
using Pulni.EditorTools;
using UnityEngine;

public static class DBG {
	public static bool Check(MonoBehaviour component) => DebugHelper.Check(component);

	[System.Diagnostics.Conditional("UNITY_EDITOR")]
	public static void BreakIfDebugged(MonoBehaviour component) => DebugHelper.BreakIfDebugged(component);

	[System.Diagnostics.Conditional("UNITY_EDITOR")]
	public static void ActionIfDebugged(MonoBehaviour component, Action action) => DebugHelper.ActionIfDebugged(component, action);

	[System.Diagnostics.Conditional("UNITY_EDITOR")]
	public static void LogIfDebugged(MonoBehaviour component, object message, UnityEngine.Object context = null, LogType logType = LogType.Log)
		=> DebugHelper.LogIfDebugged(component, message, context, logType);
}

#endif