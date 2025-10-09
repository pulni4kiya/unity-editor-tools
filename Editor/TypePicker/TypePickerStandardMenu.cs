using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

namespace Pulni.EditorTools.Editor {
    public class TypePickerStandardMenu : MonoBehaviour {
        private const string GeneratedImplementationsFolder = "Assets/CodeGen/Implementations";
        private const string GeneratedImplementationsNamespace = "CodeGen.Implementations";
        private const string PendingImplementationKey = "TypePicker_PendingImplementation";

        [InitializeOnLoadMethod]
        private static void Initialize() {
            // Check for pending implementations after domain reload
            CheckForPendingImplementations();
            TypePickerPropertyDrawer.AddMenuOption("Go to Code", (property) => {
                GoToCode(property);
            });

            TypePickerPropertyDrawer.AddMenuOption("Create/New Implementation", (property) => {
                CreateNewImplementation(property);
            });

            TypePickerPropertyDrawer.AddMenuOption("Create/One-time Implementation", (property) => {
                CreateOneTimeImplementation(property);
            });
        }

        private static void GoToCode(SerializedProperty property) {
            var currentType = TypePickerHelper.GetActualType(property.managedReferenceFullTypename);
            if (currentType == null) {
                Debug.LogWarning("Cannot find type for current value.");
                return;
            }

            var scriptInfo = FindScriptForType(currentType);
            if (scriptInfo.script != null) {
                if (scriptInfo.lineNumber > 0) {
                    AssetDatabase.OpenAsset(scriptInfo.script, scriptInfo.lineNumber);
                } else {
                    AssetDatabase.OpenAsset(scriptInfo.script);
                }
            } else {
                Debug.LogWarning($"Could not find script file for type: {currentType.Name}");
            }
        }

        private static void CreateNewImplementation(SerializedProperty property) {
            var baseType = TypePickerHelper.GetActualType(property.managedReferenceFieldTypename);
            if (baseType == null) {
                Debug.LogWarning("Cannot determine base type for implementation.");
                return;
            }

            string suggestedName = "New" + baseType.Name;
            string path = EditorUtility.SaveFilePanel("Create New Implementation", "Assets", suggestedName, "cs");

            if (!string.IsNullOrEmpty(path)) {
                string className = Path.GetFileNameWithoutExtension(path);
                string directory = Path.GetDirectoryName(path);
                CreateDefaultImplementationFile(property, baseType, className, directory, true, false);
            }
        }

        private static void CreateOneTimeImplementation(SerializedProperty property) {
            var baseType = TypePickerHelper.GetActualType(property.managedReferenceFieldTypename);
            if (baseType == null) {
                Debug.LogWarning("Cannot determine base type for implementation.");
                return;
            }

            if (!Directory.Exists(GeneratedImplementationsFolder)) {
                Directory.CreateDirectory(GeneratedImplementationsFolder);
            }

            string className = SuggestImplementationName(property, baseType);
            string filePath = Path.Combine(GeneratedImplementationsFolder, className + ".cs");

            if (File.Exists(filePath)) {
                Debug.LogWarning($"Implementation already exists: {filePath}");
                return;
            }

            CreateDefaultImplementationFile(property, baseType, className, GeneratedImplementationsFolder, true, true);
        }

        private static void CheckForPendingImplementations() {
            string pendingData = EditorPrefs.GetString(PendingImplementationKey, "");
            if (string.IsNullOrEmpty(pendingData)) return;

            try {
                var data = JsonUtility.FromJson<PendingImplementationData>(pendingData);

                // Try to find the type
                string fullTypeName = $"{data.ImplementationNamespace}.{data.ClassName}, Assembly-CSharp";
                Type implementationType = Type.GetType(fullTypeName);

                if (implementationType != null) {
                    // Type is available, try to set the reference
                    SetImplementationReference(data, implementationType);
                    EditorPrefs.DeleteKey(PendingImplementationKey);
                } else {
                    // Type not ready yet, will try again next update
                    EditorApplication.update += CheckForPendingImplementations;
                }
            } catch (Exception ex) {
                Debug.LogError($"Error processing pending implementation: {ex.Message}");
                EditorPrefs.DeleteKey(PendingImplementationKey);
            }
        }

