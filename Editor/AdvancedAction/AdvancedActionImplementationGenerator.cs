using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace Pulni.EditorTools.Editor {
    internal class AdvancedActionImplementationGenerator : ITypeImplementationGenerator {
        [InitializeOnLoadMethod]
        private static void Initialize() {
            TypePickerStandardMenu.TypeImplementationsGenerators[typeof(IAdvancedAction)] = new AdvancedActionImplementationGenerator();
        }

        public string GenerateImplementation(SerializedProperty property, Type baseType, string newClassName, string newImplementationNamespace, string typePickerInfo, bool isOneTimeImplementation) {
            var inputType = GetActionInputType(property);
            var componentType = property.serializedObject.targetObject.GetType();

            var code = $@"using System.Threading.Tasks;
using UnityEngine;
using {typeof(IAdvancedAction).Namespace};
{(inputType != null && !string.IsNullOrEmpty(inputType.Namespace) ? $"using {inputType.Namespace};" : "")}

namespace {newImplementationNamespace} {{
    {typePickerInfo}
    public class {newClassName} : {GetFullNestedTypeName(baseType)} {{
        public async Task Invoke({nameof(ExecutionContext)} context) {{
            {(componentType != null ? $"var component = ({GetFullNestedTypeName(componentType)})context.ContainingObject;" : "")}
            {(inputType != null ? $"var input = ({GetFullNestedTypeName(inputType)})context.Input;" : "")}
            // TODO: Implement action
        }}

        public string GetDescription() {{
            return ""{(isOneTimeImplementation ? "One-time-action" : "")}"";
        }}
    }}
}}";

            return code;
        }

        private Type GetActionInputType(SerializedProperty property) {
            object currentObject = property.serializedObject.targetObject;
            Type lastType = null;
            var tokens = property.propertyPath.Split('.');
            foreach (string token in tokens) {
                if (currentObject == null) break;

                if (currentObject is IAdvancedActionInputProvider inputProvider) {
                    lastType = inputProvider.GetInputType();
                }

                if (token == "Array") continue;

                if (token.Contains("data[")) {
                    var bracketIndex = token.IndexOf('[');
                    var indexStart = bracketIndex + 1;
                    var indexEnd = token.IndexOf(']');
                    var indexStr = token.Substring(indexStart, indexEnd - indexStart);
                    if (int.TryParse(indexStr, out var index)) {
                        if (currentObject is System.Collections.IList list && list.Count > index) {
                            currentObject = list[index];
                        } else {
                            currentObject = null;
                        }
                    }
                    continue;
                }

                var fieldName = token;

                var currentType = currentObject.GetType();
                var fi = currentType.GetFieldInParents(fieldName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                if (fi != null) {
                    var attr = fi.GetCustomAttribute<ActionInputAttribute>();
                    if (attr != null) {
                        lastType = attr.Input;
                    }
                    currentObject = fi.GetValue(currentObject);
                } else {
                    break;
                }
            }
            return lastType;
        }

        public static string GetFullNestedTypeName(Type type) {
            if (type == null)
                return string.Empty;
            return type.DeclaringType == null
                ? type.Name
                : GetFullNestedTypeName(type.DeclaringType) + "." + type.Name;
        }
    }
}
