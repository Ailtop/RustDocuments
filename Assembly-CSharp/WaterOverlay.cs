using System;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

public class WaterOverlay : MonoBehaviour, IClientComponent
{
	[Serializable]
	public struct EffectParams
	{
		public float scatterCoefficient;

		public bool blur;

		public float blurDistance;

		public bool wiggle;

		public float doubleVisionAmount;

		public float photoFilterDensity;

		public static EffectParams DefaultGoggles = new EffectParams
		{
			scatterCoefficient = 0.1f,
			blur = false,
			blurDistance = 10f,
			wiggle = false,
			doubleVisionAmount = 0.753f,
			photoFilterDensity = 1f
		};
	}

	public PostProcessVolume postProcessVolume;

	public static bool goggles;

	public EffectParams gogglesParams = EffectParams.DefaultGoggles;
}
