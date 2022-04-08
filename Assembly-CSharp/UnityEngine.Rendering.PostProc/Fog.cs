using System;
using UnityEngine.Scripting;

namespace UnityEngine.Rendering.PostProcessing
{
	[Serializable]
	[Preserve]
	public sealed class Fog
	{
		[Tooltip("Enables the internal deferred fog pass. Actual fog settings should be set in the Lighting panel.")]
		public bool enabled = true;

		[Tooltip("Mark true for the fog to ignore the skybox")]
		public bool excludeSkybox = true;

		internal DepthTextureMode GetCameraFlags()
		{
			return DepthTextureMode.Depth;
		}

		internal bool IsEnabledAndSupported(PostProcessRenderContext context)
		{
			if (enabled && RenderSettings.fog && !RuntimeUtilities.scriptableRenderPipelineActive && (bool)context.resources.shaders.deferredFog && context.resources.shaders.deferredFog.isSupported)
			{
				return context.camera.actualRenderingPath == RenderingPath.DeferredShading;
			}
			return false;
		}

		internal void Render(PostProcessRenderContext context)
		{
			PropertySheet propertySheet = context.propertySheets.Get(context.resources.shaders.deferredFog);
			propertySheet.ClearKeywords();
			Color color = (RuntimeUtilities.isLinearColorSpace ? RenderSettings.fogColor.linear : RenderSettings.fogColor);
			propertySheet.properties.SetVector(ShaderIDs.FogColor, color);
			propertySheet.properties.SetVector(ShaderIDs.FogParams, new Vector3(RenderSettings.fogDensity, RenderSettings.fogStartDistance, RenderSettings.fogEndDistance));
			RuntimeUtilities.BlitFullscreenTriangle(context.command, context.source, context.destination, propertySheet, excludeSkybox ? 1 : 0);
		}
	}
}
