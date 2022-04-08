using System;

namespace UnityEngine.Rendering.PostProcessing
{
	[Serializable]
	public sealed class LightMeterMonitor : Monitor
	{
		public int width = 512;

		public int height = 256;

		public bool showCurves = true;

		internal override bool ShaderResourcesAvailable(PostProcessRenderContext context)
		{
			if ((bool)context.resources.shaders.lightMeter)
			{
				return context.resources.shaders.lightMeter.isSupported;
			}
			return false;
		}

		internal override void Render(PostProcessRenderContext context)
		{
			CheckOutput(width, height);
			LogHistogram logHistogram = context.logHistogram;
			PropertySheet propertySheet = context.propertySheets.Get(context.resources.shaders.lightMeter);
			propertySheet.ClearKeywords();
			propertySheet.properties.SetBuffer(ShaderIDs.HistogramBuffer, logHistogram.data);
			Vector4 histogramScaleOffsetRes = logHistogram.GetHistogramScaleOffsetRes(context);
			histogramScaleOffsetRes.z = 1f / (float)width;
			histogramScaleOffsetRes.w = 1f / (float)height;
			propertySheet.properties.SetVector(ShaderIDs.ScaleOffsetRes, histogramScaleOffsetRes);
			if (context.logLut != null && showCurves)
			{
				propertySheet.EnableKeyword("COLOR_GRADING_HDR");
				propertySheet.properties.SetTexture(ShaderIDs.Lut3D, context.logLut);
			}
			AutoExposure autoExposure = context.autoExposure;
			if (autoExposure != null)
			{
				float x = autoExposure.filtering.value.x;
				float y = autoExposure.filtering.value.y;
				y = Mathf.Clamp(y, 1.01f, 99f);
				x = Mathf.Clamp(x, 1f, y - 0.01f);
				Vector4 value = new Vector4(x * 0.01f, y * 0.01f, RuntimeUtilities.Exp2(autoExposure.minLuminance.value), RuntimeUtilities.Exp2(autoExposure.maxLuminance.value));
				propertySheet.EnableKeyword("AUTO_EXPOSURE");
				propertySheet.properties.SetVector(ShaderIDs.Params, value);
			}
			CommandBuffer command = context.command;
			command.BeginSample("LightMeter");
			RuntimeUtilities.BlitFullscreenTriangle(command, BuiltinRenderTextureType.None, base.output, propertySheet, 0);
			command.EndSample("LightMeter");
		}
	}
}
