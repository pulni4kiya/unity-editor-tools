using UnityEngine;
using UnityEditor;
using System.Reflection;
using System;

namespace Pulni.EditorTools.Editor {
    internal static class EditorExtensionMethods {
        public static bool IsPartOfArray(this SerializedProperty property) {
            // Definitely not a hack because the API lacks such method
            return property.propertyPath.EndsWith(']');
        }

        public static T GetOrAddComponent<T>(this GameObject go, bool searchChildren = false) where T : Component {
            var component = searchChildren == true ? go.GetComponentInChildren<T>() : go.GetComponent<T>();
            if (component == null) {
                component = go.AddComponent<T>();
            }
            return component;
        }

        public static FieldInfo GetFieldInParents(this Type type, string fieldName, BindingFlags bindingFlags) {
            var fi = type.GetField(fieldName, bindingFlags);
            if (fi != null) return fi;
            if (type.BaseType != null) return GetFieldInParents(type.BaseType, fieldName, bindingFlags);
            return null;
        }
    }
}
