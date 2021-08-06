using System;
using UnityEngine;

[CreateAssetMenu(menuName = "Rust/Environment Volume Properties Collection")]
public class EnvironmentVolumePropertiesCollection : ScriptableObject
{
	[Serializable]
	public class OceanParameters
	{
		public AnimationCurve TransitionCurve = AnimationCurve.Linear(0f, 0f, 40f, 1f);

		[Range(0f, 1f)]
		public float DirectionalLightMultiplier = 0.25f;

		[Range(0f, 1f)]
		public float AmbientLightMultiplier;

		[Range(0f, 1f)]
		public float ReflectionMultiplier = 1f;

		[Range(0f, 1f)]
		public float SunMeshBrightnessMultiplier = 1f;

		[Range(0f, 1f)]
		public float MoonMeshBrightnessMultiplier = 1f;

		[Range(0f, 1f)]
		public float AtmosphereBrightnessMultiplier = 1f;

		[Range(0f, 1f)]
		public float LightColorMultiplier = 1f;

		public Color LightColor = Color.black;

		[Range(0f, 1f)]
		public float SunRayColorMultiplier = 1f;

		public Color SunRayColor = Color.black;

		[Range(0f, 1f)]
		public float MoonRayColorMultiplier = 1f;

		public Color MoonRayColor = Color.black;
	}

	public float TransitionSpeed = 1f;

	public EnvironmentVolumeProperties[] Properties;

	public OceanParameters OceanOverrides;

	public EnvironmentVolumeProperties FindQuality(int quality)
	{
		EnvironmentVolumeProperties[] properties = Properties;
		foreach (EnvironmentVolumeProperties environmentVolumeProperties in properties)
		{
			if (environmentVolumeProperties.ReflectionQuality == quality)
			{
				return environmentVolumeProperties;
			}
		}
		return null;
	}
}
