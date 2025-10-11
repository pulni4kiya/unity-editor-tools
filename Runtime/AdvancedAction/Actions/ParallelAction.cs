using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

namespace Pulni.EditorTools {
    [TypePickerInfo("Composite/Parallel Action", -500)]
    public class ParallelAction : IAdvancedAction {
        [SerializeReference, TypePicker] private List<IAdvancedAction> actions;
        public Task Invoke(ExecutionContext context) {
            return Task.WhenAll(actions.Select(action => action.Invoke(context)));
        }

        public string GetDescription() {
            if (actions == null || actions.Count == 0) {
                return "Do nothing";
            }
            return "Parallel: " + string.Join(", ", actions.Select(a => a?.GetDescription()));
        }
    }
}
