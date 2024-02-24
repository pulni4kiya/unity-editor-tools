using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Pulni.EditorTools.Editor {
	public static class EditorHelper {
		public static object GetContainingObject(SerializedObject source, SerializedProperty property) {
			var parentProperty = property;
			do {
				var lastDotIndex = parentProperty.propertyPath.LastIndexOf('.');
				if (lastDotIndex == -1) {
					return source.targetObject;
				}
				var parentPath = parentProperty.propertyPath.Substring(0, lastDotIndex);
				parentProperty = source.FindProperty(parentPath);
			} while (parentProperty.isArray);

			return parentProperty.managedReferenceValue;
		}
	}
}
