using UnityEngine.Scripting;

namespace UnityEngine.Rendering.PostProcessing
{
	[Preserve]
	internal sealed class AutoExposureRenderer : PostProcessEffectRenderer<AutoExposure>
	{
		private const int k_NumEyes = 2;

		private const int k_NumAutoExposureTextures = 2;

		private readonly RenderTexture[][] m_AutoExposurePool = new RenderTexture[2][];

		private int[] m_AutoExposurePingPong = new int[2];

		private RenderTexture m_CurrentAutoExposure;

		public AutoExposureRenderer()
		{
			for (int i = 0; i < 2; i++)
			{
				m_AutoExposurePool[i] = new RenderTexture[2];
				m_AutoExposurePingPong[i] = 0;
			}
		}

		private void CheckTexture(int eye, int id)
		{
			if (m_AutoExposurePool[eye][id] == null || !m_AutoExposurePool[eye][id].IsCreated())
			{
				m_AutoExposurePool[eye][id] = new RenderTexture(1, 1, 0, RenderTextureFormat.RFloat)
				{
					enableRandomWrite = true
				};
				m_AutoExposurePool[eye][id].Create();
			}
		}

		public override void Render(PostProcessRenderContext context)
		{
			CommandBuffer command = context.command;
			command.BeginSample("AutoExposureLookup");
			CheckTexture(context.xrActiveEye, 0);
			CheckTexture(context.xrActiveEye, 1);
			float x = base.settings.filtering.value.x;
			float y = base.settings.filtering.value.y;
			y = Mathf.Clamp(y, 1.01f, 99f);
			x = Mathf.Clamp(x, 1f, y - 0.01f);
			float value = base.settings.minLuminance.value;
			float value2 = base.settings.maxLuminance.value;
			base.settings.minLuminance.value = Mathf.Min(value, value2);
			base.settings.maxLuminance.value = Mathf.Max(value, value2);
			bool num = m_ResetHistory || !Application.isPlaying;
			string text = null;
			text = ((!num && base.settings.eyeAdaptation.value != EyeAdaptation.Fixed) ? "KAutoExposureAvgLuminance_progressive" : "KAutoExposureAvgLuminance_fixed");
			ComputeShader autoExposure = context.resources.computeShaders.autoExposure;
			int kernelIndex = autoExposure.FindKernel(text);
			command.SetComputeBufferParam(autoExposure, kernelIndex, "_HistogramBuffer", context.logHistogram.data);
			command.SetComputeVectorParam(autoExposure, "_Params1", new Vector4(x * 0.01f, y * 0.01f, RuntimeUtilities.Exp2(base.settings.minLuminance.value), RuntimeUtilities.Exp2(base.settings.maxLuminance.value)));
			command.SetComputeVectorParam(autoExposure, "_Params2", new Vector4(base.settings.speedDown.value, base.settings.speedUp.value, base.settings.keyValue.value, Time.deltaTime));
			command.SetComputeVectorParam(autoExposure, "_ScaleOffsetRes", context.logHistogram.GetHistogramScaleOffsetRes(context));
			if (num)
			{
				m_CurrentAutoExposure = m_AutoExposurePool[context.xrActiveEye][0];
				command.SetComputeTextureParam(autoExposure, kernelIndex, "_Destination", m_CurrentAutoExposure);
				command.DispatchCompute(autoExposure, kernelIndex, 1, 1, 1);
				RuntimeUtilities.CopyTexture(command, m_AutoExposurePool[context.xrActiveEye][0], m_AutoExposurePool[context.xrActiveEye][1]);
				m_ResetHistory = false;
			}
			else
			{
				int num2 = m_AutoExposurePingPong[context.xrActiveEye];
				RenderTexture renderTexture = m_AutoExposurePool[context.xrActiveEye][++num2 % 2];
				RenderTexture renderTexture2 = m_AutoExposurePool[context.xrActiveEye][++num2 % 2];
				command.SetComputeTextureParam(autoExposure, kernelIndex, "_Source", renderTexture);
				command.SetComputeTextureParam(autoExposure, kernelIndex, "_Destination", renderTexture2);
				command.DispatchCompute(autoExposure, kernelIndex, 1, 1, 1);
				m_AutoExposurePingPong[context.xrActiveEye] = ++num2 % 2;
				m_CurrentAutoExposure = renderTexture2;
			}
			command.EndSample("AutoExposureLookup");
			context.autoExposureTexture = m_CurrentAutoExposure;
			context.autoExposure = base.settings;
		}

		public override void Release()
		{
			RenderTexture[][] autoExposurePool = m_AutoExposurePool;
			foreach (RenderTexture[] array in autoExposurePool)
			{
				for (int j = 0; j < array.Length; j++)
				{
					RuntimeUtilities.Destroy(array[j]);
				}
			}
		}
	}
}
