using UnityEngine.Scripting;

namespace UnityEngine.Rendering.PostProcessing
{
	[Preserve]
	internal sealed class AmbientOcclusionRenderer : PostProcessEffectRenderer<AmbientOcclusion>
	{
		private IAmbientOcclusionMethod[] m_Methods;

		public override void Init()
		{
			if (m_Methods == null)
			{
				m_Methods = new IAmbientOcclusionMethod[2]
				{
					new ScalableAO(base.settings),
					new MultiScaleVO(base.settings)
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

		public IAmbientOcclusionMethod Get()
		{
			return m_Methods[(int)base.settings.mode.value];
		}

		public override DepthTextureMode GetCameraFlags()
		{
			return Get().GetCameraFlags();
		}

		public override void Release()
		{
			IAmbientOcclusionMethod[] methods = m_Methods;
			for (int i = 0; i < methods.Length; i++)
			{
				methods[i].Release();
			}
		}

		public ScalableAO GetScalableAO()
		{
			return (ScalableAO)m_Methods[0];
		}

		public MultiScaleVO GetMultiScaleVO()
		{
			return (MultiScaleVO)m_Methods[1];
		}

		public override void Render(PostProcessRenderContext context)
		{
		}
	}
}
