using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Pulni.EditorTools;
using UnityEngine;

namespace Pulni.EditorTools {
	[TypePickerInfo("Unity/Activate \\ Deactivate Objects", 300)]
	public class ActivateObjectsAction : IAdvancedAction {
		[SerializeField] private Mode mode;
		[SerializeField] private List<GameObject> objects;

		public async Task Invoke(ExecutionContext context) {
			var active = this.mode == Mode.Activate;
			foreach (var obj in this.objects) {
				obj.SetActive(active);
			}
		}

		public string GetDescription() {
			var action = mode == Mode.Activate ? "Activate" : "Deactivate";
			return $"{action} {string.Join(", ", objects.Where(o => o != null).Select(o => o.name))}";
		}

		public enum Mode {
			Activate,
			Deactivate
		}
	}
}
