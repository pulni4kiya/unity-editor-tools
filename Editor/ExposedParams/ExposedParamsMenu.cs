using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Pulni.EditorTools {
	[InitializeOnLoad]
	public static class ExposedParamsMenu {
		private const string UnityEventCallsPath = ".m_PersistentCalls.m_Calls";

		static ExposedParamsMenu() {
			EditorApplication.contextualPropertyMenu += OnContextMenuOpening;
		}

		private static void OnContextMenuOpening(GenericMenu menu, SerializedProperty property) {
			if (property.serializedObject.targetObjects.Length > 1) return;

			var targetObject = property.serializedObject.targetObject;
			var comp = targetObject as Component;
			var go = targetObject as GameObject;
			if (comp == null && go == null) return;

			if (go == null) {
				go = comp.gameObject;
			}

			AddMenuItem(menu, go, targetObject, property, "Here");
			foreach (var exposedParams in go.GetComponentsInParent<ExposedParams>()) {
				AddMenuItem(menu, exposedParams.gameObject, targetObject, property);
			}
		}

		private static void AddMenuItem(GenericMenu menu, GameObject go, UnityEngine.Object targetObject, SerializedProperty property, string nameOverride = null) {
			var localProperty = property.Copy();
			menu.AddItem(new GUIContent($"Expose property/{nameOverride ?? $"In '{go.name}'"}"), false, () => {
				var exposedParams = go.GetComponent<ExposedParams>();
				if (exposedParams == null) {
					exposedParams = go.AddComponent<ExposedParams>();
				}
				var propertyPath = localProperty.propertyPath;
				if (propertyPath.EndsWith(UnityEventCallsPath)) {
					propertyPath = propertyPath.Substring(0, propertyPath.Length - UnityEventCallsPath.Length);
				}
				exposedParams.ExposedParamsConfig.Params.Add(new ExposedParams.Param() {
					Target = targetObject,
					PropertyPath = propertyPath,
					Label = localProperty.displayName,
				});
				EditorUtility.SetDirty(exposedParams);
			});
		}
	}
}
