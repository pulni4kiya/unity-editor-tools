using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Pulni.EditorTools {
	public static class DebugHelper {
		private static HashSet<MonoBehaviour> _components = new HashSet<MonoBehaviour>();
		private static HashSet<GameObject> _gameObjects = new HashSet<GameObject>();

		public static bool Check(MonoBehaviour component) {
			return _components.Contains(component);
		}

		[System.Diagnostics.Conditional("UNITY_EDITOR")]
		public static void BreakIfDebugged(MonoBehaviour component) {
			if (!Check(component)) return;
			System.Diagnostics.Debugger.Break();
		}

		[System.Diagnostics.Conditional("UNITY_EDITOR")]
		public static void ActionIfDebugged(MonoBehaviour component, Action action) {
			if (!Check(component)) return;
			action();
		}

		[System.Diagnostics.Conditional("UNITY_EDITOR")]
		public static void LogIfDebugged(MonoBehaviour component, object message, UnityEngine.Object context = null, LogType logType = LogType.Log) {
			if (!Check(component)) return;
			Debug.unityLogger.Log(logType, message, context);
		}

#if UNITY_EDITOR

		[UnityEditor.MenuItem("CONTEXT/MonoBehaviour/Toggle Debug")]
		private static void DebugMono(UnityEditor.MenuCommand command) {
			var component = (MonoBehaviour)command.context;
			if (_components.Contains(component)) {
				_components.Remove(component);
				Debug.Log($"Debugging disabled for {component} - {component.GetInstanceID()}");
			} else {
				_components.Add(component);
				Debug.Log($"Debugging enabled for {component} - {component.GetInstanceID()}");
			}
		}

		[UnityEditor.MenuItem("GameObject/Toggle Debug")]
		private static void DebugGameObject(UnityEditor.MenuCommand command) {
			var go = (GameObject)command.context;
			if (_gameObjects.Contains(go)) {
				_gameObjects.Remove(go);
				_components.ExceptWith(go.GetComponents<MonoBehaviour>());
				Debug.Log($"Debugging disabled for all components on game object: {go} - {_components.Count}");
			} else {
				_gameObjects.Add(go);
				_components.UnionWith(go.GetComponents<MonoBehaviour>());
				Debug.Log($"Debugging enabled for all components on game object: {go}- {_components.Count}");
			}
		}

#endif
	}
}
