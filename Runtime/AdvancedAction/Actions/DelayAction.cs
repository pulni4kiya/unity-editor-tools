using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Pulni.EditorTools;
using UnityEngine;

namespace Pulni.EditorTools {
	[TypePickerInfo("Delay (Seconds)")]
	public class DelaySecondsAction : IAdvancedAction {
		private static DelayWebGLHelper delayGameObject;

		[SerializeField] private float seconds;
		public Task Invoke(ExecutionContext context) {
#if UNITY_WEBGL
			this.InitWebGL();

			var tcs = new TaskCompletionSource<object>();
			delayGameObject.StartCoroutine(DelayWebGL(this.seconds, tcs));
			return tcs.Task;
#else
			return Task.Delay(TimeSpan.FromSeconds(this.seconds));
#endif
		}

		public string GetDescription() {
			return $"Delay for {seconds} seconds";
		}

#if UNITY_WEBGL
		private IEnumerator DelayWebGL(float seconds, TaskCompletionSource<object> tcs) {
			yield return new WaitForSecondsRealtime(seconds);
			tcs.SetResult(null);
		}

		private void InitWebGL() {
			if (delayGameObject == null) {
				var gameObject = new GameObject("DelayGameObject");
				GameObject.DontDestroyOnLoad(gameObject);
				delayGameObject = gameObject.AddComponent<DelayWebGLHelper>();
			}
		}

		private class DelayWebGLHelper : MonoBehaviour { }
#endif
	}
}
