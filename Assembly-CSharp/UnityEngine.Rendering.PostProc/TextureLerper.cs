#define UNITY_ASSERTIONS
using System.Collections.Generic;
using UnityEngine.Assertions;

namespace UnityEngine.Rendering.PostProcessing
{
	internal class TextureLerper
	{
		private static TextureLerper m_Instance;

		private CommandBuffer m_Command;

		private PropertySheetFactory m_PropertySheets;

		private PostProcessResources m_Resources;

		private List<RenderTexture> m_Recycled;

		private List<RenderTexture> m_Actives;

		internal static TextureLerper instance
		{
			get
			{
				if (m_Instance == null)
				{
					m_Instance = new TextureLerper();
				}
				return m_Instance;
			}
		}

		private TextureLerper()
		{
			m_Recycled = new List<RenderTexture>();
			m_Actives = new List<RenderTexture>();
		}

		internal void BeginFrame(PostProcessRenderContext context)
		{
			m_Command = context.command;
			m_PropertySheets = context.propertySheets;
			m_Resources = context.resources;
		}

		internal void EndFrame()
		{
			if (m_Recycled.Count > 0)
			{
				foreach (RenderTexture item in m_Recycled)
				{
					RuntimeUtilities.Destroy(item);
				}
				m_Recycled.Clear();
			}
			if (m_Actives.Count <= 0)
			{
				return;
			}
			foreach (RenderTexture active in m_Actives)
			{
				m_Recycled.Add(active);
			}
			m_Actives.Clear();
		}

		private RenderTexture Get(RenderTextureFormat format, int w, int h, int d = 1, bool enableRandomWrite = false, bool force3D = false)
		{
			RenderTexture renderTexture = null;
			int count = m_Recycled.Count;
			int i;
			for (i = 0; i < count; i++)
			{
				RenderTexture renderTexture2 = m_Recycled[i];
				if (renderTexture2.width == w && renderTexture2.height == h && renderTexture2.volumeDepth == d && renderTexture2.format == format && renderTexture2.enableRandomWrite == enableRandomWrite && (!force3D || renderTexture2.dimension == TextureDimension.Tex3D))
				{
					renderTexture = renderTexture2;
					break;
				}
			}
			if (renderTexture == null)
			{
				TextureDimension dimension = ((d > 1 || force3D) ? TextureDimension.Tex3D : TextureDimension.Tex2D);
				renderTexture = new RenderTexture(w, h, 0, format)
				{
					dimension = dimension,
					filterMode = FilterMode.Bilinear,
					wrapMode = TextureWrapMode.Clamp,
					anisoLevel = 0,
					volumeDepth = d,
					enableRandomWrite = enableRandomWrite
				};
				renderTexture.Create();
			}
			else
			{
				m_Recycled.RemoveAt(i);
			}
			m_Actives.Add(renderTexture);
			return renderTexture;
		}

