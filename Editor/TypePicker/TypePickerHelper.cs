using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Pulni.Attributes;
using UnityEditor;
using UnityEngine;

namespace Pulni.EditorTools {
	public static class TypePickerHelper {
		private static Dictionary<string, TypePickerOptions> subtypesCache = new Dictionary<string, TypePickerOptions>();
		private static Dictionary<string, Type> typesCache = new Dictionary<string, Type>();
		private static Dictionary<(Type, string), MethodInfo> methodsCache = new Dictionary<(Type, string), MethodInfo>();


		/// <summary>
		/// Retrieves a list of available types that are valid for the property.
		/// </summary>
		/// <param name="propertyTypeName">The property name as written in SerializedProperty.managedReferenceFieldTypename</param>
		public static TypePickerOptions GetAvailableTypes(string propertyTypeName) {
			TypePickerOptions result;
			if (subtypesCache.TryGetValue(propertyTypeName, out result) == false) {
				var type = GetActualType(propertyTypeName);
				result = new TypePickerOptions();

				result.subtypes = TypeCache.GetTypesDerivedFrom(type)
					.Prepend(type)
					.Where(t => !t.IsAbstract && !t.IsInterface)
					.Where(t => typeof(UnityEngine.Object).IsAssignableFrom(t) == false)
					.Where(t => !t.ContainsGenericParameters && !t.IsGenericType)
					.OrderBy(GetTypeOrder)
					.ToArray();

				result.FillNamesFromTypes();
				subtypesCache[propertyTypeName] = result;
			}
			return result;
		}

		/// <summary>
		/// Retrieves a Type object based on the string in SerializedProperty.managedReferenceFieldTypename
		/// </summary>
		/// <param name="propertyTypeName">The property name as written in SerializedProperty.managedReferenceFieldTypename</param>
		public static Type GetActualType(string propertyTypeName) {
			Type result;
			if (typesCache.TryGetValue(propertyTypeName, out result) == false) {
				var words = propertyTypeName.Split(' ');
				result = AppDomain.CurrentDomain
					.GetAssemblies()
					.SelectMany(ass => ass.GetTypes())
					.SingleOrDefault(t => t.FullName == words[words.Length - 1] && t.Assembly.GetName().Name == words[0]);
				typesCache[propertyTypeName] = result;
			}

			return result;
		}

		/// <summary>
		/// Retrieves an int used for ordering the options within the TypePicker's popup
		/// from the TypePickerInfoAttribute attribute applied on the given type.
		/// </summary>
		/// <returns>The order value for the type (or 0 if not specified)</returns>
		public static int GetTypeOrder(Type type) {
			var attr = type.GetCustomAttribute<TypePickerInfoAttribute>();
			return attr?.Order ?? 0;
		}

		/// <summary>
		/// Retrieves a "path" used to build the hierarchy of options within the TypePicker's popup
		/// from the TypePickerInfoAttribute attribute applied on the given type.
		/// </summary>
		/// <returns>The order value for the type (or 0 if not specified)</returns>
		public static string GetTypeName(Type type) {
			var attr = type.GetCustomAttribute<TypePickerInfoAttribute>();
			return attr != null ? attr.Name : type.Name;
		}

		public static IEnumerable<string> GetTypeNames(IEnumerable<Type> types) {
			return types.Select(GetTypeName);
		}

		public static void FillNamesFromTypes(this TypePickerOptions info) {
			info.displayNames = GetTypeNames(info.subtypes).ToArray();
		}

		internal static MethodInfo GetGetterMethod(object container, string typesGetterMethodName) {
			var type = container.GetType();
			if (!methodsCache.TryGetValue((type, typesGetterMethodName), out var result)) {
				result = type.GetMethod(typesGetterMethodName, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.DeclaredOnly);
				methodsCache[(type, typesGetterMethodName)] = result;
			}
			return result;
		}
	}
}
