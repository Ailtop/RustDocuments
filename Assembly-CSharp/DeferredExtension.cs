using UnityEngine;

[RequireComponent(typeof(CommandBufferManager))]
[ExecuteInEditMode]
[RequireComponent(typeof(Camera))]
public class DeferredExtension : MonoBehaviour
{
	public ExtendGBufferParams extendGBuffer = ExtendGBufferParams.Default;

	public SubsurfaceScatteringParams subsurfaceScattering = SubsurfaceScatteringParams.Default;

	public ScreenSpaceRefractionParams screenSpaceRefraction = ScreenSpaceRefractionParams.Default;

	public Texture2D blueNoise;

	public Texture preintegratedFGD_GGX;

	public Texture envBrdfLut;

	public float depthScale = 100f;

	public bool debug;

	public bool forceToCameraResolution;
}
