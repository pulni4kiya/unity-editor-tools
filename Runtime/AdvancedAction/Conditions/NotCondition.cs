using UnityEngine;

namespace Pulni.EditorTools {
    [TypePickerInfo("Logic/Invert", -500)]
    public class NotCondition : IAdvancedCondition {
        [field: SerializeReference, TypePicker] public IAdvancedCondition Condition { get; private set; }

        public bool CheckCondition(ExecutionContext context) {
            return !this.Condition.CheckCondition(context);
        }

        public string GetDescription() {
            var conditionDesc = Condition?.GetDescription() ?? "unknown condition";
            return $"NOT {conditionDesc}";
        }
    }
}
