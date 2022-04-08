#define UNITY_ASSERTIONS
using UnityEngine.Assertions;
using UnityEngine.Scripting;

namespace UnityEngine.Rendering.PostProcessing
{
	[Preserve]
	internal sealed class ScreenSpaceReflectionsRenderer : PostProcessEffectRenderer<ScreenSpaceReflections>
	{
		private class QualityPreset
		{
			public int maximumIterationCount;

			public float thickness;

			public ScreenSpaceReflectionResolution downsampling;
		}

		private enum Pass
		{
			Test = 0,
			Resolve = 1,
			Reproject = 2,
			Composite = 3
		}

		private RenderTexture m_Resolve;

		private RenderTexture m_History;

		private int[] m_MipIDs;

		private readonly QualityPreset[] m_Presets = new QualityPreset[7]
		{
			new QualityPreset
			{
				maximumIterationCount = 10,
				thickness = 32f,
				downsampling = ScreenSpaceReflectionResolution.Downsampled
			},
			new QualityPreset
			{
				maximumIterationCount = 16,
				thickness = 32f,
				downsampling = ScreenSpaceReflectionResolution.Downsampled
			},
			new QualityPreset
			{
				maximumIterationCount = 32,
				thickness = 16f,
				downsampling = ScreenSpaceReflectionResolution.Downsampled
			},
			new QualityPreset
			{
				maximumIterationCount = 48,
				thickness = 8f,
				downsampling = ScreenSpaceReflectionResolution.Downsampled
			},
			new QualityPreset
			{
				maximumIterationCount = 16,
				thickness = 32f,
				downsampling = ScreenSpaceReflectionResolution.FullSize
			},
			new QualityPreset
			{
				maximumIterationCount = 48,
				thickness = 16f,
				downsampling = ScreenSpaceReflectionResolution.FullSize
			},
			new QualityPreset
			{
				maximumIterationCount = 128,
				thickness = 12f,
				downsampling = ScreenSpaceReflectionResolution.Supersampled
			}
		};

		public override DepthTextureMode GetCameraFlags()
		{
			return DepthTextureMode.Depth | DepthTextureMode.MotionVectors;
		}

		internal void CheckRT(ref RenderTexture rt, int width, int height, FilterMode filterMode, bool useMipMap)
		{
			if (rt == null || !rt.IsCreated() || rt.width != width || rt.height != height)
			{
				if (rt != null)
				{
					rt.Release();
					RuntimeUtilities.Destroy(rt);
				}
				rt = new RenderTexture(width, height, 0, RuntimeUtilities.defaultHDRRenderTextureFormat)
				{
					filterMode = filterMode,
					useMipMap = useMipMap,
					autoGenerateMips = false,
					hideFlags = HideFlags.HideAndDontSave
				};
				rt.Create();
			}
		}

