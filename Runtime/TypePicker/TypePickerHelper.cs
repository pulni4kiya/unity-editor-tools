using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace Pulni.EditorTools {
	public static class TypePickerHelper {
		private static Dictionary<Type, TypePickerOptions> subtypesCache = new Dictionary<Type, TypePickerOptions>();
		private static Dictionary<string, Type> typesCache = new Dictionary<string, Type>();

		/// <summary>
		/// Retrieves a list of types that can be assigned to a property of the given type.
		/// </summary>
		/// <param name="propertyTypeName">The type name as written in SerializedProperty.managedReferenceFieldTypename</param>
		/// <returns>The available types or null if called in build</returns>
		public static TypePickerOptions GetAvailableTypes(string propertyTypeName) {
#if UNITY_EDITOR
			var type = GetActualType(propertyTypeName);
			return GetAvailableTypes(type);
#else
			return null;
#endif
		}

		/// <summary>
		/// Retrieves a list of types that can be assigned to a property of the given type.
		/// </summary>
		/// <param name="propertyTypeName">The type name as written in SerializedProperty.managedReferenceFieldTypename</param>
		/// <returns>The available types or null if called in build</returns>
		public static TypePickerOptions GetAvailableTypes(Type type) {
#if UNITY_EDITOR
			TypePickerOptions result;
			if (subtypesCache.TryGetValue(type, out result) == false) {
				result = new TypePickerOptions();

				result.subtypes = UnityEditor.TypeCache.GetTypesDerivedFrom(type)
					.Prepend(type)
					.Where(t => !t.IsAbstract && !t.IsInterface)
					.Where(t => typeof(UnityEngine.Object).IsAssignableFrom(t) == false)
					.Where(t => !t.ContainsGenericParameters && !t.IsGenericType)
					.OrderBy(GetTypeOrder)
					.ThenBy(GetTypeName)
					.ToArray();

				result.FillNamesFromTypes();
				subtypesCache[type] = result;
			}
			return result;
#else
			return null;
#endif
		}

		/// <summary>
		/// Finds a Type by its name.
		/// </summary>
		/// <param name="propertyTypeName">The type name as written in SerializedProperty.managedReferenceFieldTypename</param>
		public static Type GetActualType(string propertyTypeName) {
			if (string.IsNullOrEmpty(propertyTypeName)) return null;

			Type result;
			if (typesCache.TryGetValue(propertyTypeName, out result) == false) {
				var parts = propertyTypeName.Split(' ', 2);
				var assembly = Assembly.Load(parts[0]);
				result = assembly.GetType(parts[1]);
				typesCache[propertyTypeName] = result;
			}

			return result;
		}

		/// <summary>
		/// Retrieves an int used for ordering the options within the TypePicker's popup
		/// from the TypePickerInfoAttribute attribute applied on the given type.
		/// </summary>
		/// <returns>The order value specified through TypePickerInfoAttribute  or 0 if no attribute</returns>
		public static int GetTypeOrder(Type type) {
			var attr = type?.GetCustomAttribute<TypePickerInfoAttribute>();
			return attr?.Order ?? 0;
		}

		/// <summary>
		/// Retrieves the type name used to build the hierarchy of options within the TypePicker's popup
		/// from the TypePickerInfoAttribute attribute applied on the given type.
		/// </summary>
		/// <returns>The name specified through TypePickerInfoAttribute or the type's name if no attribute.</returns>
		public static string GetTypeName(Type type) {
			if (type == null) return "<null>";
			var attr = type.GetCustomAttribute<TypePickerInfoAttribute>();
			return attr != null && attr.Name != null ? attr.Name : type.Name;
		}

		/// <summary>
		/// Retrieves the type names used to build the hierarchy of options within the TypePicker's popup
		/// from the TypePickerInfoAttribute attribute applied on the given types.
		/// </summary>
		/// <returns>The names specified through TypePickerInfoAttribute or the types' names for those without the attribute.</returns>
		public static IEnumerable<string> GetTypeNames(IEnumerable<Type> types) {
			return types.Select(GetTypeName);
		}

		/// <summary>
		/// Resolve the names of the types and fill them in the displayNames array.
		/// </summary>
		public static void FillNamesFromTypes(this TypePickerOptions info) {
			info.displayNames = GetTypeNames(info.subtypes).ToArray();
		}
	}
}
