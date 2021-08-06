namespace UnityEngine.Rendering.PostProcessing
{
	[ExecuteAlways]
	[AddComponentMenu("Rendering/Post-process Debug", 1002)]
	public sealed class PostProcessDebug : MonoBehaviour
	{
		public PostProcessLayer postProcessLayer;

		private PostProcessLayer m_PreviousPostProcessLayer;

		public bool lightMeter;

		public bool histogram;

		public bool waveform;

		public bool vectorscope;

		public DebugOverlay debugOverlay;

		private Camera m_CurrentCamera;

		private CommandBuffer m_CmdAfterEverything;

		private void OnEnable()
		{
			m_CmdAfterEverything = new CommandBuffer
			{
				name = "Post-processing Debug Overlay"
			};
		}

		private void OnDisable()
		{
			if (m_CurrentCamera != null)
			{
				m_CurrentCamera.RemoveCommandBuffer(CameraEvent.AfterImageEffects, m_CmdAfterEverything);
			}
			m_CurrentCamera = null;
			m_PreviousPostProcessLayer = null;
		}

		private void Update()
		{
			UpdateStates();
		}

		private void Reset()
		{
			postProcessLayer = GetComponent<PostProcessLayer>();
		}

		private void UpdateStates()
		{
			if (m_PreviousPostProcessLayer != postProcessLayer)
			{
				if (m_CurrentCamera != null)
				{
					m_CurrentCamera.RemoveCommandBuffer(CameraEvent.AfterImageEffects, m_CmdAfterEverything);
					m_CurrentCamera = null;
				}
				m_PreviousPostProcessLayer = postProcessLayer;
				if (postProcessLayer != null)
				{
					m_CurrentCamera = postProcessLayer.GetComponent<Camera>();
					m_CurrentCamera.AddCommandBuffer(CameraEvent.AfterImageEffects, m_CmdAfterEverything);
				}
			}
			if (!(postProcessLayer == null) && postProcessLayer.enabled)
			{
				if (lightMeter)
				{
					postProcessLayer.debugLayer.RequestMonitorPass(MonitorType.LightMeter);
				}
				if (histogram)
				{
					postProcessLayer.debugLayer.RequestMonitorPass(MonitorType.Histogram);
				}
				if (waveform)
				{
					postProcessLayer.debugLayer.RequestMonitorPass(MonitorType.Waveform);
				}
				if (vectorscope)
				{
					postProcessLayer.debugLayer.RequestMonitorPass(MonitorType.Vectorscope);
				}
				postProcessLayer.debugLayer.RequestDebugOverlay(debugOverlay);
			}
		}

		private void OnPostRender()
		{
			m_CmdAfterEverything.Clear();
			if (!(postProcessLayer == null) && postProcessLayer.enabled && postProcessLayer.debugLayer.debugOverlayActive)
			{
				m_CmdAfterEverything.Blit(postProcessLayer.debugLayer.debugOverlayTarget, BuiltinRenderTextureType.CameraTarget);
			}
		}

		private void OnGUI()
		{
			if (!(postProcessLayer == null) && postProcessLayer.enabled)
			{
				RenderTexture.active = null;
				Rect rect = new Rect(5f, 5f, 0f, 0f);
				PostProcessDebugLayer debugLayer = postProcessLayer.debugLayer;
				DrawMonitor(ref rect, debugLayer.lightMeter, lightMeter);
				DrawMonitor(ref rect, debugLayer.histogram, histogram);
				DrawMonitor(ref rect, debugLayer.waveform, waveform);
				DrawMonitor(ref rect, debugLayer.vectorscope, vectorscope);
			}
		}

		private void DrawMonitor(ref Rect rect, Monitor monitor, bool enabled)
		{
			if (enabled && !(monitor.output == null))
			{
				rect.width = monitor.output.width;
				rect.height = monitor.output.height;
				GUI.DrawTexture(rect, monitor.output);
				rect.x += (float)monitor.output.width + 5f;
			}
		}
	}
}