		public override void Render(PostProcessRenderContext context)
		{
			CommandBuffer command = context.command;
			command.BeginSample("Screen-space Reflections");
			if (base.settings.preset.value != ScreenSpaceReflectionPreset.Custom)
			{
				int value = (int)base.settings.preset.value;
				base.settings.maximumIterationCount.value = m_Presets[value].maximumIterationCount;
				base.settings.thickness.value = m_Presets[value].thickness;
				base.settings.resolution.value = m_Presets[value].downsampling;
			}
			base.settings.maximumMarchDistance.value = Mathf.Max(0f, base.settings.maximumMarchDistance.value);
			int num = Mathf.ClosestPowerOfTwo(Mathf.Min(context.width, context.height));
			if (base.settings.resolution.value == ScreenSpaceReflectionResolution.Downsampled)
			{
				num >>= 1;
			}
			else if (base.settings.resolution.value == ScreenSpaceReflectionResolution.Supersampled)
			{
				num <<= 1;
			}
			int a = Mathf.FloorToInt(Mathf.Log(num, 2f) - 3f);
			a = Mathf.Min(a, 12);
			CheckRT(ref m_Resolve, num, num, FilterMode.Trilinear, true);
			Texture2D texture2D = context.resources.blueNoise256[0];
			PropertySheet propertySheet = context.propertySheets.Get(context.resources.shaders.screenSpaceReflections);
			propertySheet.properties.SetTexture(ShaderIDs.Noise, texture2D);
			Matrix4x4 value2 = default(Matrix4x4);
			value2.SetRow(0, new Vector4((float)num * 0.5f, 0f, 0f, (float)num * 0.5f));
			value2.SetRow(1, new Vector4(0f, (float)num * 0.5f, 0f, (float)num * 0.5f));
			value2.SetRow(2, new Vector4(0f, 0f, 1f, 0f));
			value2.SetRow(3, new Vector4(0f, 0f, 0f, 1f));
			Matrix4x4 gPUProjectionMatrix = GL.GetGPUProjectionMatrix(context.camera.projectionMatrix, false);
			value2 *= gPUProjectionMatrix;
			propertySheet.properties.SetMatrix(ShaderIDs.ViewMatrix, context.camera.worldToCameraMatrix);
			propertySheet.properties.SetMatrix(ShaderIDs.InverseViewMatrix, context.camera.worldToCameraMatrix.inverse);
			propertySheet.properties.SetMatrix(ShaderIDs.InverseProjectionMatrix, gPUProjectionMatrix.inverse);
			propertySheet.properties.SetMatrix(ShaderIDs.ScreenSpaceProjectionMatrix, value2);
			propertySheet.properties.SetVector(ShaderIDs.Params, new Vector4(base.settings.vignette.value, base.settings.distanceFade.value, base.settings.maximumMarchDistance.value, a));
			propertySheet.properties.SetVector(ShaderIDs.Params2, new Vector4((float)context.width / (float)context.height, (float)num / (float)texture2D.width, base.settings.thickness.value, base.settings.maximumIterationCount.value));
			command.GetTemporaryRT(ShaderIDs.Test, num, num, 0, FilterMode.Point, context.sourceFormat);
			RuntimeUtilities.BlitFullscreenTriangle(command, context.source, ShaderIDs.Test, propertySheet, 0);
			if (context.isSceneView)
			{
				RuntimeUtilities.BlitFullscreenTriangle(command, context.source, m_Resolve, propertySheet, 1);
			}
			else
			{
				CheckRT(ref m_History, num, num, FilterMode.Bilinear, false);
				if (m_ResetHistory)
				{
					RuntimeUtilities.BlitFullscreenTriangle(context.command, context.source, m_History);
					m_ResetHistory = false;
				}
				command.GetTemporaryRT(ShaderIDs.SSRResolveTemp, num, num, 0, FilterMode.Bilinear, context.sourceFormat);
				RuntimeUtilities.BlitFullscreenTriangle(command, context.source, ShaderIDs.SSRResolveTemp, propertySheet, 1);
				propertySheet.properties.SetTexture(ShaderIDs.History, m_History);
				RuntimeUtilities.BlitFullscreenTriangle(command, ShaderIDs.SSRResolveTemp, m_Resolve, propertySheet, 2);
				command.CopyTexture(m_Resolve, 0, 0, m_History, 0, 0);
				command.ReleaseTemporaryRT(ShaderIDs.SSRResolveTemp);
			}
			command.ReleaseTemporaryRT(ShaderIDs.Test);
			if (m_MipIDs == null || m_MipIDs.Length == 0)
			{
				m_MipIDs = new int[12];
				for (int i = 0; i < 12; i++)
				{
					m_MipIDs[i] = Shader.PropertyToID("_SSRGaussianMip" + i);
				}
			}
			ComputeShader gaussianDownsample = context.resources.computeShaders.gaussianDownsample;
			int kernelIndex = gaussianDownsample.FindKernel("KMain");
			RenderTargetIdentifier rt = new RenderTargetIdentifier(m_Resolve);
			for (int j = 0; j < a; j++)
			{
				num >>= 1;
				Assert.IsTrue(num > 0);
				command.GetTemporaryRT(m_MipIDs[j], num, num, 0, FilterMode.Bilinear, context.sourceFormat, RenderTextureReadWrite.Default, 1, true);
				command.SetComputeTextureParam(gaussianDownsample, kernelIndex, "_Source", rt);
				command.SetComputeTextureParam(gaussianDownsample, kernelIndex, "_Result", m_MipIDs[j]);
				command.SetComputeVectorParam(gaussianDownsample, "_Size", new Vector4(num, num, 1f / (float)num, 1f / (float)num));
				command.DispatchCompute(gaussianDownsample, kernelIndex, num / 8, num / 8, 1);
				command.CopyTexture(m_MipIDs[j], 0, 0, m_Resolve, 0, j + 1);
				rt = m_MipIDs[j];
			}
			for (int k = 0; k < a; k++)
			{
				command.ReleaseTemporaryRT(m_MipIDs[k]);
			}
			propertySheet.properties.SetTexture(ShaderIDs.Resolve, m_Resolve);
			RuntimeUtilities.BlitFullscreenTriangle(command, context.source, context.destination, propertySheet, 3);
			command.EndSample("Screen-space Reflections");
		}

		public override void Release()
		{
			RuntimeUtilities.Destroy(m_Resolve);
			RuntimeUtilities.Destroy(m_History);
			m_Resolve = null;
			m_History = null;
		}
	}
}
