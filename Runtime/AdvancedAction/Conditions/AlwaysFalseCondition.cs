using UnityEngine;

namespace Pulni.EditorTools {
    [TypePickerInfo("Constant/False", -1000)]
    public class AlwaysFalseCondition : IAdvancedCondition {
        public bool CheckCondition(ExecutionContext context) {
            return false;
        }

        public string GetDescription() {
            return "FALSE";
        }
    }
}
