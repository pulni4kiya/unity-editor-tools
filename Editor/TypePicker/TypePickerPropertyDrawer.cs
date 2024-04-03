using System;
using System.Linq;
using UnityEngine;
using UnityEditor;

namespace Pulni.EditorTools.Editor {
	[CustomPropertyDrawer(typeof(TypePickerAttribute))]
	public class TypePickerPropertyDrawer : PropertyDrawer {
		private static object[] typesProvider0Args = new object[0];
		private static object[] typesProvider1Arg = new object[1];
		private TypePickerAttribute Attribute => (TypePickerAttribute)attribute;

		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
			if (property.isArray) {
				for (int i = 0; i < property.arraySize; i++) {
					var itemProperty = property.GetArrayElementAtIndex(i);
					var itemPos = position;
					OnItemGUI(itemPos, itemProperty, label);
				}
			} else {
				OnItemGUI(position, property, label);
			}
		}

		private void OnItemGUI(Rect position, SerializedProperty property, GUIContent label) {
			var subtypes = GetAvailableTypes(property);
			var currentType = TypePickerHelper.GetActualType(property.managedReferenceFullTypename);
			var index = Array.IndexOf(subtypes.subtypes, currentType);

			if (index < 0) {
				if (subtypes.subtypes.Length == 0) {
					EditorGUI.LabelField(position, label, new GUIContent("No valid types found."));
					return;
				}
				SetReferenceValue(property, subtypes.subtypes[0]);
			}

			if (property.isExpanded) {
				var labelCopy = new GUIContent(label);


				var typePickerPosition = position;
				typePickerPosition.height = EditorGUIUtility.singleLineHeight;
				var newIndex = EditorGUI.Popup(typePickerPosition, " ", index, subtypes.displayNames);

				if (newIndex != index) {
					SetReferenceValue(property, subtypes.subtypes[newIndex]);
				}

				EditorGUI.PropertyField(position, property, labelCopy, true);
			} else {
				property.isExpanded = EditorGUI.Foldout(position, property.isExpanded, label);
			}
		}

		public override float GetPropertyHeight(SerializedProperty property, GUIContent label) {
			return EditorGUI.GetPropertyHeight(property, label);
		}

		private TypePickerOptions GetAvailableTypes(SerializedProperty property) {
			var options = GetCustomAvailableTypes(property);

			if (options == null) {
				options = TypePickerHelper.GetAvailableTypes(property.managedReferenceFieldTypename);
			}

			if (this.Attribute.AllowNull && !options.subtypes.Contains(null)) {
				options = InsertNullOption(options);
			}

			return options;
		}

		private TypePickerOptions GetCustomAvailableTypes(SerializedProperty property) {
			if (string.IsNullOrEmpty(this.Attribute.TypesGetterMethodName)) return null;

			try {
				var container = EditorHelper.GetContainingObject(property.serializedObject, property);
				if (container == null) {
					Debug.LogError($"[TypePicker] Couldn't resolve object containing property {property.propertyPath}.");
					return null;
				}

				var method = EditorHelper.GetMethodOnObject(container, this.Attribute.TypesGetterMethodName);
				if (method == null) {
					Debug.LogError($"[TypePicker] Couldn't resolve method \"{ this.Attribute.TypesGetterMethodName}\" on an object of type \"{container.GetType().Name}\"!");
					return null;
				}

				if (method.GetParameters().Length > 0)
                {
					typesProvider1Arg[0] = property;
					return (TypePickerOptions)method.Invoke(container, typesProvider1Arg);
				}
				else
				{
					return (TypePickerOptions)method.Invoke(container, typesProvider0Args);
				}

			} catch (Exception ex) {
				Debug.LogException(ex);
			}
			return null;
		}

		private TypePickerOptions InsertNullOption(TypePickerOptions options) {
			var optionsWithNull = new TypePickerOptions();

			optionsWithNull.subtypes = new Type[options.subtypes.Length + 1];
			Array.Copy(options.subtypes, 0, optionsWithNull.subtypes, 1, options.subtypes.Length);
			optionsWithNull.subtypes[0] = null;

			optionsWithNull.displayNames = new string[options.displayNames.Length + 1];
			Array.Copy(options.displayNames, 0, optionsWithNull.displayNames, 1, options.displayNames.Length);
			optionsWithNull.displayNames[0] = "<null>";

			return optionsWithNull;
		}

		private void SetReferenceValue(SerializedProperty property, Type type) {
			foreach (var obj in property.serializedObject.targetObjects) {
				var serializedObj = new SerializedObject(obj);
				var prop = serializedObj.FindProperty(property.propertyPath);
				ClearOldManagedReference(prop);
				prop.managedReferenceValue = type != null ? Activator.CreateInstance(type) : null;
				serializedObj.ApplyModifiedProperties();
			}
		}

		/// <summary>
		/// Recursively clears a managed reference and its child managed references stored as prefab overrides.
		/// If not done this way, the reference data is not destroyed, which leads to leaks and potentially errors.
		/// </summary>
		private static void ClearOldManagedReference(SerializedProperty property) {
			if (!property.isInstantiatedPrefab) return;


			var modifications = PrefabUtility
				.GetPropertyModifications(property.serializedObject.targetObject)
				?.ToList();

			if (modifications == null || modifications.Count == 0) return;

			var traversalProperty = property.Copy();
			do {
				if (traversalProperty.propertyType != SerializedPropertyType.ManagedReference) continue;

				var managedReferenceString = $"managedReferences[{traversalProperty.managedReferenceId}]";
				modifications.RemoveAll(mod => mod.propertyPath.StartsWith(managedReferenceString));
			}
			while (traversalProperty.Next(true) && traversalProperty.propertyPath.StartsWith(property.propertyPath));

			PrefabUtility.SetPropertyModifications(property.serializedObject.targetObject, modifications.ToArray());
		}
	}
}
