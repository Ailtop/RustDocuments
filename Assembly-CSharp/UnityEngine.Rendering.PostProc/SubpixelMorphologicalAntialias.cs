using System;
using UnityEngine.Scripting;

namespace UnityEngine.Rendering.PostProcessing
{
	[Serializable]
	[Preserve]
	public sealed class SubpixelMorphologicalAntialiasing
	{
		private enum Pass
		{
			EdgeDetection = 0,
			BlendWeights = 3,
			NeighborhoodBlending = 6
		}

		public enum Quality
		{
			Low = 0,
			Medium = 1,
			High = 2
		}

		[Tooltip("Lower quality is faster at the expense of visual quality (Low = ~60%, Medium = ~80%).")]
		public Quality quality = Quality.High;

		public bool IsSupported()
		{
			return !RuntimeUtilities.isSinglePassStereoEnabled;
		}

		internal void Render(PostProcessRenderContext context)
		{
			PropertySheet propertySheet = context.propertySheets.Get(context.resources.shaders.subpixelMorphologicalAntialiasing);
			propertySheet.properties.SetTexture("_AreaTex", context.resources.smaaLuts.area);
			propertySheet.properties.SetTexture("_SearchTex", context.resources.smaaLuts.search);
			CommandBuffer command = context.command;
			command.BeginSample("SubpixelMorphologicalAntialiasing");
			command.GetTemporaryRT(ShaderIDs.SMAA_Flip, context.width, context.height, 0, FilterMode.Bilinear, context.sourceFormat, RenderTextureReadWrite.Linear);
			command.GetTemporaryRT(ShaderIDs.SMAA_Flop, context.width, context.height, 0, FilterMode.Bilinear, context.sourceFormat, RenderTextureReadWrite.Linear);
			RuntimeUtilities.BlitFullscreenTriangle(command, context.source, ShaderIDs.SMAA_Flip, propertySheet, (int)quality, true);
			RuntimeUtilities.BlitFullscreenTriangle(command, ShaderIDs.SMAA_Flip, ShaderIDs.SMAA_Flop, propertySheet, (int)(3 + quality));
			command.SetGlobalTexture("_BlendTex", ShaderIDs.SMAA_Flop);
			RuntimeUtilities.BlitFullscreenTriangle(command, context.source, context.destination, propertySheet, 6);
			command.ReleaseTemporaryRT(ShaderIDs.SMAA_Flip);
			command.ReleaseTemporaryRT(ShaderIDs.SMAA_Flop);
			command.EndSample("SubpixelMorphologicalAntialiasing");
		}
	}
}
