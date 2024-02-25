using System;
using UnityEngine;

namespace Pulni.EditorTools {
	public class TypePickerInfoAttribute : Attribute {
		public string Name { get; set; }
		public int Order { get; set; }

		public TypePickerInfoAttribute(string name = null, int order = 0) {
			Name = name;
			Order = order;
		}
	}
}
