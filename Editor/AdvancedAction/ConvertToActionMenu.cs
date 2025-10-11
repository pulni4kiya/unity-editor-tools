using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEditor;
using UnityEditor.Events;
using UnityEngine;
using UnityEngine.Events;

namespace Pulni.EditorTools.Editor {
	[InitializeOnLoad]
	public static class ConvertToActionMenu {
		private const string UnityEventCallsPath = ".m_PersistentCalls.m_Calls";

		static ConvertToActionMenu() {
			EditorApplication.contextualPropertyMenu += OnContextMenuOpening;
		}

		private static void OnContextMenuOpening(GenericMenu menu, SerializedProperty property) {
			if (property.serializedObject.targetObjects.Length > 1) return;

			// Check if this is a UnityEvent property
			if (!IsUnityEventProperty(property)) return;

			var targetObject = property.serializedObject.targetObject;
			var comp = targetObject as Component;
			var go = targetObject as GameObject;
			if (comp == null && go == null) return;

			if (go == null) {
				go = comp.gameObject;
			}

			AddConvertToActionMenuItem(menu, go, property);
		}

		private static bool IsUnityEventProperty(SerializedProperty property) {
			return property.propertyPath.EndsWith(UnityEventCallsPath);
		}

		private static void AddConvertToActionMenuItem(GenericMenu menu, GameObject go, SerializedProperty property) {
			var localProperty = property.Copy();
			menu.AddItem(new GUIContent("Convert to action"), false, () => {
				var manualAction = go.GetOrAddComponent<ManualAction>();
				var eventProperty = localProperty.serializedObject.FindProperty(localProperty.propertyPath.Substring(0, localProperty.propertyPath.Length - UnityEventCallsPath.Length));
				if (eventProperty.boxedValue is UnityEventBase unityEvent) {
					// Add a persistent listener to the UnityEvent that calls the ManualAction
					UnityEventTools.AddVoidPersistentListener(unityEvent, new UnityAction(manualAction.Invoke));
					eventProperty.boxedValue = unityEvent;
					eventProperty.serializedObject.ApplyModifiedProperties();
				}

				EditorUtility.SetDirty(manualAction);
				EditorUtility.SetDirty(eventProperty.serializedObject.targetObject);
			});
		}
	}


}