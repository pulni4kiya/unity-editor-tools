using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Pulni.EditorTools.Editor {
	[CustomEditor(typeof(ExposedProperties))]
	public class ExposedPropertiesEditor : UnityEditor.Editor {
		private bool isInEditMode = false;

		private GroupNode rootNode;

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

			var hasManualProperty = ExposedPropertiesMenu.ManualProeprty != null;
			if (!hasManualProperty) {
				GUI.enabled = false;
			}
			var addManualProperty = GUILayout.Button("Add manually copied property");
			if (addManualProperty) {
				var copy = JsonUtility.FromJson<ExposedProperties.Param>(JsonUtility.ToJson(ExposedPropertiesMenu.ManualProeprty));
				config.Properties.Add(copy);
			}
			if (!hasManualProperty) {
				GUI.enabled = true;
			}

			this.rootNode = new GroupNode("");
			foreach (var property in config.Properties) {
				this.rootNode.AddProperty(property, config);
			}

			this.rootNode.ShowRootNodes();
		}

		private interface INode {
			public void ShowNode();
		}

		private class GroupNode : INode {
			private List<INode> subnodes = new List<INode>();
			private bool IsExpanded {
				get {
					return SessionState.GetBool($"{this.Name}_IsExpanded", false);
				}
				set {
					SessionState.SetBool($"{this.Name}_IsExpanded", value);
				}
			}

			public string Name { get; private set; }

			public GroupNode(string name) {
				this.Name = name;
			}

			public void ShowRootNodes() {
				ShowNode(true);
			}

			public void ShowNode() {
				ShowNode(false);
			}

			public void ShowNode(bool isRoot) {
				if (!isRoot) {
					this.IsExpanded = EditorGUILayout.Foldout(this.IsExpanded, this.Name);
					EditorGUI.indentLevel++;
				}
				if (isRoot || this.IsExpanded) {
					foreach (var node in subnodes) {
						node.ShowNode();
					}
				}
				if (!isRoot) {
					EditorGUI.indentLevel--;
				}
			}

			public void AddProperty(ExposedProperties.Param property, ExposedProperties.PropertiesConfig globalConfig) {
				this.AddProperty(property, property.Label.Split('/'), 0, globalConfig);
			}

			private void AddProperty(ExposedProperties.Param property, string[] path, int indexInPath, ExposedProperties.PropertiesConfig globalConfig) {
				if (indexInPath == path.Length - 1) {
					this.subnodes.Add(new PropertyNode(property, path[indexInPath], globalConfig));
				} else {
					var group = subnodes.OfType<GroupNode>().Where(grp => grp.Name == path[indexInPath]).FirstOrDefault();
					if (group == null) {
						group = new GroupNode(path[indexInPath]);
						this.subnodes.Add(group);
					}
					group.AddProperty(property, path, indexInPath + 1, globalConfig);
				}
			}
		}

		private class PropertyNode : INode {
			private ExposedProperties.Param property;
			private string label;
			private ExposedProperties.PropertiesConfig globalConfig;

			public PropertyNode(ExposedProperties.Param property, string label, ExposedProperties.PropertiesConfig globalConfig) {
				this.property = property;
				this.label = label;
				this.globalConfig = globalConfig;
			}

			public void ShowNode() {
				if (this.property.Target == null) return;

				var obj = new SerializedObject(this.property.Target);
				var prop = obj.FindProperty(this.property.PropertyPath);

				if (prop == null) return;

				if (this.globalConfig.IsReadOnly || this.property.IsReadOnly) {
					GUI.enabled = false;
				}

				if (string.IsNullOrEmpty(this.property.Label)) {
					EditorGUILayout.PropertyField(prop, this.property.ShowChildren);
				} else {
					EditorGUILayout.PropertyField(prop, new GUIContent(this.label), this.property.ShowChildren);
				}

				if (this.globalConfig.IsReadOnly || this.property.IsReadOnly) {
					GUI.enabled = true;
				}

				obj.ApplyModifiedProperties();
			}
		}
	}
}
