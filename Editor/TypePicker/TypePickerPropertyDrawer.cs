using System;
using System.Linq;
using UnityEngine;
using UnityEditor;

namespace Pulni.EditorTools.Editor {
	[CustomPropertyDrawer(typeof(TypePickerAttribute))]
	public class TypePickerPropertyDrawer : PropertyDrawer {
		private static object[] typesProviderArgs = new object[0];
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

			if (string.IsNullOrEmpty(property.managedReferenceFullTypename) || index < 0) {
				if (subtypes.subtypes.Length == 0) {
					EditorGUI.LabelField(position, label, new GUIContent("No valid types found."));
					return;
				}
				SetReferenceValue(property, () => Activator.CreateInstance(subtypes.subtypes[0]));
			}

			if (property.isExpanded) {
				var labelCopy = new GUIContent(label);


				var typePickerPosition = position;
				typePickerPosition.height = EditorGUIUtility.singleLineHeight;
				var newIndex = EditorGUI.Popup(typePickerPosition, " ", index, subtypes.displayNames);

				if (newIndex != index) {
					SetReferenceValue(property, () => Activator.CreateInstance(subtypes.subtypes[newIndex]));
				}

				EditorGUI.PropertyField(position, property, labelCopy, true);
			} else {
				property.isExpanded = EditorGUI.Foldout(position, property.isExpanded, label);
			}
		}

		public override float GetPropertyHeight(SerializedProperty property, GUIContent label) {
			return EditorGUI.GetPropertyHeight(property, label);
		}

		public TypePickerOptions GetAvailableTypes(SerializedProperty property) {
			if (string.IsNullOrEmpty(this.Attribute.TypesGetterMethodName)) {
				return TypePickerHelper.GetAvailableTypes(property.managedReferenceFieldTypename);
			} else {
				try {
					var container = EditorHelper.GetContainingObject(property.serializedObject, property);
					if (container == null) {
						return new TypePickerOptions() {
							displayNames = new string[0],
							subtypes = new Type[0]
						};
					}
					var method = EditorHelper.GetGetterMethod(container, this.Attribute.TypesGetterMethodName);
					return (TypePickerOptions)method.Invoke(container, typesProviderArgs);
				}
				catch (Exception ex) {
					Debug.LogException(ex);
					return TypePickerHelper.GetAvailableTypes(property.managedReferenceFieldTypename);
				}
			}
		}

		private void SetReferenceValue(SerializedProperty property, Func<object> valueProvider) {
			foreach (var obj in property.serializedObject.targetObjects) {
				var serializedObj = new SerializedObject(obj);
				var prop = serializedObj.FindProperty(property.propertyPath);
				ClearOldManagedReference(prop);
				prop.managedReferenceValue = valueProvider();
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
