using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Pulni.EditorTools.Attributes {
	public class InlineAttribute : PropertyAttribute {
		public bool PrependNameToInlinedFields { get; set; }

		public InlineAttribute(bool prependNameToInlinedFields = false) {
			this.PrependNameToInlinedFields = prependNameToInlinedFields;
		}
	}
}
