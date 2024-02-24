using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Pulni.EditorTools {
	public class ExposedProperties : MonoBehaviour {
#if UNITY_EDITOR || !UNITY_2021_2_OR_NEWER
		[field: SerializeField] public PropertiesConfig ExposedPropertiesConfig { get; set; } = new PropertiesConfig();
#endif

		[Serializable]
		public class PropertiesConfig {
			[field: SerializeField] public bool IsReadOnly { get; set; }
			[field: SerializeField] public List<Param> Properties { get; set; } = new List<Param>();
		}

		[Serializable]
		public class Param {
			public string Label;
			public UnityEngine.Object Target;
			public string PropertyPath;
			public bool IsReadOnly;
			public bool ShowChildren = true;
		}
	}
}
