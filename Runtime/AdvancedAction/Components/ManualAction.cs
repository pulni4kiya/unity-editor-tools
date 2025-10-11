using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Pulni.EditorTools;
using UnityEngine;

namespace Pulni.EditorTools {
	public class ManualAction : MonoBehaviour {
		[SerializeReference, TypePicker] private IAdvancedAction action = new NoAction();

		public async void Invoke() {
			await action.Invoke(new ExecutionContext(this));
		}
	}
}