using System;
using System.Collections.Generic;

namespace UnityEngine.Rendering.PostProcessing
{
	[Serializable]
	public sealed class PostProcessDebugLayer
	{
		[Serializable]
		public class OverlaySettings
		{
			public bool linearDepth;

			[Range(0f, 16f)]
			public float motionColorIntensity = 4f;

			[Range(4f, 128f)]
			public int motionGridSize = 64;

			public ColorBlindnessType colorBlindnessType;

			[Range(0f, 1f)]
			public float colorBlindnessStrength = 1f;
		}

		public LightMeterMonitor lightMeter;

		public HistogramMonitor histogram;

		public WaveformMonitor waveform;

		public VectorscopeMonitor vectorscope;

		private Dictionary<MonitorType, Monitor> m_Monitors;

		private int frameWidth;

		private int frameHeight;

		public OverlaySettings overlaySettings;

		public RenderTexture debugOverlayTarget { get; private set; }

		public bool debugOverlayActive { get; private set; }

		public DebugOverlay debugOverlay { get; private set; }

		internal void OnEnable()
		{
			RuntimeUtilities.CreateIfNull(ref lightMeter);
			RuntimeUtilities.CreateIfNull(ref histogram);
			RuntimeUtilities.CreateIfNull(ref waveform);
			RuntimeUtilities.CreateIfNull(ref vectorscope);
			RuntimeUtilities.CreateIfNull(ref overlaySettings);
			m_Monitors = new Dictionary<MonitorType, Monitor>
			{
				{
					MonitorType.LightMeter,
					lightMeter
				},
				{
					MonitorType.Histogram,
					histogram
				},
				{
					MonitorType.Waveform,
					waveform
				},
				{
					MonitorType.Vectorscope,
					vectorscope
				}
			};
			foreach (KeyValuePair<MonitorType, Monitor> monitor in m_Monitors)
			{
				monitor.Value.OnEnable();
			}
		}

		internal void OnDisable()
		{
			foreach (KeyValuePair<MonitorType, Monitor> monitor in m_Monitors)
			{
				monitor.Value.OnDisable();
			}
			DestroyDebugOverlayTarget();
		}

		private void DestroyDebugOverlayTarget()
		{
			RuntimeUtilities.Destroy(debugOverlayTarget);
			debugOverlayTarget = null;
		}

		public void RequestMonitorPass(MonitorType monitor)
		{
			m_Monitors[monitor].requested = true;
		}

		public void RequestDebugOverlay(DebugOverlay mode)
		{
			debugOverlay = mode;
		}

		internal void SetFrameSize(int width, int height)
		{
			frameWidth = width;
			frameHeight = height;
			debugOverlayActive = false;
		}

		public void PushDebugOverlay(CommandBuffer cmd, RenderTargetIdentifier source, PropertySheet sheet, int pass)
		{
			if (debugOverlayTarget == null || !debugOverlayTarget.IsCreated() || debugOverlayTarget.width != frameWidth || debugOverlayTarget.height != frameHeight)
			{
				RuntimeUtilities.Destroy(debugOverlayTarget);
				debugOverlayTarget = new RenderTexture(frameWidth, frameHeight, 0, RenderTextureFormat.ARGB32)
				{
					name = "Debug Overlay Target",
					anisoLevel = 1,
					filterMode = FilterMode.Bilinear,
					wrapMode = TextureWrapMode.Clamp,
					hideFlags = HideFlags.HideAndDontSave
				};
				debugOverlayTarget.Create();
			}
			RuntimeUtilities.BlitFullscreenTriangle(cmd, source, debugOverlayTarget, sheet, pass);
			debugOverlayActive = true;
		}

		internal DepthTextureMode GetCameraFlags()
		{
			if (debugOverlay == DebugOverlay.Depth)
			{
				return DepthTextureMode.Depth;
			}
			if (debugOverlay == DebugOverlay.Normals)
			{
				return DepthTextureMode.DepthNormals;
			}
			if (debugOverlay == DebugOverlay.MotionVectors)
			{
				return DepthTextureMode.Depth | DepthTextureMode.MotionVectors;
			}
			return DepthTextureMode.None;
		}

