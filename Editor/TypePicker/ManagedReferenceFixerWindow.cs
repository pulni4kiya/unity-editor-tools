using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Pulni.EditorTools {
	public class ManagedReferenceFixerWindow : EditorWindow {
		private const string ManagedReferencePath = "managedReferences";

		private Dictionary<GameObject, ManagedReferenceError> _errors = new Dictionary<GameObject, ManagedReferenceError>();

		private Vector2 _scroll;

		[MenuItem("Window/Pulni/Serialized Reference Fixer")]
		public static void ShowWindow() {
			//Show existing window instance. If one doesn't exist, make one.
			EditorWindow.GetWindow(typeof(ManagedReferenceFixerWindow));
		}

		private void OnGUI() {
			ShowFindErrorButtons();
			ShowErrorsFound();
		}

		private void ShowFindErrorButtons() {
			GUILayout.Label("Find managed reference errors in:", EditorStyles.boldLabel);
			GUILayout.BeginHorizontal();
			var selectionClicked = GUILayout.Button("Selection");
			if (selectionClicked) {
				FindErrorsInSelection();
			}
			var activeSceneClicked = GUILayout.Button("Active Scene");
			if (activeSceneClicked) {
				FindErrorsInActiveScene();
			}
			var allPrefabsClicked = GUILayout.Button("All Prefabs");
			if (allPrefabsClicked) {
				FindErrorsInAllPrefabs();
			}
			GUILayout.EndHorizontal();
		}

		private void ShowErrorsFound() {
			if (_errors == null || _errors.Count == 0) return;

			GUILayout.Label($"Errors found: {_errors.Count} / {_errors.Sum(err => err.Value.Modifications.Count)}", EditorStyles.boldLabel);

			var fixAllClicked = GUILayout.Button("Fix ALL");
			if (fixAllClicked) {
				foreach (var error in _errors.Values) {
					FixManyErrors(error.ObjectWithModifications, error.Modifications);
					_errors.Clear();
				}
			}

			_scroll = EditorGUILayout.BeginScrollView(_scroll);
			foreach (var kvp in _errors) {
				ShowError(kvp.Key, kvp.Value);
			}
			EditorGUILayout.EndScrollView();
		}

		private void ShowError(GameObject go, ManagedReferenceError error) {
			error.IsExpanded = EditorGUILayout.Foldout(error.IsExpanded, error.ObjectWithModifications.name);
			if (error.IsExpanded) {
				EditorGUI.indentLevel++;
				var fixAllClicked = GUILayout.Button("Fix All");
				if (fixAllClicked) {
					FixManyErrors(error.ObjectWithModifications, error.Modifications);
				}
				foreach (var mod in error.Modifications) {
					EditorGUILayout.BeginHorizontal();
					EditorGUILayout.LabelField(mod.propertyPath);
					GUI.enabled = false;
					EditorGUILayout.ObjectField(go, typeof(UnityEngine.Object), true, GUILayout.Width(200f));
					GUI.enabled = true;
					var fixClicked = GUILayout.Button("Fix", GUILayout.Width(70f));
					if (fixClicked) {
						FixError(error.ObjectWithModifications, mod);
					}
					EditorGUILayout.EndHorizontal();
				}
				EditorGUI.indentLevel--;
			}
		}

		private void FindErrorsInSelection() {
			var gameObjects = Selection.gameObjects
				.SelectMany(go => go.GetComponentsInChildren<Transform>(true).Select(tr => PrefabUtility.GetOutermostPrefabInstanceRoot(tr.gameObject)))
				.Distinct();
			_errors = FindErrorsInGameObjects(gameObjects);
		}

		private void FindErrorsInActiveScene() {
			var gameObjects = SceneManager
				.GetActiveScene()
				.GetRootGameObjects()
				.SelectMany(go => go.GetComponentsInChildren<Transform>(true).Select(tr => PrefabUtility.GetOutermostPrefabInstanceRoot(tr.gameObject)))
				.Distinct();
			_errors = FindErrorsInGameObjects(gameObjects);
		}
		private void FindErrorsInAllPrefabs() {
			var gameObjects = AssetDatabase.FindAssets("t:prefab")
				.Select(guid => AssetDatabase.GUIDToAssetPath(guid))
				.Select(path => AssetDatabase.LoadAssetAtPath<GameObject>(path))
				.SelectMany(go =>
					go.GetComponentsInChildren<Transform>(true)
					.Select(tr => PrefabUtility.GetOutermostPrefabInstanceRoot(tr.gameObject)))
				.Distinct();
			_errors = FindErrorsInGameObjects(gameObjects);
		}

		private void FixError(GameObject objectWithModifications, PropertyModification modification) {
			var mods = PrefabUtility.GetPropertyModifications(objectWithModifications)
				.Where(mod => mod.target != modification.target || mod.propertyPath != modification.propertyPath)
				.ToArray();
			PrefabUtility.SetPropertyModifications(objectWithModifications, mods);
			EditorUtility.SetDirty(objectWithModifications);
		}

		private void FixManyErrors(GameObject objectWithModifications, List<PropertyModification> modifications) {
			var mods = PrefabUtility.GetPropertyModifications(objectWithModifications)
				.Where(mod => !modifications.Any(modification => mod.target == modification.target && mod.propertyPath == modification.propertyPath))
				.ToArray();
			PrefabUtility.SetPropertyModifications(objectWithModifications, mods);
			EditorUtility.SetDirty(objectWithModifications);
		}

		private Dictionary<GameObject, ManagedReferenceError> FindErrorsInGameObjects(IEnumerable<GameObject> gameObjects) {
			var result = new Dictionary<GameObject, ManagedReferenceError>();

			foreach (var go in gameObjects) {
				var mods = PrefabUtility.GetPropertyModifications(go);
				if (mods == null) continue;
				foreach (var mod in mods) {
					if (!mod.propertyPath.Contains(ManagedReferencePath)) continue;

					var instanceObj = GetInstanceObject(go, mod.target);
					if (instanceObj == null) continue;

					var indexOfFirstDot = mod.propertyPath.IndexOf('.');
					if (indexOfFirstDot == -1) indexOfFirstDot = mod.propertyPath.Length;
					var managedRefId = long.Parse(mod.propertyPath.Substring(ManagedReferencePath.Length + 1, indexOfFirstDot - ManagedReferencePath.Length - 2));
					var so = new SerializedObject(instanceObj);
					var prop = so.GetIterator();
					var isValidProperty = false;

					while (prop.Next(true)) {
						if (prop.propertyType != SerializedPropertyType.ManagedReference || prop.managedReferenceId != managedRefId) continue;

						var actualProp = so.FindProperty(prop.propertyPath + mod.propertyPath.Substring(indexOfFirstDot));
						isValidProperty = actualProp != null;

						break;
					}

					if (isValidProperty) continue;

					if (!result.TryGetValue(go, out var error)) {
						error = new ManagedReferenceError() { ObjectWithModifications = go };
						result[go] = error;
					}
					error.Modifications.Add(mod);
				}
			}

			return result;
		}

		public static Object GetInstanceObject(GameObject instanceRoot, Object objectInPrefab) {
			var assetPath = AssetDatabase.GetAssetPath(objectInPrefab);
			var objectInPrefabPath = objectInPrefab.name;
			if (objectInPrefab is GameObject go) {
				objectInPrefabPath = GetScenePath(go);
				foreach (var tr in instanceRoot.GetComponentsInChildren<Transform>(true)) {
					if (PrefabUtility.GetCorrespondingObjectFromSourceAtPath(tr.gameObject, assetPath) == objectInPrefab) {
						return tr.gameObject;
					}
				}
			} else if (objectInPrefab is Component component) {
				objectInPrefabPath = GetScenePath(component.gameObject);
				foreach (var comp in instanceRoot.GetComponentsInChildren<Component>(true)) {
					if (PrefabUtility.GetCorrespondingObjectFromSourceAtPath(comp, assetPath) == objectInPrefab) {
						return comp;
					}
				}
			}
			Debug.LogError($"Corresponding instance not found! -- {GetScenePath(instanceRoot)} -- {objectInPrefabPath} -- {objectInPrefab.GetType().Name}", instanceRoot);
			return null;
		}

		private static string GetScenePath(GameObject go) {
			var path = go.name;
			return path;
		}

		private class ManagedReferenceError {
			public GameObject ObjectWithModifications;
			public List<PropertyModification> Modifications = new List<PropertyModification>();
			public bool IsExpanded = false;
		}
	}
}
