using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Pulni.EditorTools.Editor {
    public interface ITypeImplementationGenerator {
        public string GenerateImplementation(SerializedProperty property, Type baseType, string className, string newImplementationNamespace, string typePickerInfo, bool isOneTimeImplementation);
    }
}
