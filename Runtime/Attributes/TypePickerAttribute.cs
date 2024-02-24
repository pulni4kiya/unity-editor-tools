using UnityEngine;

namespace Pulni.Attributes {
	public class TypePickerAttribute : PropertyAttribute {
		public string TypesGetterMethodName { get; set; }

		public TypePickerAttribute(string typesGetterMethodName = null) {
			this.TypesGetterMethodName = typesGetterMethodName;
		}
	}
}