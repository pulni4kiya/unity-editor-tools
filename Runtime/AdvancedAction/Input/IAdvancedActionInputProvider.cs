using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pulni.EditorTools {
	public interface IAdvancedActionInputProvider {
		public Type GetInputType();
	}
}
