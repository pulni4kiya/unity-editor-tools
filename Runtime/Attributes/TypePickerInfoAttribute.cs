using System;
using UnityEngine;

namespace Pulni.Attributes {
	public class TypePickerInfoAttribute : Attribute {
		public string Name { get; set; }
		public int Order { get; set; }

		public TypePickerInfoAttribute(string name, int order = 0) {
			Name = name;
			Order = order;
		}
	}
}
