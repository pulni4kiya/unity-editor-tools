using System.Threading.Tasks;
using UnityEngine;


namespace Pulni.EditorTools {
	public interface IAdvancedCondition : IEditorDescribable {
		public bool CheckCondition(ExecutionContext context);
	}
}