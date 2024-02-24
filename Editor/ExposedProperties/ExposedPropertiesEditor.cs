using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Pulni.EditorTools.Editor {
	[CustomEditor(typeof(ExposedProperties))]
	public class ExposedPropertiesEditor : UnityEditor.Editor {
		private bool isInEditMode = false;

		public override void OnInspectorGUI() {
			if (this.isInEditMode) {
				DrawDefaultInspector();
				var stopEditing = GUILayout.Button("Finish editing settings");
				if (stopEditing) {
					this.isInEditMode = false;
				}
			} else {
				var startEditing = GUILayout.Button("Edit exposed properties settings");
				if (startEditing) {
					this.isInEditMode = true;
				}
			}

			var exposedProperties = (ExposedProperties)this.target;
			var config = exposedProperties.ExposedPropertiesConfig;

			foreach (var param in config.Properties) {
				if (param.Target == null) continue;

				var obj = new SerializedObject(param.Target);
				var prop = obj.FindProperty(param.PropertyPath);

				if (prop == null) continue;

				if (param.IsReadOnly || config.IsReadOnly) {
					GUI.enabled = false;
				}

				if (string.IsNullOrEmpty(param.Label)) {
					EditorGUILayout.PropertyField(prop, param.ShowChildren);
				} else {
					EditorGUILayout.PropertyField(prop, new GUIContent(param.Label), param.ShowChildren);
				}

				if (param.IsReadOnly || config.IsReadOnly) {
					GUI.enabled = true;
				}

				obj.ApplyModifiedProperties();
			}
		}
	}
}
