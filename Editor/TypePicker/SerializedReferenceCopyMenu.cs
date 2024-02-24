using System;
using System.Collections;
using System.Collections.Generic;
using Pulni.EditorTools;
using UnityEditor;
using UnityEngine;

[InitializeOnLoad]
public static class SerializedReferenceCopyMenu {
	private static string _lastCopied;
	private static Type _lastCopiedType;

	static SerializedReferenceCopyMenu() {
		EditorApplication.contextualPropertyMenu += OnContextMenuOpening;
	}

	private static void OnContextMenuOpening(GenericMenu menu, SerializedProperty property) {
		if (property.serializedObject.targetObjects.Length > 1) return;

		if (property.propertyType != SerializedPropertyType.ManagedReference) return;

		var localProp = property.Copy();
		menu.AddItem(new GUIContent("Copy as value"), false, () => {
			_lastCopied = JsonUtility.ToJson(localProp.boxedValue);
			_lastCopiedType = localProp.boxedValue.GetType();
		});

		var propType = TypePickerHelper.GetActualType(property.managedReferenceFieldTypename);
		if (propType.IsAssignableFrom(_lastCopiedType)) {
			menu.AddItem(new GUIContent("Paste value"), false, () => {
				var newCopy = JsonUtility.FromJson(_lastCopied, _lastCopiedType);
				localProp.managedReferenceValue = newCopy;
				localProp.serializedObject.ApplyModifiedProperties();
			});
		} else {
			menu.AddDisabledItem(new GUIContent("Paste value"));
		}
	}
}