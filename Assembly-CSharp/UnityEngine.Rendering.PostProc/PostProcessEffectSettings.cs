using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;

namespace UnityEngine.Rendering.PostProcessing
{
	[Serializable]
	public class PostProcessEffectSettings : ScriptableObject
	{
		public bool active = true;

		public BoolParameter enabled = new BoolParameter
		{
			overrideState = true,
			value = false
		};

		internal ReadOnlyCollection<ParameterOverride> parameters;

		private void OnEnable()
		{
			parameters = (from t in GetType().GetFields(BindingFlags.Instance | BindingFlags.Public)
				where t.FieldType.IsSubclassOf(typeof(ParameterOverride))
				orderby t.MetadataToken
				select (ParameterOverride)t.GetValue(this)).ToList().AsReadOnly();
			foreach (ParameterOverride parameter in parameters)
			{
				parameter.OnEnable();
			}
		}

		private void OnDisable()
		{
			if (parameters == null)
			{
				return;
			}
			foreach (ParameterOverride parameter in parameters)
			{
				parameter.OnDisable();
			}
		}

		public void SetAllOverridesTo(bool state, bool excludeEnabled = true)
		{
			foreach (ParameterOverride parameter in parameters)
			{
				if (!excludeEnabled || parameter != enabled)
				{
					parameter.overrideState = state;
				}
			}
		}

		public virtual bool IsEnabledAndSupported(PostProcessRenderContext context)
		{
			return enabled.value;
		}

		public int GetHash()
		{
			int num = 17;
			foreach (ParameterOverride parameter in parameters)
			{
				num = num * 23 + parameter.GetHash();
			}
			return num;
		}
	}
}
