using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Pulni.EditorTools;
using UnityEngine;
using UnityEngine.Events;

namespace Pulni.EditorTools {
	[TypePickerInfo("Unity/Unity Event", 300)]
	public class UnityEventAction : IAdvancedAction {
		[SerializeField] private UnityEvent actions;

		public Task Invoke(ExecutionContext context) {
			this.actions.Invoke();
			return Task.CompletedTask;
		}

		public string GetDescription() {
			return "Invoke Unity events";
		}
	}
}
