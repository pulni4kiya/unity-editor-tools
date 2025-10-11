using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Pulni.EditorTools;
using UnityEngine;

namespace Pulni.EditorTools {
	[TypePickerInfo("Unity/Enable \\ Disable Behaviours")]
	public class EnableBehavioursAction : IAdvancedAction {
		[SerializeField] private Mode mode;
		[SerializeField] private List<Behaviour> behaviours;

		public async Task Invoke(ExecutionContext context) {
			var enabled = this.mode == Mode.Enable;
			foreach (var behaviour in this.behaviours) {
				behaviour.enabled = enabled;
			}
		}

		public string GetDescription() {
			var action = mode == Mode.Enable ? "Enable" : "Disable";
			if (behaviours == null || behaviours.Count == 0) {
				return $"{action} no behaviours";
			}

			return $"{action} {string.Join(", ", behaviours.Where(b => b != null).Select(b => $"{b.gameObject.name}'s {b.GetType().Name}"))}";
		}

		public enum Mode {
			Enable,
			Disable
		}
	}
}
