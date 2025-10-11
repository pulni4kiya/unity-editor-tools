using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Pulni.EditorTools {
	public class ExecutionContext {
		public UnityEngine.Object ContainingObject;
		public object Input;

		public ExecutionContext(UnityEngine.Object containingObject, object input = null) {
			this.ContainingObject = containingObject;
			this.Input = input;
		}
	}
}
