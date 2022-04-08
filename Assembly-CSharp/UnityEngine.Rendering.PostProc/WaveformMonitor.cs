using System;

namespace UnityEngine.Rendering.PostProcessing
{
	[Serializable]
	public sealed class WaveformMonitor : Monitor
	{
		public float exposure = 0.12f;

		public int height = 256;

		private ComputeBuffer m_Data;

		private const int k_ThreadGroupSize = 256;

		private const int k_ThreadGroupSizeX = 16;

		private const int k_ThreadGroupSizeY = 16;

		internal override void OnDisable()
		{
			base.OnDisable();
			if (m_Data != null)
			{
				m_Data.Release();
			}
			m_Data = null;
		}

		internal override bool NeedsHalfRes()
		{
			return true;
		}

		internal override bool ShaderResourcesAvailable(PostProcessRenderContext context)
		{
			return context.resources.computeShaders.waveform;
		}

		internal override void Render(PostProcessRenderContext context)
		{
			float num = (float)context.width / 2f / ((float)context.height / 2f);
			int num2 = Mathf.FloorToInt((float)height * num);
			CheckOutput(num2, height);
			exposure = Mathf.Max(0f, exposure);
			int num3 = num2 * height;
			if (m_Data == null)
			{
				m_Data = new ComputeBuffer(num3, 16);
			}
			else if (m_Data.count < num3)
			{
				m_Data.Release();
				m_Data = new ComputeBuffer(num3, 16);
			}
			ComputeShader waveform = context.resources.computeShaders.waveform;
			CommandBuffer command = context.command;
			command.BeginSample("Waveform");
			Vector4 val = new Vector4(num2, height, RuntimeUtilities.isLinearColorSpace ? 1 : 0, 0f);
			int kernelIndex = waveform.FindKernel("KWaveformClear");
			command.SetComputeBufferParam(waveform, kernelIndex, "_WaveformBuffer", m_Data);
			command.SetComputeVectorParam(waveform, "_Params", val);
			command.DispatchCompute(waveform, kernelIndex, Mathf.CeilToInt((float)num2 / 16f), Mathf.CeilToInt((float)height / 16f), 1);
			command.GetTemporaryRT(ShaderIDs.WaveformSource, num2, height, 0, FilterMode.Bilinear, context.sourceFormat);
			RuntimeUtilities.BlitFullscreenTriangle(command, ShaderIDs.HalfResFinalCopy, ShaderIDs.WaveformSource);
			kernelIndex = waveform.FindKernel("KWaveformGather");
			command.SetComputeBufferParam(waveform, kernelIndex, "_WaveformBuffer", m_Data);
			command.SetComputeTextureParam(waveform, kernelIndex, "_Source", ShaderIDs.WaveformSource);
			command.SetComputeVectorParam(waveform, "_Params", val);
			command.DispatchCompute(waveform, kernelIndex, num2, Mathf.CeilToInt((float)height / 256f), 1);
			command.ReleaseTemporaryRT(ShaderIDs.WaveformSource);
			PropertySheet propertySheet = context.propertySheets.Get(context.resources.shaders.waveform);
			propertySheet.properties.SetVector(ShaderIDs.Params, new Vector4(num2, height, exposure, 0f));
			propertySheet.properties.SetBuffer(ShaderIDs.WaveformBuffer, m_Data);
			RuntimeUtilities.BlitFullscreenTriangle(command, BuiltinRenderTextureType.None, base.output, propertySheet, 0);
			command.EndSample("Waveform");
		}
	}
}
