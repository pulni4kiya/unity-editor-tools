using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Pulni.EditorTools.Editor {
	[InitializeOnLoad]
	public static class ExposedPropertiesMenu {
		private const string UnityEventCallsPath = ".m_PersistentCalls.m_Calls";

		public static ExposedProperties.Param ManualProeprty { get; private set; }

		static ExposedPropertiesMenu() {
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
			foreach (var exposedProperties in go.GetComponentsInParent<ExposedProperties>()) {
				AddMenuItem(menu, exposedProperties.gameObject, targetObject, property);
			}
			AddCopyItem(menu, targetObject, property);
		}

		private static void AddMenuItem(GenericMenu menu, GameObject go, UnityEngine.Object targetObject, SerializedProperty property, string nameOverride = null) {
			var localProperty = property.Copy();
			menu.AddItem(new GUIContent($"Expose property/{nameOverride ?? $"In '{go.name}'"}"), false, () => {
				var exposedProperties = go.GetComponent<ExposedProperties>();
				if (exposedProperties == null) {
					exposedProperties = go.AddComponent<ExposedProperties>();
				}

				var propertyPath = FixPropertyPath(localProperty.propertyPath);

				exposedProperties.ExposedPropertiesConfig.Properties.Add(new ExposedProperties.Param() {
					Target = targetObject,
					PropertyPath = propertyPath,
					Label = localProperty.displayName,
				});
				EditorUtility.SetDirty(exposedProperties);
			});
		}

		private static void AddCopyItem(GenericMenu menu, Object targetObject, SerializedProperty property) {
			var localProperty = property.Copy();
			menu.AddItem(new GUIContent($"Expose property/Manual"), false, () => {
				var propertyPath = FixPropertyPath(localProperty.propertyPath);
				ManualProeprty = new ExposedProperties.Param() {
					Target = targetObject,
					PropertyPath = propertyPath,
					Label = localProperty.displayName,
				};
			});
		}

		private static string FixPropertyPath(string propertyPath) {
			if (propertyPath.EndsWith(UnityEventCallsPath)) {
				propertyPath = propertyPath.Substring(0, propertyPath.Length - UnityEventCallsPath.Length);
			}
			return propertyPath;
		}
	}
}
