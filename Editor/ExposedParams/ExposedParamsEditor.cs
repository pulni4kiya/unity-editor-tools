using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Pulni.EditorTools {
	[CustomEditor(typeof(ExposedParams))]
	public class ExposedParamsEditor : Editor {
		private bool isInEditMode = false;

		public override void OnInspectorGUI() {
			if (this.isInEditMode) {
				DrawDefaultInspector();
				var stopEditing = GUILayout.Button("Finish editing settings");
				if (stopEditing) {
					this.isInEditMode = false;
				}
			} else {
				var startEditing = GUILayout.Button("Edit exposed params settings");
				if (startEditing) {
					this.isInEditMode = true;
				}
			}

			var exposedParams = (ExposedParams)this.target;
			var config = exposedParams.ExposedParamsConfig;

			foreach (var param in config.Params) {
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
