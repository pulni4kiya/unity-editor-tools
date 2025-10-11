using System.Threading.Tasks;
using UnityEngine;

namespace Pulni.EditorTools {
    [TypePickerInfo("Unity/Destroy Game Object")]
    public class DestroyAction : IAdvancedAction {
        [SerializeField] private GameObject gameObject;

        public async Task Invoke(ExecutionContext context) {
            UnityEngine.Object.Destroy(this.gameObject);
        }

        public string GetDescription() {
            return $"Destroy {gameObject?.name ?? "unknown object"}";
        }
    }
}
