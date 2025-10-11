using System.Threading.Tasks;
using UnityEngine;

namespace Pulni.EditorTools {
    [TypePickerInfo("Composite/Conditional Action", -500)]
    public class ConditionalAction : IAdvancedAction {
        [field: SerializeReference, TypePicker] public IAdvancedCondition Condition { get; private set; }
        [field: SerializeReference, TypePicker] public IAdvancedAction IfTrue { get; private set; }
        [field: SerializeReference, TypePicker] public IAdvancedAction IfFalse { get; private set; }

        public async Task Invoke(ExecutionContext context) {
            if (this.Condition.CheckCondition(context)) {
                await this.IfTrue.Invoke(context);
            } else {
                await this.IfFalse.Invoke(context);
            }
        }

        public string GetDescription() {
            var conditionDesc = Condition?.GetDescription() ?? "unknown condition";
            var actionDesc = IfTrue?.GetDescription() ?? "unknown action";
            if (IfFalse is NoAction) {
                return $"If {conditionDesc}, then {actionDesc}";
            }
            var falseActionDesc = IfFalse?.GetDescription() ?? "unknown action";
            return $"If {conditionDesc}, then {actionDesc}, else {falseActionDesc}";
        }
    }
}
