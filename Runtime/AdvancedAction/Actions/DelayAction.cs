using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Pulni.EditorTools;
using UnityEngine;

namespace Pulni.EditorTools {
	[TypePickerInfo("Delay (Seconds)")]
	public class DelaySecondsAction : IAdvancedAction {
		[SerializeField] private float seconds;
		[SerializeField] private bool realtime = false;
		public async Task Invoke(ExecutionContext context) {
			if (seconds <= 0f) {
				return;
			}

			var dt = 0f;
			while (dt < seconds) {
				await Task.Yield();
				dt += realtime ? Time.unscaledDeltaTime : Time.deltaTime;
			}
		}

		public string GetDescription() {
			return $"Delay for {seconds} seconds";
		}
	}
}