        private static void StorePendingImplementation(SerializedProperty property, string className, string implementationNamespace) {
            var data = new PendingImplementationData {
                PropertyPath = property.propertyPath,
                TargetObjectInstanceID = property.serializedObject.targetObject.GetInstanceID(),
                ClassName = className,
                ImplementationNamespace = implementationNamespace,
                BaseTypeName = property.managedReferenceFieldTypename
            };

            string json = JsonUtility.ToJson(data);
            EditorPrefs.SetString(PendingImplementationKey, json);

            // Set up a callback to check after domain reload
            EditorApplication.update += CheckForPendingImplementations;
        }

        private static void SetImplementationReference(PendingImplementationData data, Type implementationType) {
            // Find the target object
            var targetObject = EditorUtility.InstanceIDToObject(data.TargetObjectInstanceID) as UnityEngine.Object;
            if (targetObject == null) {
                Debug.LogWarning("Target object not found for pending implementation");
                return;
            }

            // Create a new SerializedObject and find the property
            var serializedObject = new SerializedObject(targetObject);
            var property = serializedObject.FindProperty(data.PropertyPath);

            if (property != null) {
                // Create an instance of the implementation type
                var instance = Activator.CreateInstance(implementationType);
                property.managedReferenceValue = instance;
                serializedObject.ApplyModifiedProperties();

                Debug.Log($"Successfully set {data.ClassName} as reference for {data.PropertyPath}");
            } else {
                Debug.LogWarning($"Property {data.PropertyPath} not found on target object");
            }
        }

        private static void CreateDefaultImplementationFile(SerializedProperty property, Type baseType, string className, string directory, bool setAsReference = false, bool isOneTimeImplementation = false) {
            string filePath = Path.Combine(directory, className + ".cs");

            // Get the namespace from the base type
            string baseNamespace = baseType.Namespace ?? "";
            string implementationNamespace = !string.IsNullOrEmpty(GeneratedImplementationsNamespace) ? GeneratedImplementationsNamespace : baseNamespace;

            // Generate default implementation template with more complete structure
            string code = GenerateDefaultImplementationTemplate(className, baseType, implementationNamespace, baseNamespace, isOneTimeImplementation);

            File.WriteAllText(filePath, code);
            AssetDatabase.ImportAsset(filePath);
            AssetDatabase.Refresh();

            Debug.Log($"Created default implementation: {filePath}");

            // If we need to set this as a reference, store the information for after domain reload
            if (setAsReference) {
                StorePendingImplementation(property, className, implementationNamespace);
            }
        }

        private static string GenerateDefaultImplementationTemplate(string className, Type baseType, string implementationNamespace, string baseNamespace, bool isOneTimeImplementation = false) {
            string baseTypeName = GetFullTypeName(baseType);

            // Get all abstract/virtual methods from the base type
            var methods = GetImplementableMethods(baseType);
            var properties = GetImplementableProperties(baseType);

            // Collect all types used in methods and properties
            var usedTypes = new HashSet<string>();
            CollectUsedTypes(methods, usedTypes);
            CollectUsedTypes(properties, usedTypes);

            // Generate using statements
            string usingStatements = GenerateUsingStatements(baseNamespace, implementationNamespace, usedTypes);

            // Generate TypePickerInfo attribute
            string typePickerInfo = GenerateTypePickerInfoAttribute(className, isOneTimeImplementation);

            string methodImplementations = "";
            foreach (var method in methods) {
                methodImplementations += GenerateMethodImplementation(method);
            }

            string propertyImplementations = "";
            foreach (var property in properties) {
                propertyImplementations += GeneratePropertyImplementation(property);
            }

            return $@"{usingStatements}
namespace {implementationNamespace} {{
    {typePickerInfo}
    public class {className} : {baseTypeName} {{
{propertyImplementations}
{methodImplementations}
    }}
}}";
        }

