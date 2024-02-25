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

			this.rootNode = new GroupNode("");
			foreach (var property in config.Properties) {
				this.rootNode.AddProperty(property);
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

			public void AddProperty(ExposedProperties.Param property) {
				this.AddProperty(property, property.Label.Split('/'), 0);
			}

			private void AddProperty(ExposedProperties.Param property, string[] path, int indexInPath) {
				if (indexInPath == path.Length - 1) {
					this.subnodes.Add(new PropertyNode(property, path[indexInPath]));
				} else {
					var group = subnodes.OfType<GroupNode>().Where(grp => grp.Name == path[indexInPath]).FirstOrDefault();
					if (group == null) {
						group = new GroupNode(path[indexInPath]);
						this.subnodes.Add(group);
					}
					group.AddProperty(property, path, indexInPath + 1);
				}
			}
		}

		private class PropertyNode : INode {
			private ExposedProperties.Param property;
			private string label;

			public PropertyNode(ExposedProperties.Param property, string label) {
				this.property = property;
				this.label = label;
			}

			public void ShowNode() {
				if (this.property.Target == null) return;

				var obj = new SerializedObject(this.property.Target);
				var prop = obj.FindProperty(this.property.PropertyPath);

				if (prop == null) return;

				if (this.property.IsReadOnly || this.property.IsReadOnly) {
					GUI.enabled = false;
				}

				if (string.IsNullOrEmpty(this.property.Label)) {
					EditorGUILayout.PropertyField(prop, this.property.ShowChildren);
				} else {
					EditorGUILayout.PropertyField(prop, new GUIContent(this.label), this.property.ShowChildren);
				}

				if (this.property.IsReadOnly || this.property.IsReadOnly) {
					GUI.enabled = true;
				}

				obj.ApplyModifiedProperties();
			}
		}
	}
}
