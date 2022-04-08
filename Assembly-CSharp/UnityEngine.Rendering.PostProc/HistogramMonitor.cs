using System;

namespace UnityEngine.Rendering.PostProcessing
{
	[Serializable]
	public sealed class HistogramMonitor : Monitor
	{
		public enum Channel
		{
			Red = 0,
			Green = 1,
			Blue = 2,
			Master = 3
		}

		public int width = 512;

		public int height = 256;

		public Channel channel = Channel.Master;

		private ComputeBuffer m_Data;

		private const int k_NumBins = 256;

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
			return context.resources.computeShaders.gammaHistogram;
		}

		internal override void Render(PostProcessRenderContext context)
		{
			CheckOutput(width, height);
			if (m_Data == null)
			{
				m_Data = new ComputeBuffer(256, 4);
			}
			ComputeShader gammaHistogram = context.resources.computeShaders.gammaHistogram;
			CommandBuffer command = context.command;
			command.BeginSample("GammaHistogram");
			int kernelIndex = gammaHistogram.FindKernel("KHistogramClear");
			command.SetComputeBufferParam(gammaHistogram, kernelIndex, "_HistogramBuffer", m_Data);
			command.DispatchCompute(gammaHistogram, kernelIndex, Mathf.CeilToInt(16f), 1, 1);
			kernelIndex = gammaHistogram.FindKernel("KHistogramGather");
			Vector4 val = new Vector4(context.width / 2, context.height / 2, RuntimeUtilities.isLinearColorSpace ? 1 : 0, (float)channel);
			command.SetComputeVectorParam(gammaHistogram, "_Params", val);
			command.SetComputeTextureParam(gammaHistogram, kernelIndex, "_Source", ShaderIDs.HalfResFinalCopy);
			command.SetComputeBufferParam(gammaHistogram, kernelIndex, "_HistogramBuffer", m_Data);
			command.DispatchCompute(gammaHistogram, kernelIndex, Mathf.CeilToInt(val.x / 16f), Mathf.CeilToInt(val.y / 16f), 1);
			PropertySheet propertySheet = context.propertySheets.Get(context.resources.shaders.gammaHistogram);
			propertySheet.properties.SetVector(ShaderIDs.Params, new Vector4(width, height, 0f, 0f));
			propertySheet.properties.SetBuffer(ShaderIDs.HistogramBuffer, m_Data);
			RuntimeUtilities.BlitFullscreenTriangle(command, BuiltinRenderTextureType.None, base.output, propertySheet, 0);
			command.EndSample("GammaHistogram");
		}
	}
}
