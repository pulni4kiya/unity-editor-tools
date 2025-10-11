using System;
using System.Linq;
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

namespace Pulni.EditorTools.Editor {
	[CustomPropertyDrawer(typeof(TypePickerAttribute))]
	public class TypePickerPropertyDrawer : PropertyDrawer {
		private static object[] typesProvider0Args = new object[0];
		private static object[] typesProvider1Arg = new object[1];
		private static List<TypePickerMenuOption> menuOptions = new();
		private TypePickerAttribute Attribute => (TypePickerAttribute)attribute;

		public static void AddMenuOption(string name, Action<SerializedProperty> invokeAction) {
			menuOptions.Add(new TypePickerMenuOption { Name = name, InvokeAction = invokeAction });
		}

		public static void RemoveMenuOption(string name) {
			menuOptions.RemoveAll(option => option.Name == name);
		}

		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
			if (property.propertyType != SerializedPropertyType.ManagedReference) {
				this.DrawPropertyField(position, property, label, 0f);
				Debug.LogError("TypePicker is only supported on managed reference properties.");
				return;
			}

			var currentType = TypePickerHelper.GetActualType(property.managedReferenceFullTypename);
			if (currentType == null) {
				var subtypes = GetAvailableTypes(property);
				if (subtypes.subtypes.Length == 0) {
					EditorGUI.LabelField(position, label, new GUIContent("No valid types found."));
					return;
				}
				SetReferenceValue(property, subtypes.subtypes[0]);
			}

			var labelCopy = new GUIContent(label);
			if (this.Attribute.DrawMode == TypePickerAttribute.TypePickerDrawMode.Compact) {
				this.DrawCompactPicker(position, property, labelCopy);
			} else if (this.Attribute.DrawMode == TypePickerAttribute.TypePickerDrawMode.Extended) {
				this.DrawStandardPicker(position, property, labelCopy);
			} else {
				if (property.boxedValue is ITypePickerExtended) {
					this.DrawStandardPicker(position, property, labelCopy);
				} else {
					this.DrawCompactPicker(position, property, labelCopy);
				}
			}
		}

		private void DrawStandardPicker(Rect position, SerializedProperty property, GUIContent label) {
			var subtypes = GetAvailableTypes(property);
			var currentType = TypePickerHelper.GetActualType(property.managedReferenceFullTypename);
			var index = Array.IndexOf(subtypes.subtypes, currentType);

			var typePickerPosition = position;
			typePickerPosition.height = EditorGUIUtility.singleLineHeight;
			typePickerPosition.width -= 30;
			var newIndex = EditorGUI.Popup(typePickerPosition, " ", index, subtypes.displayNames);

			var menuButtonRect = position;
			menuButtonRect.x = typePickerPosition.xMax;
			menuButtonRect.width = 30;
			menuButtonRect.height = EditorGUIUtility.singleLineHeight;

			if (newIndex != index) {
				SetReferenceValue(property, subtypes.subtypes[newIndex]);
			}

			if (GUI.Button(menuButtonRect, "...")) {
				var menu = new GenericMenu();
				foreach (var option in menuOptions) {
					menu.AddItem(new GUIContent(option.Name), false, () => option.InvokeAction(property));
				}
				menu.DropDown(menuButtonRect);
			}

			DrawPropertyField(position, property, label, position.width - EditorGUIUtility.labelWidth);
		}

		private void DrawCompactPicker(Rect position, SerializedProperty property, GUIContent label) {
			var buttonRect = position;
			buttonRect.width = 20;
			buttonRect.height = EditorGUIUtility.singleLineHeight;
			buttonRect.x = position.xMax - buttonRect.width;

			if (GUI.Button(buttonRect, $"⁕")) {
				var menu = new GenericMenu();

				// Add type picker options
				var subtypes = GetAvailableTypes(property);
				var currentType = TypePickerHelper.GetActualType(property.managedReferenceFullTypename);
				for (int i = 0; i < subtypes.subtypes.Length; i++) {
					var type = subtypes.subtypes[i];
					var displayName = subtypes.displayNames[i];
					var isSelected = type == currentType;

					menu.AddItem(new GUIContent(displayName), isSelected, () => {
						SetReferenceValue(property, type);
					});
				}

				// Add menu options
				if (menuOptions.Count > 0) {
					menu.AddSeparator("");

					foreach (var option in menuOptions) {
						menu.AddItem(new GUIContent(option.Name), false, () => option.InvokeAction(property));
					}
				}

				menu.DropDown(buttonRect);
			}
			position.width -= 10f;
			DrawPropertyField(position, property, label, buttonRect.width);
		}


		public override float GetPropertyHeight(SerializedProperty property, GUIContent label) {
			return EditorGUI.GetPropertyHeight(property, label);
		}

		private void DrawPropertyField(Rect position, SerializedProperty property, GUIContent label, float pickerWidth) {
			// Hack: This is a hack to properly draw the description text, if the property is drawn with that editor
			var oldWidth = EditorDescribablePropertyDrawer.DescriptionWidthTaken;
			EditorDescribablePropertyDrawer.DescriptionWidthTaken = pickerWidth;
			EditorGUI.PropertyField(position, property, label, true);
			EditorDescribablePropertyDrawer.DescriptionWidthTaken = oldWidth;
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
					Debug.LogError($"[TypePicker] Couldn't resolve method \"{this.Attribute.TypesGetterMethodName}\" on an object of type \"{container.GetType().Name}\"!");
					return null;
				}

				if (method.GetParameters().Length > 0) {
					typesProvider1Arg[0] = property;
					return (TypePickerOptions)method.Invoke(container, typesProvider1Arg);
				} else {
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

	public class TypePickerMenuOption {
		public string Name { get; set; }
		public Action<SerializedProperty> InvokeAction { get; set; }
	}
}
