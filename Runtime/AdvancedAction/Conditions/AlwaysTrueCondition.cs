using UnityEngine;

namespace Pulni.EditorTools {
    [TypePickerInfo("Constant/True", -1001)]
    public class AlwaysTrueCondition : IAdvancedCondition {
        public bool CheckCondition(ExecutionContext context) {
            return true;
        }

        public string GetDescription() {
            return "TRUE";
        }
    }
}
