using UnityEngine;

namespace Pulni.EditorTools {
    public interface IEditorDescribable : ITypePickerCompact {
        public string GetDescription();
    }
}
