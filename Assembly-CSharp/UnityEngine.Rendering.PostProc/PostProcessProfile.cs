using System;
using System.Collections.Generic;

namespace UnityEngine.Rendering.PostProcessing
{
	public sealed class PostProcessProfile : ScriptableObject
	{
		[Tooltip("A list of all settings currently stored in this profile.")]
		public List<PostProcessEffectSettings> settings = new List<PostProcessEffectSettings>();

		[NonSerialized]
		public bool isDirty = true;

		private void OnEnable()
		{
			settings.RemoveAll((PostProcessEffectSettings x) => x == null);
		}

		public T AddSettings<T>() where T : PostProcessEffectSettings
		{
			return (T)AddSettings(typeof(T));
		}

		public PostProcessEffectSettings AddSettings(Type type)
		{
			if (HasSettings(type))
			{
				throw new InvalidOperationException("Effect already exists in the stack");
			}
			PostProcessEffectSettings postProcessEffectSettings = (PostProcessEffectSettings)ScriptableObject.CreateInstance(type);
			postProcessEffectSettings.hideFlags = HideFlags.HideInHierarchy | HideFlags.HideInInspector;
			postProcessEffectSettings.name = type.Name;
			postProcessEffectSettings.enabled.value = true;
			settings.Add(postProcessEffectSettings);
			isDirty = true;
			return postProcessEffectSettings;
		}

		public PostProcessEffectSettings AddSettings(PostProcessEffectSettings effect)
		{
			if (HasSettings(settings.GetType()))
			{
				throw new InvalidOperationException("Effect already exists in the stack");
			}
			settings.Add(effect);
			isDirty = true;
			return effect;
		}

		public void RemoveSettings<T>() where T : PostProcessEffectSettings
		{
			RemoveSettings(typeof(T));
		}

		public void RemoveSettings(Type type)
		{
			int num = -1;
			for (int i = 0; i < settings.Count; i++)
			{
				if (settings[i].GetType() == type)
				{
					num = i;
					break;
				}
			}
			if (num < 0)
			{
				throw new InvalidOperationException("Effect doesn't exist in the profile");
			}
			settings.RemoveAt(num);
			isDirty = true;
		}

		public bool HasSettings<T>() where T : PostProcessEffectSettings
		{
			return HasSettings(typeof(T));
		}

		public bool HasSettings(Type type)
		{
			foreach (PostProcessEffectSettings setting in settings)
			{
				if (setting.GetType() == type)
				{
					return true;
				}
			}
			return false;
		}

		public T GetSetting<T>() where T : PostProcessEffectSettings
		{
			foreach (PostProcessEffectSettings setting in settings)
			{
				if (setting is T)
				{
					return setting as T;
				}
			}
			return null;
		}

		public bool TryGetSettings<T>(out T outSetting) where T : PostProcessEffectSettings
		{
			Type typeFromHandle = typeof(T);
			outSetting = null;
			foreach (PostProcessEffectSettings setting in settings)
			{
				if (setting.GetType() == typeFromHandle)
				{
					outSetting = (T)setting;
					return true;
				}
			}
			return false;
		}
	}
}