		internal void RenderMonitors(PostProcessRenderContext context)
		{
			bool flag = false;
			bool flag2 = false;
			foreach (KeyValuePair<MonitorType, Monitor> monitor in m_Monitors)
			{
				bool flag3 = monitor.Value.IsRequestedAndSupported(context);
				flag = flag || flag3;
				flag2 |= flag3 && monitor.Value.NeedsHalfRes();
			}
			if (!flag)
			{
				return;
			}
			CommandBuffer command = context.command;
			command.BeginSample("Monitors");
			if (flag2)
			{
				command.GetTemporaryRT(ShaderIDs.HalfResFinalCopy, context.width / 2, context.height / 2, 0, FilterMode.Bilinear, context.sourceFormat);
				command.Blit(context.destination, ShaderIDs.HalfResFinalCopy);
			}
			foreach (KeyValuePair<MonitorType, Monitor> monitor2 in m_Monitors)
			{
				Monitor value = monitor2.Value;
				if (value.requested)
				{
					value.Render(context);
				}
			}
			if (flag2)
			{
				command.ReleaseTemporaryRT(ShaderIDs.HalfResFinalCopy);
			}
			command.EndSample("Monitors");
		}

		internal void RenderSpecialOverlays(PostProcessRenderContext context)
		{
			if (debugOverlay == DebugOverlay.Depth)
			{
				PropertySheet propertySheet = context.propertySheets.Get(context.resources.shaders.debugOverlays);
				propertySheet.properties.SetVector(ShaderIDs.Params, new Vector4(overlaySettings.linearDepth ? 1f : 0f, 0f, 0f, 0f));
				PushDebugOverlay(context.command, BuiltinRenderTextureType.None, propertySheet, 0);
			}
			else if (debugOverlay == DebugOverlay.Normals)
			{
				PropertySheet propertySheet2 = context.propertySheets.Get(context.resources.shaders.debugOverlays);
				propertySheet2.ClearKeywords();
				if (context.camera.actualRenderingPath == RenderingPath.DeferredLighting)
				{
					propertySheet2.EnableKeyword("SOURCE_GBUFFER");
				}
				PushDebugOverlay(context.command, BuiltinRenderTextureType.None, propertySheet2, 1);
			}
			else if (debugOverlay == DebugOverlay.MotionVectors)
			{
				PropertySheet propertySheet3 = context.propertySheets.Get(context.resources.shaders.debugOverlays);
				propertySheet3.properties.SetVector(ShaderIDs.Params, new Vector4(overlaySettings.motionColorIntensity, overlaySettings.motionGridSize, 0f, 0f));
				PushDebugOverlay(context.command, context.source, propertySheet3, 2);
			}
			else if (debugOverlay == DebugOverlay.NANTracker)
			{
				PropertySheet sheet = context.propertySheets.Get(context.resources.shaders.debugOverlays);
				PushDebugOverlay(context.command, context.source, sheet, 3);
			}
			else if (debugOverlay == DebugOverlay.ColorBlindnessSimulation)
			{
				PropertySheet propertySheet4 = context.propertySheets.Get(context.resources.shaders.debugOverlays);
				propertySheet4.properties.SetVector(ShaderIDs.Params, new Vector4(overlaySettings.colorBlindnessStrength, 0f, 0f, 0f));
				PushDebugOverlay(context.command, context.source, propertySheet4, (int)(4 + overlaySettings.colorBlindnessType));
			}
		}

		internal void EndFrame()
		{
			foreach (KeyValuePair<MonitorType, Monitor> monitor in m_Monitors)
			{
				monitor.Value.requested = false;
			}
			if (!debugOverlayActive)
			{
				DestroyDebugOverlayTarget();
			}
			debugOverlay = DebugOverlay.None;
		}
	}
}
