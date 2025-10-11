using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Pulni.EditorTools {
    [TypePickerInfo("Logic/Composite", -500)]
    public class ConditionList : IAdvancedCondition {
        [field: SerializeField] public LogicalOperator Operator { get; private set; } = LogicalOperator.And;
        [field: SerializeReference, TypePicker] public List<IAdvancedCondition> Conditions { get; private set; } = new List<IAdvancedCondition>();

        public bool CheckCondition(ExecutionContext context) {
            if (this.Conditions == null || this.Conditions.Count == 0) {
                return true; // Empty condition list is considered true
            }

            if (this.Operator == LogicalOperator.And) {
                return this.Conditions.All(condition => condition != null && condition.CheckCondition(context));
            } else {
                return this.Conditions.Any(condition => condition != null && condition.CheckCondition(context));
            }
        }

        public string GetDescription() {
            if (Conditions == null || Conditions.Count == 0) {
                return "True"; // Empty condition list is considered true
            }
            var descriptions = Conditions.Where(c => c != null).Select(c => c.GetDescription());
            var op = Operator == LogicalOperator.And ? " AND " : " OR ";
            return string.Join(op, descriptions);
        }

        public enum LogicalOperator {
            And,
            Or
        }
    }
}
