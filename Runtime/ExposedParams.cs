using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Pulni.EditorTools {
	public class ExposedParams : MonoBehaviour {
#if UNITY_EDITOR || !UNITY_2021_2_OR_NEWER
		[field: SerializeField] public ParamsConfig ExposedParamsConfig { get; set; } = new ParamsConfig();
#endif

		[Serializable]
		public class ParamsConfig {
			[field: SerializeField] public bool IsReadOnly { get; set; }
			[field: SerializeField] public List<Param> Params { get; set; } = new List<Param>();
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