        private static string GetFullTypeName(Type type) {
            if (type.DeclaringType != null) {
                return GetFullTypeName(type.DeclaringType) + "." + type.Name;
            }
            return type.Name;
        }

        private static List<MethodInfo> GetImplementableMethods(Type baseType) {
            var methods = new List<MethodInfo>();

            // Get all methods from the base type and its interfaces
            var allTypes = new List<Type> { baseType };
            allTypes.AddRange(baseType.GetInterfaces());

            foreach (var type in allTypes) {
                var typeMethods = type.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly)
                    .Where(m => m.IsAbstract || m.IsVirtual)
                    .Where(m => !m.IsSpecialName) // Exclude property getters/setters
                    .ToList();

                methods.AddRange(typeMethods);
            }

            // Remove duplicates
            return methods.GroupBy(m => m.Name + m.GetParameters().Length).Select(g => g.First()).ToList();
        }

        private static List<PropertyInfo> GetImplementableProperties(Type baseType) {
            var properties = new List<PropertyInfo>();

            // Get all properties from the base type and its interfaces
            var allTypes = new List<Type> { baseType };
            allTypes.AddRange(baseType.GetInterfaces());

            foreach (var type in allTypes) {
                var typeProperties = type.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly)
                    .Where(p => p.CanRead || p.CanWrite)
                    .ToList();

                properties.AddRange(typeProperties);
            }

