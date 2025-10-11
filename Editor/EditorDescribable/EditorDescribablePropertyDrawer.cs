using UnityEditor;
using UnityEngine;

namespace Pulni.EditorTools.Editor {
    [CustomPropertyDrawer(typeof(IEditorDescribable), true)]
    public class EditorDescribablePropertyDrawer : PropertyDrawer {
        public static float DescriptionWidthTaken = 0f;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
            EditorGUI.BeginProperty(position, label, property);

            // Get the IListDescribable object
            var describable = property.boxedValue as IEditorDescribable;

            // Display the description text
            string description = describable?.GetDescription() ?? "N/A";
            if (!property.IsPartOfArray()) {
                description = $"{label.text}: {description}";
            }
            var labelRect = new Rect(position.x, position.y, position.width - DescriptionWidthTaken, EditorGUIUtility.singleLineHeight);
            EditorGUI.LabelField(labelRect, description, EditorStyles.label);

            EditorGUI.PropertyField(position, property, new GUIContent(""), true);

            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label) {
            return EditorGUI.GetPropertyHeight(property);
        }
    }
}
