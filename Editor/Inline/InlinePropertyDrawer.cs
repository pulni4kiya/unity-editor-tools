using System.Collections;
using System.Collections.Generic;
using Pulni.EditorTools.Attributes;
using UnityEditor;
using UnityEngine;

namespace Pulni.EditorTools {
	[CustomPropertyDrawer(typeof(InlineAttribute))]
	public class InlinePropertyDrawer : PropertyDrawer {
		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
			var attr = (InlineAttribute)this.attribute;
			var labelText = label.text;

			// Get first child property
			SerializedProperty iterator = property.Copy();
			var hasNext = iterator.Next(true);

			// If no child properties, show error.
			if (!hasNext || iterator.depth <= property.depth) {
				Debug.LogError($"[{nameof(InlinePropertyDrawer)}] Trying to inline property '{property.propertyPath}', which has no child properties!");
				return;
			}

			do {
				// Draw each child property
				position.height = EditorGUI.GetPropertyHeight(iterator, true);
				if (attr.PrependNameToInlinedFields) {
					var childLabel = $"{labelText}.{iterator.displayName}";
					EditorGUI.PropertyField(position, iterator, new GUIContent(childLabel), true);
				} else {
					EditorGUI.PropertyField(position, iterator, true);
				}
				position.y += position.height + EditorGUIUtility.standardVerticalSpacing;
			} while (iterator.NextVisible(false) && iterator.depth > property.depth);
		}

		public override float GetPropertyHeight(SerializedProperty property, GUIContent label) {
			property.isExpanded = true;
			var height = EditorGUI.GetPropertyHeight(property, label);
			height -= EditorGUIUtility.singleLineHeight;
			height -= EditorGUIUtility.standardVerticalSpacing;
			return height;
		}
	}
}
