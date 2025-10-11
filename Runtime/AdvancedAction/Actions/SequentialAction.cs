using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Pulni.EditorTools;
using UnityEngine;

namespace Pulni.EditorTools {
	[TypePickerInfo("Composite/Sequential Action", -500)]
	public class SequentialAction : IAdvancedAction {
		[SerializeReference, TypePicker] private List<IAdvancedAction> actions;

		public async Task Invoke(ExecutionContext context) {
			foreach (var action in this.actions) {
				await action.Invoke(context);
			}
		}

		public string GetDescription() {
			if (actions == null || actions.Count == 0) {
				return "Do nothing";
			}
			return string.Join(", ", actions.Select(a => a?.GetDescription()));
		}
	}
}