            // Remove duplicates
            return properties.GroupBy(p => p.Name).Select(g => g.First()).ToList();
        }

        private static string GenerateMethodImplementation(MethodInfo method) {
            string returnType = GetTypeName(method.ReturnType);
            string methodName = method.Name;
            string parameters = string.Join(", ", method.GetParameters().Select(p => $"{GetTypeName(p.ParameterType)} {p.Name}"));

            string returnStatement = "";
            if (method.ReturnType != typeof(void)) {
                if (method.ReturnType.IsValueType) {
                    returnStatement = $"return default({returnType});";
                } else {
                    returnStatement = "return null;";
                }
            }

            return $@"
        public {returnType} {methodName}({parameters}) {{
            // TODO: Implement {methodName}
            {returnStatement}
        }}";
        }

        private static string GeneratePropertyImplementation(PropertyInfo property) {
            string propertyType = GetTypeName(property.PropertyType);
            string propertyName = property.Name;

            string getter = "";
            string setter = "";

            if (property.CanRead) {
                string returnValue = property.PropertyType.IsValueType ? $"default({propertyType})" : "null";
                getter = $@"
        get {{
            // TODO: Implement {propertyName} getter
            return {returnValue};
        }}";
            }

            if (property.CanWrite) {
                setter = $@"
        set {{
            // TODO: Implement {propertyName} setter
        }}";
            }

            return $@"
        public {propertyType} {propertyName} {{{getter}{setter}
        }}";
        }

        private static string GetTypeName(Type type) {
            if (type == typeof(void)) return "void";
            if (type == typeof(int)) return "int";
            if (type == typeof(float)) return "float";
            if (type == typeof(double)) return "double";
            if (type == typeof(bool)) return "bool";
            if (type == typeof(string)) return "string";
            if (type == typeof(object)) return "object";

            if (type.IsGenericType) {
                string genericTypeName = type.Name.Split('`')[0];
                var genericArgs = type.GetGenericArguments().Select(GetTypeName);
                return $"{genericTypeName}<{string.Join(", ", genericArgs)}>";
            }

            return type.Name;
        }

        private static void CollectUsedTypes(IEnumerable<MethodInfo> methods, HashSet<string> usedTypes) {
            foreach (var method in methods) {
                // Add return type
                AddTypeToUsedTypes(method.ReturnType, usedTypes);

                // Add parameter types
                foreach (var parameter in method.GetParameters()) {
                    AddTypeToUsedTypes(parameter.ParameterType, usedTypes);
                }
            }
        }

        private static void CollectUsedTypes(IEnumerable<PropertyInfo> properties, HashSet<string> usedTypes) {
            foreach (var property in properties) {
                AddTypeToUsedTypes(property.PropertyType, usedTypes);
            }
        }

        private static void AddTypeToUsedTypes(Type type, HashSet<string> usedTypes) {
            if (type == null) return;

            // Skip primitive types and common types that don't need using statements
            if (IsPrimitiveOrCommonType(type)) return;

            // Add the namespace if it exists and is not empty
            if (!string.IsNullOrEmpty(type.Namespace)) {
                usedTypes.Add(type.Namespace);
            }

            // For generic types, also check generic arguments
            if (type.IsGenericType) {
                foreach (var genericArg in type.GetGenericArguments()) {
                    AddTypeToUsedTypes(genericArg, usedTypes);
                }
            }
        }

        private static bool IsPrimitiveOrCommonType(Type type) {
            // Common types that don't need using statements
            var commonTypes = new[] {
                typeof(void), typeof(object), typeof(string),
                typeof(int), typeof(float), typeof(double), typeof(bool),
                typeof(byte), typeof(sbyte), typeof(short), typeof(ushort),
                typeof(uint), typeof(long), typeof(ulong), typeof(char),
                typeof(decimal), typeof(IntPtr), typeof(UIntPtr)
            };

            if (commonTypes.Contains(type)) return true;

            // Check if it's a primitive type
            if (type.IsPrimitive) return true;

            // Check if it's in System namespace (most System types don't need explicit using)
            if (type.Namespace == "System") return true;

            return false;
        }

        private static string GenerateUsingStatements(string baseNamespace, string implementationNamespace, HashSet<string> usedTypes) {
            var usingStatements = new List<string>();

            // Always include UnityEngine
            usingStatements.Add("using UnityEngine;");

            // Always include Pulni.EditorTools for TypePickerInfo attribute
            usingStatements.Add("using Pulni.EditorTools;");

            // Add base namespace if different from implementation namespace
            if (!string.IsNullOrEmpty(baseNamespace) && baseNamespace != implementationNamespace) {
                usingStatements.Add($"using {baseNamespace};");
            }

            // Add other used namespaces, excluding System and UnityEngine
            foreach (var usedType in usedTypes.OrderBy(ns => ns)) {
                if (usedType != "System" && usedType != "UnityEngine" && usedType != baseNamespace && usedType != "Pulni.EditorTools") {
                    usingStatements.Add($"using {usedType};");
                }
            }

            return string.Join("\n", usingStatements) + "\n";
        }

        private static string GenerateTypePickerInfoAttribute(string className, bool isOneTimeImplementation) {
            string displayName;
            if (isOneTimeImplementation) {
                displayName = "Custom/" + className;
            } else {
                displayName = ObjectNames.NicifyVariableName(className);
            }

            return $"[TypePickerInfo(\"{displayName}\")]";
        }

        private static string SuggestImplementationName(SerializedProperty property, Type baseType) {
            // Get the component that owns the property
            var targetObj = property.serializedObject.targetObject;
            var objectName = GetPrefabOrObjectName(targetObj);

            // Get the component's type name
            var compTypeName = targetObj.GetType().Name;

            // Get the property's name
            var propName = property.propertyPath.Split('.')[0].Replace("k__BackingField", "");

            // Combine them with underscores
            var combined = $"{objectName}_{compTypeName}_{propName}_{baseType.Name}";

            // Remove any invalid characters (only allow letters, digits, and underscores)
            combined = Regex.Replace(combined, @"[^a-zA-Z0-9_]", "");

            // Ensure the first character is a letter or underscore
            if (!char.IsLetter(combined, 0) && combined[0] != '_') {
                combined = "_" + combined;
            }

            return combined;
        }

        private static string GetPrefabOrObjectName(UnityEngine.Object obj) {
            if (obj is Component comp) {
                var prefabStage = UnityEditor.SceneManagement.PrefabStageUtility.GetCurrentPrefabStage();
                if (prefabStage != null && (comp.gameObject == prefabStage.prefabContentsRoot || comp.transform.IsChildOf(prefabStage.prefabContentsRoot.transform))) {
                    return prefabStage.prefabContentsRoot.name;
                }

                var prefabAsset = PrefabUtility.GetNearestPrefabInstanceRoot(comp.gameObject);
                if (prefabAsset != null) {
                    return prefabAsset.name;
                }
            }

            return obj.name;
        }

        private static ScriptInfo FindScriptForType(Type type) {
            string[] guids = AssetDatabase.FindAssets("t:MonoScript");
            string targetClassName = type.Name;
            string targetNamespace = type.Namespace ?? "";

            foreach (string guid in guids) {
                string path = AssetDatabase.GUIDToAssetPath(guid);

                // Skip if not a .cs file
                if (!path.EndsWith(".cs", StringComparison.OrdinalIgnoreCase)) {
                    continue;
                }

                try {
                    string content = File.ReadAllText(path);

                    // Check if this file contains the class we're looking for
                    var classInfo = FindClassDefinitionWithLine(content, targetClassName, targetNamespace);
                    if (classInfo.found) {
                        MonoScript script = AssetDatabase.LoadAssetAtPath<MonoScript>(path);
                        if (script != null) {
                            return new ScriptInfo(script, classInfo.lineNumber);
                        }
                    }
                } catch (Exception ex) {
                    // Skip files that can't be read
                    Debug.LogWarning($"Could not read file {path}: {ex.Message}");
                }
            }

            return new ScriptInfo(null, 0);
        }

        private static (bool found, int lineNumber) FindClassDefinitionWithLine(string content, string className, string namespaceName) {
            // Single merged regex pattern for all type definitions
            // Matches: [modifiers] [partial/abstract/sealed] (class|interface|struct|enum) ClassName
            string mergedPattern = $@"\b(?:public\s+|private\s+|protected\s+|internal\s+|static\s+)*(?:partial\s+|abstract\s+|sealed\s+)?(?:class|interface|struct|enum)\s+{Regex.Escape(className)}\b";

            // Find the line number where the class is defined
            string[] lines = content.Split('\n');
            for (int i = 0; i < lines.Length; i++) {
                string line = lines[i];
                string cleanLine = RemoveCommentsAndStrings(line);

                if (Regex.IsMatch(cleanLine, mergedPattern, RegexOptions.IgnoreCase)) {
                    // If namespace is specified, check if the class is in the correct namespace
                    if (!string.IsNullOrEmpty(namespaceName)) {
                        // Look for namespace declaration before this line
                        bool foundNamespace = false;
                        for (int j = 0; j < i; j++) {
                            string namespacePattern = $@"\bnamespace\s+{Regex.Escape(namespaceName)}\b";
                            if (Regex.IsMatch(lines[j], namespacePattern, RegexOptions.IgnoreCase)) {
                                foundNamespace = true;
                                break;
                            }
                        }
                        if (!foundNamespace) {
                            continue; // Try next line
                        }
                    }

                    return (true, i + 1); // Line numbers are 1-based
                }
            }

            return (false, 0);
        }

        private static string RemoveCommentsAndStrings(string content) {
            // Remove single-line comments
            content = Regex.Replace(content, @"//.*$", "", RegexOptions.Multiline);

            // Remove multi-line comments
            content = Regex.Replace(content, @"/\*.*?\*/", "", RegexOptions.Singleline);

            // Remove string literals (both single and double quotes)
            content = Regex.Replace(content, @"""[^""]*""", "\"\"");
            content = Regex.Replace(content, @"'[^']*'", "''");

            // Remove verbatim strings
            content = Regex.Replace(content, @"@""[^""]*""", "@\"\"");

            return content;
        }
    }

    public struct ScriptInfo {
        public MonoScript script;
        public int lineNumber;

        public ScriptInfo(MonoScript script, int lineNumber = 0) {
            this.script = script;
            this.lineNumber = lineNumber;
        }
    }

    [System.Serializable]
    public class PendingImplementationData {
        public string PropertyPath;
        public int TargetObjectInstanceID;
        public string ClassName;
        public string ImplementationNamespace;
        public string BaseTypeName;
    }
}
