using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Pulni.EditorTools;
using UnityEngine;

namespace Pulni.EditorTools {
	[TypePickerInfo("No Action", order: -1000)]
	public class NoAction : IAdvancedAction {
		public Task Invoke(ExecutionContext context) {
			return Task.CompletedTask;
		}

		public string GetDescription() {
			return "Do nothing";
		}
	}
}