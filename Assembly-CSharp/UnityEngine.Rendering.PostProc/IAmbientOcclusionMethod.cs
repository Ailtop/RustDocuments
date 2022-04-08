namespace UnityEngine.Rendering.PostProcessing;

internal interface IAmbientOcclusionMethod
{
	DepthTextureMode GetCameraFlags();

	void RenderAfterOpaque(PostProcessRenderContext context);

	void RenderAmbientOnly(PostProcessRenderContext context);

	void CompositeAmbientOnly(PostProcessRenderContext context);

	void Release();
}
