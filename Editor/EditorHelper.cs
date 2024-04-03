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

			return GetTargetObject(parentProperty);
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

		public static object GetTargetObject(SerializedProperty property) {
			var path = property.propertyPath.Replace(".Array.data[", ".[");
			object obj = property.serializedObject.targetObject;
			var elements = path.Split('.');

			foreach (var element in elements) {
				if (element.StartsWith("[")) {
					var index = Int32.Parse(element.Substring(1, element.Length - 2));
					obj = GetValueAtIndex(obj, index);
				} else {
					obj = GetValue(obj, element);
				}
			}
			return obj;
		}

		private static object GetValue(object source, string name) {
			var type = source.GetType();

			var currentType = type;
			while (currentType != null) {
				var f = currentType.GetField(name, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
				if (f != null) {
					return f.GetValue(source);
				}

				var p = currentType.GetProperty(name, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
				if (p != null) {
					return p.GetValue(source, null);
				}

				currentType = currentType.BaseType;
			}

			throw new InvalidOperationException($"Could not retrieve field or property {name} from object of type {type.FullName}");
		}

		private static object GetValueAtIndex(object source, int index)
		{
			var list = source as IList;
			if (list != null) return list[index];

			var enumerable = source as IEnumerable;
			if (enumerable != null) {
				var current = 0;
				foreach (var item in enumerable)
				{
					if (current == index) return item;
					current++;
				}
			}

			throw new InvalidOperationException($"Could not retrieve object at index {index}");
		}
	}
}