		internal Texture Lerp(Texture from, Texture to, float t)
		{
			Assert.IsNotNull(from);
			Assert.IsNotNull(to);
			Assert.AreEqual(from.width, to.width);
			Assert.AreEqual(from.height, to.height);
			if (from == to)
			{
				return from;
			}
			if (t <= 0f)
			{
				return from;
			}
			if (t >= 1f)
			{
				return to;
			}
			RenderTexture renderTexture;
			if (from is Texture3D || (from is RenderTexture && ((RenderTexture)from).volumeDepth > 1))
			{
				int num = ((from is Texture3D) ? ((Texture3D)from).depth : ((RenderTexture)from).volumeDepth);
				int num2 = Mathf.Max(Mathf.Max(from.width, from.height), num);
				renderTexture = Get(RenderTextureFormat.ARGBHalf, from.width, from.height, num, true, true);
				ComputeShader texture3dLerp = m_Resources.computeShaders.texture3dLerp;
				int kernelIndex = texture3dLerp.FindKernel("KTexture3DLerp");
				m_Command.SetComputeVectorParam(texture3dLerp, "_DimensionsAndLerp", new Vector4(from.width, from.height, num, t));
				m_Command.SetComputeTextureParam(texture3dLerp, kernelIndex, "_Output", renderTexture);
				m_Command.SetComputeTextureParam(texture3dLerp, kernelIndex, "_From", from);
				m_Command.SetComputeTextureParam(texture3dLerp, kernelIndex, "_To", to);
				uint x;
				uint y;
				uint z;
				texture3dLerp.GetKernelThreadGroupSizes(kernelIndex, out x, out y, out z);
				Assert.AreEqual(x, y);
				int num3 = Mathf.CeilToInt((float)num2 / (float)x);
				int threadGroupsZ = Mathf.CeilToInt((float)num2 / (float)z);
				m_Command.DispatchCompute(texture3dLerp, kernelIndex, num3, num3, threadGroupsZ);
				return renderTexture;
			}
			RenderTextureFormat uncompressedRenderTextureFormat = TextureFormatUtilities.GetUncompressedRenderTextureFormat(to);
			renderTexture = Get(uncompressedRenderTextureFormat, to.width, to.height);
			PropertySheet propertySheet = m_PropertySheets.Get(m_Resources.shaders.texture2dLerp);
			propertySheet.properties.SetTexture(ShaderIDs.To, to);
			propertySheet.properties.SetFloat(ShaderIDs.Interp, t);
			RuntimeUtilities.BlitFullscreenTriangle(m_Command, from, renderTexture, propertySheet, 0);
			return renderTexture;
		}

		internal Texture Lerp(Texture from, Color to, float t)
		{
			Assert.IsNotNull(from);
			if ((double)t < 1E-05)
			{
				return from;
			}
			RenderTexture renderTexture;
			if (from is Texture3D || (from is RenderTexture && ((RenderTexture)from).volumeDepth > 1))
			{
				int num = ((from is Texture3D) ? ((Texture3D)from).depth : ((RenderTexture)from).volumeDepth);
				int num2 = Mathf.Max(Mathf.Max(from.width, from.height), num);
				renderTexture = Get(RenderTextureFormat.ARGBHalf, from.width, from.height, num, true, true);
				ComputeShader texture3dLerp = m_Resources.computeShaders.texture3dLerp;
				int kernelIndex = texture3dLerp.FindKernel("KTexture3DLerpToColor");
				m_Command.SetComputeVectorParam(texture3dLerp, "_DimensionsAndLerp", new Vector4(from.width, from.height, num, t));
				m_Command.SetComputeVectorParam(texture3dLerp, "_TargetColor", new Vector4(to.r, to.g, to.b, to.a));
				m_Command.SetComputeTextureParam(texture3dLerp, kernelIndex, "_Output", renderTexture);
				m_Command.SetComputeTextureParam(texture3dLerp, kernelIndex, "_From", from);
				int num3 = Mathf.CeilToInt((float)num2 / 4f);
				m_Command.DispatchCompute(texture3dLerp, kernelIndex, num3, num3, num3);
				return renderTexture;
			}
			RenderTextureFormat uncompressedRenderTextureFormat = TextureFormatUtilities.GetUncompressedRenderTextureFormat(from);
			renderTexture = Get(uncompressedRenderTextureFormat, from.width, from.height);
			PropertySheet propertySheet = m_PropertySheets.Get(m_Resources.shaders.texture2dLerp);
			propertySheet.properties.SetVector(ShaderIDs.TargetColor, new Vector4(to.r, to.g, to.b, to.a));
			propertySheet.properties.SetFloat(ShaderIDs.Interp, t);
			RuntimeUtilities.BlitFullscreenTriangle(m_Command, from, renderTexture, propertySheet, 1);
			return renderTexture;
		}

		internal void Clear()
		{
			foreach (RenderTexture active in m_Actives)
			{
				RuntimeUtilities.Destroy(active);
			}
			foreach (RenderTexture item in m_Recycled)
			{
				RuntimeUtilities.Destroy(item);
			}
			m_Actives.Clear();
			m_Recycled.Clear();
		}
	}
}
