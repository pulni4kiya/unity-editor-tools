using System.Threading.Tasks;
using Pulni.EditorTools;
using UnityEngine;

[TypePickerInfo("Unity/Set Camera to Canvas")]
public class SetCameraToCanvas : IAdvancedAction {
    [SerializeField] private Canvas canvas;
    [SerializeField] private Camera camera;

    public async Task Invoke(ExecutionContext context) {
        if (this.camera == null) {
            this.camera = Camera.main;
        }
        this.canvas.worldCamera = this.camera;
    }

    public string GetDescription() {
        return $"Set camera {camera?.name ?? "main"} to canvas {canvas?.name ?? "unknown"}";
    }
}
