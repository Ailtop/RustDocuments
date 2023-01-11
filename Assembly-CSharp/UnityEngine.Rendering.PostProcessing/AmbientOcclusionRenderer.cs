using UnityEngine.Scripting;

namespace UnityEngine.Rendering.PostProcessing;

[Preserve]
internal sealed class AmbientOcclusionRenderer : PostProcessEffectRenderer<AmbientOcclusion>
{
	private UnityEngine.Rendering.PostProcessing.IAmbientOcclusionMethod[] m_Methods;

	public override void Init()
	{
		if (m_Methods == null)
		{
			m_Methods = new UnityEngine.Rendering.PostProcessing.IAmbientOcclusionMethod[2]
			{
				new UnityEngine.Rendering.PostProcessing.ScalableAO(base.settings),
				new UnityEngine.Rendering.PostProcessing.MultiScaleVO(base.settings)
			};
		}
	}

	public bool IsAmbientOnly(PostProcessRenderContext context)
	{
		Camera camera = context.camera;
		if (base.settings.ambientOnly.value && camera.actualRenderingPath == RenderingPath.DeferredShading)
		{
			return camera.allowHDR;
		}
		return false;
	}

	public UnityEngine.Rendering.PostProcessing.IAmbientOcclusionMethod Get()
	{
		return m_Methods[(int)base.settings.mode.value];
	}

	public override DepthTextureMode GetCameraFlags()
	{
		return Get().GetCameraFlags();
	}

	public override void Release()
	{
		UnityEngine.Rendering.PostProcessing.IAmbientOcclusionMethod[] methods = m_Methods;
		for (int i = 0; i < methods.Length; i++)
		{
			methods[i].Release();
		}
	}

	public UnityEngine.Rendering.PostProcessing.ScalableAO GetScalableAO()
	{
		return (UnityEngine.Rendering.PostProcessing.ScalableAO)m_Methods[0];
	}

	public UnityEngine.Rendering.PostProcessing.MultiScaleVO GetMultiScaleVO()
	{
		return (UnityEngine.Rendering.PostProcessing.MultiScaleVO)m_Methods[1];
	}

	public override void Render(PostProcessRenderContext context)
	{
	}
}
