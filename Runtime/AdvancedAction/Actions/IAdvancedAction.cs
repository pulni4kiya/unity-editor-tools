using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace Pulni.EditorTools {
	public interface IAdvancedAction : IEditorDescribable {
		public Task Invoke(ExecutionContext context);
	}
}
