using System.Collections;
using System.Collections.Generic;
using Pulni.EditorTools;
using UnityEngine;

public class ObjectLifetimeAction : MonoBehaviour {
	[SerializeReference, TypePicker] private IAdvancedAction StartAction = new NoAction();
	[SerializeReference, TypePicker] private IAdvancedAction DestroyAction = new NoAction();
	[SerializeReference, TypePicker] private IAdvancedAction EnableAction = new NoAction();
	[SerializeReference, TypePicker] private IAdvancedAction DisableAction = new NoAction();

	private void Start() {
		this.StartAction.Invoke(new ExecutionContext(this));
	}

	private void OnDestroy() {
		this.DestroyAction.Invoke(new ExecutionContext(this));
	}

	private void OnEnable() {
		this.EnableAction.Invoke(new ExecutionContext(this));
	}

	private void OnDisable() {
		this.DisableAction.Invoke(new ExecutionContext(this));
	}
}
