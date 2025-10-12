using UnityEngine;

namespace Pulni.EditorTools {
	public class TypePickerAttribute : PropertyAttribute {
		public bool AllowNull { get; set; } = false;
		public string TypesGetterMethodName { get; set; }

		public TypePickerAttribute(bool allowNull, string typesGetterMethodName = null) {
			this.AllowNull = allowNull;
			this.TypesGetterMethodName = typesGetterMethodName;
		}

		public TypePickerAttribute(string typesGetterMethodName = null) {
			this.TypesGetterMethodName = typesGetterMethodName;
			this.AllowNull = false;
		}
	}
}