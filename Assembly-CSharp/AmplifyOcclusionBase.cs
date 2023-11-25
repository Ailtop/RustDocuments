using System;
using UnityEngine;

[AddComponentMenu("")]
public class AmplifyOcclusionBase : MonoBehaviour
{
	public enum ApplicationMethod
	{
		PostEffect = 0,
		Deferred = 1,
		Debug = 2
	}

	public enum PerPixelNormalSource
	{
		None = 0,
		Camera = 1,
		GBuffer = 2,
		GBufferOctaEncoded = 3
	}

	public enum SampleCountLevel
	{
		Low = 0,
		Medium = 1,
		High = 2,
		VeryHigh = 3
	}

	[Header("Ambient Occlusion")]
	public ApplicationMethod ApplyMethod;

	[Tooltip("Number of samples per pass.")]
	public SampleCountLevel SampleCount = SampleCountLevel.Medium;

	public PerPixelNormalSource PerPixelNormals = PerPixelNormalSource.Camera;

	[Tooltip("Final applied intensity of the occlusion effect.")]
	[Range(0f, 1f)]
	public float Intensity = 1f;

	public Color Tint = Color.black;

	[Tooltip("Radius spread of the occlusion.")]
	[Range(0f, 32f)]
	public float Radius = 2f;

	[NonSerialized]
	[Range(32f, 1024f)]
	[Tooltip("Max sampling range in pixels.")]
	public int PixelRadiusLimit = 512;

	[NonSerialized]
	[Range(0f, 2f)]
	[Tooltip("Occlusion contribution amount on relation to radius.")]
	public float RadiusIntensity = 1f;

	[Tooltip("Power exponent attenuation of the occlusion.")]
	[Range(0f, 16f)]
	public float PowerExponent = 1.8f;

	[Range(0f, 0.99f)]
	[Tooltip("Controls the initial occlusion contribution offset.")]
	public float Bias = 0.05f;

	[Range(0f, 1f)]
	[Tooltip("Controls the thickness occlusion contribution.")]
	public float Thickness = 1f;

	[Tooltip("Compute the Occlusion and Blur at half of the resolution.")]
	public bool Downsample = true;

	[Header("Distance Fade")]
	[Tooltip("Control parameters at faraway.")]
	public bool FadeEnabled;

	[Tooltip("Distance in Unity unities that start to fade.")]
	public float FadeStart = 100f;

	[Tooltip("Length distance to performe the transition.")]
	public float FadeLength = 50f;

	[Range(0f, 1f)]
	[Tooltip("Final Intensity parameter.")]
	public float FadeToIntensity;

	public Color FadeToTint = Color.black;

	[Tooltip("Final Radius parameter.")]
	[Range(0f, 32f)]
	public float FadeToRadius = 2f;

	[Tooltip("Final PowerExponent parameter.")]
	[Range(0f, 16f)]
	public float FadeToPowerExponent = 1.8f;

	[Range(0f, 1f)]
	[Tooltip("Final Thickness parameter.")]
	public float FadeToThickness = 1f;

	[Header("Bilateral Blur")]
	public bool BlurEnabled = true;

	[Tooltip("Radius in screen pixels.")]
	[Range(1f, 4f)]
	public int BlurRadius = 3;

	[Tooltip("Number of times that the Blur will repeat.")]
	[Range(1f, 4f)]
	public int BlurPasses = 1;

	[Tooltip("0 - Blured, 1 - Sharpened.")]
	[Range(0f, 20f)]
	public float BlurSharpness = 10f;

	[Header("Temporal Filter")]
	[Tooltip("Accumulates the effect over the time.")]
	public bool FilterEnabled = true;

	[Tooltip("Controls the accumulation decayment. 0 - Faster update, more flicker. 1 - Slow update (ghosting on moving objects), less flicker.")]
	[Range(0f, 1f)]
	public float FilterBlending = 0.5f;

	[Range(0f, 1f)]
	[Tooltip("Controls the discard sensibility based on the motion of the scene and objects. 0 - Discard less, reuse more (more ghost effect). 1 - Discard more, reuse less (less ghost effect).")]
	public float FilterResponse = 0.5f;

	[NonSerialized]
	[Tooltip("Enables directional variations.")]
	public bool TemporalDirections = true;

	[NonSerialized]
	[Tooltip("Enables offset variations.")]
	public bool TemporalOffsets = true;

	[NonSerialized]
	[Tooltip("Reduces ghosting effect near the objects's edges while moving.")]
	public bool TemporalDilation;

	[NonSerialized]
	[Tooltip("Uses the object movement information for calc new areas of occlusion.")]
	public bool UseMotionVectors = true;
}
