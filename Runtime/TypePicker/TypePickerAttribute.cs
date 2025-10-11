using UnityEngine;

namespace Pulni.EditorTools {
	public class TypePickerAttribute : PropertyAttribute {
		public bool AllowNull { get; set; } = false;
		public string TypesGetterMethodName { get; set; }
		public TypePickerDrawMode DrawMode { get; set; } = TypePickerDrawMode.BasedOnType;

		public TypePickerAttribute(bool allowNull, string typesGetterMethodName = null) {
			this.AllowNull = allowNull;
			this.TypesGetterMethodName = typesGetterMethodName;
		}

		public TypePickerAttribute(string typesGetterMethodName = null) {
			this.TypesGetterMethodName = typesGetterMethodName;
			this.AllowNull = false;
		}

		public enum TypePickerDrawMode {
			BasedOnType, // If the type implements ITypePickerCompact, use Compact, otherwise use Extended
			Compact,
			Extended
		}
	}
}