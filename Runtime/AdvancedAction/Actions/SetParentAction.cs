using System.Threading.Tasks;
using Pulni.EditorTools;
using UnityEngine;

[TypePickerInfo("Unity/Set Parent")]
public class SetParent : IAdvancedAction {
    [SerializeField] private Transform target;
    [SerializeField] private Transform newParent;

    public async Task Invoke(ExecutionContext context) {
        this.target.SetParent(this.newParent);
    }

    public string GetDescription() {
        return $"Set parent of {target?.name ?? "unknown"} to {newParent?.name ?? "unknown"}";
    }
}
