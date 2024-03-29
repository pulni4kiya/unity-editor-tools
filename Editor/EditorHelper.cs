using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace Pulni.EditorTools.Editor {
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

		public static MethodInfo GetMethodOnObject(object container, string typesGetterMethodName) {
			var type = container.GetType();
			if (!methodsCache.TryGetValue((type, typesGetterMethodName), out var result)) {
				var currentType = type;
				while (currentType != null) {
					result = currentType.GetMethod(typesGetterMethodName, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
					if (result != null) {
						break;
					}
					currentType = currentType.BaseType;
				}
				methodsCache[(type, typesGetterMethodName)] = result;
			}
			return result;
		}
	}
}
