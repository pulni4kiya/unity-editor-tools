using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace Pulni.EditorTools {
	public static class EditorHelper {
		private static Dictionary<(Type, string), MethodInfo> methodsCache = new Dictionary<(Type, string), MethodInfo>();

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

		public static MethodInfo GetGetterMethod(object container, string typesGetterMethodName) {
			var type = container.GetType();
			if (!methodsCache.TryGetValue((type, typesGetterMethodName), out var result)) {
				result = type.GetMethod(typesGetterMethodName, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.DeclaredOnly);
				methodsCache[(type, typesGetterMethodName)] = result;
			}
			return result;
		}
	}
}
