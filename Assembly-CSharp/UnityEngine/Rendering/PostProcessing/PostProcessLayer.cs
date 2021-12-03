#define UNITY_ASSERTIONS
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.Assertions;
using UnityEngine.Scripting;

namespace UnityEngine.Rendering.PostProcessing
{
	[ExecuteAlways]
	[DisallowMultipleComponent]
	[ImageEffectAllowedInSceneView]
	[AddComponentMenu("Rendering/Post-process Layer", 1000)]
	[RequireComponent(typeof(Camera))]
	public class PostProcessLayer : MonoBehaviour
	{
		private enum ScalingMode
		{
			NATIVE,
			BILINEAR,
			DLSS
		}

		public enum Antialiasing
		{
			None,
			FastApproximateAntialiasing,
			SubpixelMorphologicalAntialiasing,
			TemporalAntialiasing
		}

		[Serializable]
		public sealed class SerializedBundleRef
		{
			public string assemblyQualifiedName;

			public PostProcessBundle bundle;
		}

		public Transform volumeTrigger;

		public LayerMask volumeLayer;

		public bool stopNaNPropagation = true;

		public bool finalBlitToCameraTarget;

		public Antialiasing antialiasingMode;

		public TemporalAntialiasing temporalAntialiasing;

		public SubpixelMorphologicalAntialiasing subpixelMorphologicalAntialiasing;

		public FastApproximateAntialiasing fastApproximateAntialiasing;

		public Fog fog;

		private Dithering dithering;

		public PostProcessDebugLayer debugLayer;

		public RenderTextureFormat intermediateFormat = RenderTextureFormat.DefaultHDR;

		private RenderTextureFormat prevIntermediateFormat = RenderTextureFormat.DefaultHDR;

		private bool supportsIntermediateFormat = true;

		[SerializeField]
		private PostProcessResources m_Resources;

		[Preserve]
		[SerializeField]
		private bool m_ShowToolkit;

		[Preserve]
		[SerializeField]
		private bool m_ShowCustomSorter;

		public bool breakBeforeColorGrading;

		[SerializeField]
		private List<SerializedBundleRef> m_BeforeTransparentBundles;

		[SerializeField]
		private List<SerializedBundleRef> m_BeforeStackBundles;

		[SerializeField]
		private List<SerializedBundleRef> m_AfterStackBundles;

		private Dictionary<Type, PostProcessBundle> m_Bundles;

		private PropertySheetFactory m_PropertySheetFactory;

		private CommandBuffer m_LegacyCmdBufferBeforeReflections;

		private CommandBuffer m_LegacyCmdBufferBeforeLighting;

		private CommandBuffer m_LegacyCmdBufferOpaque;

		private CommandBuffer m_LegacyCmdBuffer;

		private Camera m_Camera;

		private PostProcessRenderContext m_CurrentContext;

		private LogHistogram m_LogHistogram;

		private bool m_SettingsUpdateNeeded = true;

		private bool m_IsRenderingInSceneView;

		private TargetPool m_TargetPool;

		private bool m_NaNKilled;

		private readonly List<PostProcessEffectRenderer> m_ActiveEffects = new List<PostProcessEffectRenderer>();

		private readonly List<RenderTargetIdentifier> m_Targets = new List<RenderTargetIdentifier>();

		public Dictionary<PostProcessEvent, List<SerializedBundleRef>> sortedBundles { get; private set; }

		public bool haveBundlesBeenInited { get; private set; }

		private void OnEnable()
		{
			Init(null);
			if (!haveBundlesBeenInited)
			{
				InitBundles();
			}
			m_LogHistogram = new LogHistogram();
			m_PropertySheetFactory = new PropertySheetFactory();
			m_TargetPool = new TargetPool();
			debugLayer.OnEnable();
			if (!RuntimeUtilities.scriptableRenderPipelineActive)
			{
				InitLegacy();
			}
		}

		private void InitLegacy()
		{
			m_LegacyCmdBufferBeforeReflections = new CommandBuffer
			{
				name = "Deferred Ambient Occlusion"
			};
			m_LegacyCmdBufferBeforeLighting = new CommandBuffer
			{
				name = "Deferred Ambient Occlusion"
			};
			m_LegacyCmdBufferOpaque = new CommandBuffer
			{
				name = "Opaque Only Post-processing"
			};
			m_LegacyCmdBuffer = new CommandBuffer
			{
				name = "Post-processing"
			};
			m_Camera = GetComponent<Camera>();
			m_Camera.AddCommandBuffer(CameraEvent.BeforeReflections, m_LegacyCmdBufferBeforeReflections);
			m_Camera.AddCommandBuffer(CameraEvent.BeforeLighting, m_LegacyCmdBufferBeforeLighting);
			m_Camera.AddCommandBuffer(CameraEvent.BeforeImageEffectsOpaque, m_LegacyCmdBufferOpaque);
			m_Camera.AddCommandBuffer(CameraEvent.BeforeImageEffects, m_LegacyCmdBuffer);
			m_CurrentContext = new PostProcessRenderContext();
		}

		[ImageEffectUsesCommandBuffer]
		private void OnRenderImage(RenderTexture src, RenderTexture dst)
		{
			if (finalBlitToCameraTarget)
			{
				RenderTexture.active = dst;
			}
			else
			{
				Graphics.Blit(src, dst);
			}
		}

		public void Init(PostProcessResources resources)
		{
			if (resources != null)
			{
				m_Resources = resources;
			}
			RuntimeUtilities.CreateIfNull(ref temporalAntialiasing);
			RuntimeUtilities.CreateIfNull(ref subpixelMorphologicalAntialiasing);
			RuntimeUtilities.CreateIfNull(ref fastApproximateAntialiasing);
			RuntimeUtilities.CreateIfNull(ref dithering);
			RuntimeUtilities.CreateIfNull(ref fog);
			RuntimeUtilities.CreateIfNull(ref debugLayer);
		}

		public void InitBundles()
		{
			if (haveBundlesBeenInited)
			{
				return;
			}
			RuntimeUtilities.CreateIfNull(ref m_BeforeTransparentBundles);
			RuntimeUtilities.CreateIfNull(ref m_BeforeStackBundles);
			RuntimeUtilities.CreateIfNull(ref m_AfterStackBundles);
			m_Bundles = new Dictionary<Type, PostProcessBundle>();
			foreach (Type key in PostProcessManager.instance.settingsTypes.Keys)
			{
				PostProcessBundle value = new PostProcessBundle((PostProcessEffectSettings)ScriptableObject.CreateInstance(key));
				m_Bundles.Add(key, value);
			}
			UpdateBundleSortList(m_BeforeTransparentBundles, PostProcessEvent.BeforeTransparent);
			UpdateBundleSortList(m_BeforeStackBundles, PostProcessEvent.BeforeStack);
			UpdateBundleSortList(m_AfterStackBundles, PostProcessEvent.AfterStack);
			sortedBundles = new Dictionary<PostProcessEvent, List<SerializedBundleRef>>(default(PostProcessEventComparer))
			{
				{
					PostProcessEvent.BeforeTransparent,
					m_BeforeTransparentBundles
				},
				{
					PostProcessEvent.BeforeStack,
					m_BeforeStackBundles
				},
				{
					PostProcessEvent.AfterStack,
					m_AfterStackBundles
				}
			};
			haveBundlesBeenInited = true;
		}

		private void UpdateBundleSortList(List<SerializedBundleRef> sortedList, PostProcessEvent evt)
		{
			List<PostProcessBundle> effects = (from kvp in m_Bundles
				where kvp.Value.attribute.eventType == evt && !kvp.Value.attribute.builtinEffect
				select kvp.Value).ToList();
			sortedList.RemoveAll(delegate(SerializedBundleRef x)
			{
				string searchStr = x.assemblyQualifiedName;
				return !effects.Exists((PostProcessBundle b) => b.settings.GetType().AssemblyQualifiedName == searchStr);
			});
			foreach (PostProcessBundle item2 in effects)
			{
				string typeName2 = item2.settings.GetType().AssemblyQualifiedName;
				if (!sortedList.Exists((SerializedBundleRef b) => b.assemblyQualifiedName == typeName2))
				{
					SerializedBundleRef item = new SerializedBundleRef
					{
						assemblyQualifiedName = typeName2
					};
					sortedList.Add(item);
				}
			}
			foreach (SerializedBundleRef sorted in sortedList)
			{
				string typeName = sorted.assemblyQualifiedName;
				PostProcessBundle postProcessBundle = (sorted.bundle = effects.Find((PostProcessBundle b) => b.settings.GetType().AssemblyQualifiedName == typeName));
			}
		}

		private void OnDisable()
		{
			if (m_Camera != null)
			{
				if (m_LegacyCmdBufferBeforeReflections != null)
				{
					m_Camera.RemoveCommandBuffer(CameraEvent.BeforeReflections, m_LegacyCmdBufferBeforeReflections);
				}
				if (m_LegacyCmdBufferBeforeLighting != null)
				{
					m_Camera.RemoveCommandBuffer(CameraEvent.BeforeLighting, m_LegacyCmdBufferBeforeLighting);
				}
				if (m_LegacyCmdBufferOpaque != null)
				{
					m_Camera.RemoveCommandBuffer(CameraEvent.BeforeImageEffectsOpaque, m_LegacyCmdBufferOpaque);
				}
				if (m_LegacyCmdBuffer != null)
				{
					m_Camera.RemoveCommandBuffer(CameraEvent.BeforeImageEffects, m_LegacyCmdBuffer);
				}
			}
			temporalAntialiasing.Release();
			m_LogHistogram.Release();
			foreach (PostProcessBundle value in m_Bundles.Values)
			{
				value.Release();
			}
			m_Bundles.Clear();
			m_PropertySheetFactory.Release();
			if (debugLayer != null)
			{
				debugLayer.OnDisable();
			}
			TextureLerper.instance.Clear();
			m_Camera.ResetProjectionMatrix();
			m_Camera.nonJitteredProjectionMatrix = m_Camera.projectionMatrix;
			Shader.SetGlobalVector("_FrustumJitter", Vector2.zero);
			haveBundlesBeenInited = false;
		}

		private void Reset()
		{
			volumeTrigger = base.transform;
		}

		private void OnPreCull()
		{
			if (!RuntimeUtilities.scriptableRenderPipelineActive)
			{
				if (m_Camera == null || m_CurrentContext == null)
				{
					InitLegacy();
				}
				if (!m_Camera.usePhysicalProperties)
				{
					m_Camera.ResetProjectionMatrix();
				}
				m_Camera.nonJitteredProjectionMatrix = m_Camera.projectionMatrix;
				if (m_Camera.stereoEnabled)
				{
					m_Camera.ResetStereoProjectionMatrices();
				}
				else
				{
					Shader.SetGlobalFloat(ShaderIDs.RenderViewportScaleFactor, 1f);
				}
				BuildCommandBuffers();
				Shader.SetGlobalVector("_FrustumJitter", temporalAntialiasing.jitter);
			}
		}

		private void OnPreRender()
		{
			if (!RuntimeUtilities.scriptableRenderPipelineActive && m_Camera.stereoActiveEye == Camera.MonoOrStereoscopicEye.Right)
			{
				BuildCommandBuffers();
			}
		}

		private RenderTextureFormat GetIntermediateFormat()
		{
			if (intermediateFormat != prevIntermediateFormat)
			{
				supportsIntermediateFormat = SystemInfo.SupportsRenderTextureFormat(intermediateFormat);
				prevIntermediateFormat = intermediateFormat;
			}
			if (!supportsIntermediateFormat)
			{
				return RenderTextureFormat.DefaultHDR;
			}
			return intermediateFormat;
		}

		private static bool RequiresInitialBlit(Camera camera, PostProcessRenderContext context)
		{
			if (camera.allowMSAA)
			{
				return true;
			}
			if (RuntimeUtilities.scriptableRenderPipelineActive)
			{
				return true;
			}
			return false;
		}

		private void UpdateSrcDstForOpaqueOnly(ref int src, ref int dst, PostProcessRenderContext context, RenderTargetIdentifier cameraTarget, int opaqueOnlyEffectsRemaining)
		{
			if (src > -1)
			{
				context.command.ReleaseTemporaryRT(src);
			}
			context.source = context.destination;
			src = dst;
			if (opaqueOnlyEffectsRemaining == 1)
			{
				context.destination = cameraTarget;
				return;
			}
			dst = m_TargetPool.Get();
			context.destination = dst;
			context.GetScreenSpaceTemporaryRT(context.command, dst, 0, context.sourceFormat);
		}

		private void BuildCommandBuffers()
		{
			PostProcessRenderContext currentContext = m_CurrentContext;
			RenderTextureFormat renderTextureFormat = GetIntermediateFormat();
			RenderTextureFormat renderTextureFormat2 = (m_Camera.allowHDR ? renderTextureFormat : RenderTextureFormat.Default);
			if (!RuntimeUtilities.isFloatingPointFormat(renderTextureFormat2))
			{
				m_NaNKilled = true;
			}
			currentContext.Reset();
			currentContext.camera = m_Camera;
			currentContext.sourceFormat = renderTextureFormat2;
			m_LegacyCmdBufferBeforeReflections.Clear();
			m_LegacyCmdBufferBeforeLighting.Clear();
			m_LegacyCmdBufferOpaque.Clear();
			m_LegacyCmdBuffer.Clear();
			SetupContext(currentContext);
			currentContext.command = m_LegacyCmdBufferOpaque;
			TextureLerper.instance.BeginFrame(currentContext);
			UpdateVolumeSystem(currentContext.camera, currentContext.command);
			PostProcessBundle bundle = GetBundle<AmbientOcclusion>();
			AmbientOcclusion ambientOcclusion = bundle.CastSettings<AmbientOcclusion>();
			AmbientOcclusionRenderer ambientOcclusionRenderer = bundle.CastRenderer<AmbientOcclusionRenderer>();
			bool flag = ambientOcclusion.IsEnabledAndSupported(currentContext);
			bool flag2 = ambientOcclusionRenderer.IsAmbientOnly(currentContext);
			bool flag3 = flag && flag2;
			bool flag4 = flag && !flag2;
			PostProcessBundle bundle2 = GetBundle<ScreenSpaceReflections>();
			PostProcessEffectSettings settings = bundle2.settings;
			PostProcessEffectRenderer renderer = bundle2.renderer;
			bool flag5 = settings.IsEnabledAndSupported(currentContext);
			if (flag3)
			{
				IAmbientOcclusionMethod ambientOcclusionMethod = ambientOcclusionRenderer.Get();
				currentContext.command = m_LegacyCmdBufferBeforeReflections;
				ambientOcclusionMethod.RenderAmbientOnly(currentContext);
				currentContext.command = m_LegacyCmdBufferBeforeLighting;
				ambientOcclusionMethod.CompositeAmbientOnly(currentContext);
			}
			else if (flag4)
			{
				currentContext.command = m_LegacyCmdBufferOpaque;
				ambientOcclusionRenderer.Get().RenderAfterOpaque(currentContext);
			}
			bool flag6 = fog.IsEnabledAndSupported(currentContext);
			bool flag7 = HasOpaqueOnlyEffects(currentContext);
			int num = 0;
			num += (flag5 ? 1 : 0);
			num += (flag6 ? 1 : 0);
			num += (flag7 ? 1 : 0);
			RenderTargetIdentifier renderTargetIdentifier = new RenderTargetIdentifier(BuiltinRenderTextureType.CameraTarget);
			if (num > 0)
			{
				CommandBuffer commandBuffer = (currentContext.command = m_LegacyCmdBufferOpaque);
				currentContext.source = renderTargetIdentifier;
				currentContext.destination = renderTargetIdentifier;
				int src = -1;
				int dst = -1;
				UpdateSrcDstForOpaqueOnly(ref src, ref dst, currentContext, renderTargetIdentifier, num + 1);
				if (RequiresInitialBlit(m_Camera, currentContext) || num == 1)
				{
					RuntimeUtilities.BuiltinBlit(commandBuffer, currentContext.source, currentContext.destination, RuntimeUtilities.copyStdMaterial, stopNaNPropagation ? 1 : 0);
					UpdateSrcDstForOpaqueOnly(ref src, ref dst, currentContext, renderTargetIdentifier, num);
				}
				if (flag5)
				{
					renderer.Render(currentContext);
					num--;
					UpdateSrcDstForOpaqueOnly(ref src, ref dst, currentContext, renderTargetIdentifier, num);
				}
				if (flag6)
				{
					fog.Render(currentContext);
					num--;
					UpdateSrcDstForOpaqueOnly(ref src, ref dst, currentContext, renderTargetIdentifier, num);
				}
				if (flag7)
				{
					RenderOpaqueOnly(currentContext);
				}
				commandBuffer.ReleaseTemporaryRT(src);
			}
			BuildPostEffectsOld(renderTextureFormat2, currentContext, renderTargetIdentifier);
		}

		private void BuildPostEffectsOld(RenderTextureFormat sourceFormat, PostProcessRenderContext context, RenderTargetIdentifier cameraTarget)
		{
			int num = -1;
			bool flag = !m_NaNKilled && stopNaNPropagation && RuntimeUtilities.isFloatingPointFormat(sourceFormat);
			if (RequiresInitialBlit(m_Camera, context) || flag)
			{
				num = m_TargetPool.Get();
				context.GetScreenSpaceTemporaryRT(m_LegacyCmdBuffer, num, 0, sourceFormat, RenderTextureReadWrite.sRGB);
				RuntimeUtilities.BuiltinBlit(m_LegacyCmdBuffer, cameraTarget, num, RuntimeUtilities.copyStdMaterial, stopNaNPropagation ? 1 : 0);
				if (!m_NaNKilled)
				{
					m_NaNKilled = stopNaNPropagation;
				}
				context.source = num;
			}
			else
			{
				context.source = cameraTarget;
			}
			context.destination = cameraTarget;
			if (finalBlitToCameraTarget && !RuntimeUtilities.scriptableRenderPipelineActive)
			{
				if ((bool)m_Camera.targetTexture)
				{
					context.destination = m_Camera.targetTexture.colorBuffer;
				}
				else
				{
					context.flip = true;
					context.destination = Display.main.colorBuffer;
				}
			}
			context.command = m_LegacyCmdBuffer;
			Render(context);
			if (num > -1)
			{
				m_LegacyCmdBuffer.ReleaseTemporaryRT(num);
			}
		}

		private void OnPostRender()
		{
			if (!RuntimeUtilities.scriptableRenderPipelineActive && m_CurrentContext.IsTemporalAntialiasingActive())
			{
				if (m_CurrentContext.physicalCamera)
				{
					m_Camera.usePhysicalProperties = true;
				}
				else
				{
					m_Camera.ResetProjectionMatrix();
				}
				if (m_CurrentContext.stereoActive && (RuntimeUtilities.isSinglePassStereoEnabled || m_Camera.stereoActiveEye == Camera.MonoOrStereoscopicEye.Right))
				{
					m_Camera.ResetStereoProjectionMatrices();
				}
			}
		}

		public PostProcessBundle GetBundle<T>() where T : PostProcessEffectSettings
		{
			return GetBundle(typeof(T));
		}

		public PostProcessBundle GetBundle(Type settingsType)
		{
			Assert.IsTrue(m_Bundles.ContainsKey(settingsType), "Invalid type");
			return m_Bundles[settingsType];
		}

		public T GetSettings<T>() where T : PostProcessEffectSettings
		{
			return GetBundle<T>().CastSettings<T>();
		}

		public void BakeMSVOMap(CommandBuffer cmd, Camera camera, RenderTargetIdentifier destination, RenderTargetIdentifier? depthMap, bool invert, bool isMSAA = false)
		{
			MultiScaleVO multiScaleVO = GetBundle<AmbientOcclusion>().CastRenderer<AmbientOcclusionRenderer>().GetMultiScaleVO();
			multiScaleVO.SetResources(m_Resources);
			multiScaleVO.GenerateAOMap(cmd, camera, destination, depthMap, invert, isMSAA);
		}

		internal void OverrideSettings(List<PostProcessEffectSettings> baseSettings, float interpFactor)
		{
			foreach (PostProcessEffectSettings baseSetting in baseSettings)
			{
				if (!baseSetting.active)
				{
					continue;
				}
				PostProcessEffectSettings settings = GetBundle(baseSetting.GetType()).settings;
				int count = baseSetting.parameters.Count;
				for (int i = 0; i < count; i++)
				{
					ParameterOverride parameterOverride = baseSetting.parameters[i];
					if (parameterOverride.overrideState)
					{
						ParameterOverride parameterOverride2 = settings.parameters[i];
						parameterOverride2.Interp(parameterOverride2, parameterOverride, interpFactor);
					}
				}
			}
		}

		private void SetLegacyCameraFlags(PostProcessRenderContext context)
		{
			DepthTextureMode depthTextureMode = context.camera.depthTextureMode;
			foreach (KeyValuePair<Type, PostProcessBundle> bundle in m_Bundles)
			{
				if (bundle.Value.settings.IsEnabledAndSupported(context))
				{
					depthTextureMode |= bundle.Value.renderer.GetCameraFlags();
				}
			}
			if (context.IsTemporalAntialiasingActive())
			{
				depthTextureMode |= temporalAntialiasing.GetCameraFlags();
			}
			if (fog.IsEnabledAndSupported(context))
			{
				depthTextureMode |= fog.GetCameraFlags();
			}
			if (debugLayer.debugOverlay != 0)
			{
				depthTextureMode |= debugLayer.GetCameraFlags();
			}
			context.camera.depthTextureMode = depthTextureMode;
		}

		public void ResetHistory()
		{
			foreach (KeyValuePair<Type, PostProcessBundle> bundle in m_Bundles)
			{
				bundle.Value.ResetHistory();
			}
			temporalAntialiasing.ResetHistory();
		}

		public bool HasOpaqueOnlyEffects(PostProcessRenderContext context)
		{
			return HasActiveEffects(PostProcessEvent.BeforeTransparent, context);
		}

		public bool HasActiveEffects(PostProcessEvent evt, PostProcessRenderContext context)
		{
			foreach (SerializedBundleRef item in sortedBundles[evt])
			{
				bool flag = item.bundle.settings.IsEnabledAndSupported(context);
				if (context.isSceneView)
				{
					if (item.bundle.attribute.allowInSceneView && flag)
					{
						return true;
					}
				}
				else if (flag)
				{
					return true;
				}
			}
			return false;
		}

		private void SetupContext(PostProcessRenderContext context)
		{
			RuntimeUtilities.s_Resources = m_Resources;
			m_IsRenderingInSceneView = context.camera.cameraType == CameraType.SceneView;
			context.isSceneView = m_IsRenderingInSceneView;
			context.resources = m_Resources;
			context.propertySheets = m_PropertySheetFactory;
			context.debugLayer = debugLayer;
			context.antialiasing = antialiasingMode;
			context.temporalAntialiasing = temporalAntialiasing;
			context.logHistogram = m_LogHistogram;
			context.physicalCamera = context.camera.usePhysicalProperties;
			SetLegacyCameraFlags(context);
			debugLayer.SetFrameSize(context.width, context.height);
			m_CurrentContext = context;
		}

		public void UpdateVolumeSystem(Camera cam, CommandBuffer cmd)
		{
			if (m_SettingsUpdateNeeded)
			{
				cmd.BeginSample("VolumeBlending");
				PostProcessManager.instance.UpdateSettings(this, cam);
				cmd.EndSample("VolumeBlending");
				m_TargetPool.Reset();
				if (RuntimeUtilities.scriptableRenderPipelineActive)
				{
					Shader.SetGlobalFloat(ShaderIDs.RenderViewportScaleFactor, 1f);
				}
			}
			m_SettingsUpdateNeeded = false;
		}

		public void RenderOpaqueOnly(PostProcessRenderContext context)
		{
			if (RuntimeUtilities.scriptableRenderPipelineActive)
			{
				SetupContext(context);
			}
			TextureLerper.instance.BeginFrame(context);
			UpdateVolumeSystem(context.camera, context.command);
			RenderList(sortedBundles[PostProcessEvent.BeforeTransparent], context, "OpaqueOnly");
		}

		public void Render(PostProcessRenderContext context)
		{
			if (RuntimeUtilities.scriptableRenderPipelineActive)
			{
				SetupContext(context);
			}
			TextureLerper.instance.BeginFrame(context);
			CommandBuffer command = context.command;
			UpdateVolumeSystem(context.camera, context.command);
			int num = -1;
			RenderTargetIdentifier source = context.source;
			if (context.stereoActive && context.numberOfEyes > 1 && context.stereoRenderingMode == PostProcessRenderContext.StereoRenderingMode.SinglePass)
			{
				command.SetSinglePassStereo(SinglePassStereoMode.None);
				command.DisableShaderKeyword("UNITY_SINGLE_PASS_STEREO");
			}
			for (int i = 0; i < context.numberOfEyes; i++)
			{
				bool flag = false;
				if (stopNaNPropagation && !m_NaNKilled)
				{
					num = m_TargetPool.Get();
					context.GetScreenSpaceTemporaryRT(command, num, 0, context.sourceFormat);
					if (context.stereoActive && context.numberOfEyes > 1)
					{
						if (context.stereoRenderingMode == PostProcessRenderContext.StereoRenderingMode.SinglePassInstanced)
						{
							RuntimeUtilities.BlitFullscreenTriangleFromTexArray(command, context.source, num, RuntimeUtilities.copyFromTexArraySheet, 1, false, i);
							flag = true;
						}
						else if (context.stereoRenderingMode == PostProcessRenderContext.StereoRenderingMode.SinglePass)
						{
							RuntimeUtilities.BlitFullscreenTriangleFromDoubleWide(command, context.source, num, RuntimeUtilities.copyStdFromDoubleWideMaterial, 1, i);
							flag = true;
						}
					}
					else
					{
						RuntimeUtilities.BlitFullscreenTriangle(command, context.source, num, RuntimeUtilities.copySheet, 1);
					}
					context.source = num;
					m_NaNKilled = true;
				}
				if (!flag && context.numberOfEyes > 1)
				{
					num = m_TargetPool.Get();
					context.GetScreenSpaceTemporaryRT(command, num, 0, context.sourceFormat);
					if (context.stereoActive)
					{
						if (context.stereoRenderingMode == PostProcessRenderContext.StereoRenderingMode.SinglePassInstanced)
						{
							RuntimeUtilities.BlitFullscreenTriangleFromTexArray(command, context.source, num, RuntimeUtilities.copyFromTexArraySheet, 1, false, i);
							flag = true;
						}
						else if (context.stereoRenderingMode == PostProcessRenderContext.StereoRenderingMode.SinglePass)
						{
							RuntimeUtilities.BlitFullscreenTriangleFromDoubleWide(command, context.source, num, RuntimeUtilities.copyStdFromDoubleWideMaterial, stopNaNPropagation ? 1 : 0, i);
							flag = true;
						}
					}
					context.source = num;
				}
				if (context.IsTemporalAntialiasingActive())
				{
					temporalAntialiasing.sampleCount = 8;
					if (!RuntimeUtilities.scriptableRenderPipelineActive)
					{
						if (context.stereoActive)
						{
							if (context.camera.stereoActiveEye != Camera.MonoOrStereoscopicEye.Right)
							{
								temporalAntialiasing.ConfigureStereoJitteredProjectionMatrices(context);
							}
						}
						else
						{
							temporalAntialiasing.ConfigureJitteredProjectionMatrix(context);
						}
					}
					int num2 = m_TargetPool.Get();
					RenderTargetIdentifier destination = context.destination;
					context.GetScreenSpaceTemporaryRT(command, num2, 0, context.sourceFormat);
					context.destination = num2;
					temporalAntialiasing.Render(context);
					context.source = num2;
					context.destination = destination;
					if (num > -1)
					{
						command.ReleaseTemporaryRT(num);
					}
					num = num2;
				}
				bool num3 = HasActiveEffects(PostProcessEvent.BeforeStack, context);
				bool flag2 = HasActiveEffects(PostProcessEvent.AfterStack, context) && !breakBeforeColorGrading;
				bool flag3 = (flag2 || antialiasingMode == Antialiasing.FastApproximateAntialiasing || (antialiasingMode == Antialiasing.SubpixelMorphologicalAntialiasing && subpixelMorphologicalAntialiasing.IsSupported())) && !breakBeforeColorGrading;
				if (num3)
				{
					num = RenderInjectionPoint(PostProcessEvent.BeforeStack, context, "BeforeStack", num);
				}
				num = RenderBuiltins(context, !flag3, num, i);
				if (flag2)
				{
					num = RenderInjectionPoint(PostProcessEvent.AfterStack, context, "AfterStack", num);
				}
				if (flag3)
				{
					RenderFinalPass(context, num, i);
				}
				if (context.stereoActive)
				{
					context.source = source;
				}
			}
			if (context.stereoActive && context.numberOfEyes > 1 && context.stereoRenderingMode == PostProcessRenderContext.StereoRenderingMode.SinglePass)
			{
				command.SetSinglePassStereo(SinglePassStereoMode.SideBySide);
				command.EnableShaderKeyword("UNITY_SINGLE_PASS_STEREO");
			}
			debugLayer.RenderSpecialOverlays(context);
			debugLayer.RenderMonitors(context);
			TextureLerper.instance.EndFrame();
			debugLayer.EndFrame();
			m_SettingsUpdateNeeded = true;
			m_NaNKilled = false;
		}

		private int RenderInjectionPoint(PostProcessEvent evt, PostProcessRenderContext context, string marker, int releaseTargetAfterUse = -1)
		{
			int num = m_TargetPool.Get();
			RenderTargetIdentifier destination = context.destination;
			CommandBuffer command = context.command;
			context.GetScreenSpaceTemporaryRT(command, num, 0, context.sourceFormat);
			context.destination = num;
			RenderList(sortedBundles[evt], context, marker);
			context.source = num;
			context.destination = destination;
			if (releaseTargetAfterUse > -1)
			{
				command.ReleaseTemporaryRT(releaseTargetAfterUse);
			}
			return num;
		}

		private void RenderList(List<SerializedBundleRef> list, PostProcessRenderContext context, string marker)
		{
			CommandBuffer command = context.command;
			command.BeginSample(marker);
			m_ActiveEffects.Clear();
			for (int i = 0; i < list.Count; i++)
			{
				PostProcessBundle bundle = list[i].bundle;
				if (bundle.settings.IsEnabledAndSupported(context) && (!context.isSceneView || (context.isSceneView && bundle.attribute.allowInSceneView)))
				{
					m_ActiveEffects.Add(bundle.renderer);
				}
			}
			int count = m_ActiveEffects.Count;
			if (count == 1)
			{
				m_ActiveEffects[0].Render(context);
			}
			else
			{
				m_Targets.Clear();
				m_Targets.Add(context.source);
				int num = m_TargetPool.Get();
				int num2 = m_TargetPool.Get();
				for (int j = 0; j < count - 1; j++)
				{
					m_Targets.Add((j % 2 == 0) ? num : num2);
				}
				m_Targets.Add(context.destination);
				context.GetScreenSpaceTemporaryRT(command, num, 0, context.sourceFormat);
				if (count > 2)
				{
					context.GetScreenSpaceTemporaryRT(command, num2, 0, context.sourceFormat);
				}
				for (int k = 0; k < count; k++)
				{
					context.source = m_Targets[k];
					context.destination = m_Targets[k + 1];
					m_ActiveEffects[k].Render(context);
				}
				command.ReleaseTemporaryRT(num);
				if (count > 2)
				{
					command.ReleaseTemporaryRT(num2);
				}
			}
			command.EndSample(marker);
		}

		private void ApplyFlip(PostProcessRenderContext context, MaterialPropertyBlock properties)
		{
			if (context.flip && !context.isSceneView)
			{
				properties.SetVector(ShaderIDs.UVTransform, new Vector4(1f, 1f, 0f, 0f));
			}
			else
			{
				ApplyDefaultFlip(properties);
			}
		}

		private void ApplyDefaultFlip(MaterialPropertyBlock properties)
		{
			properties.SetVector(ShaderIDs.UVTransform, SystemInfo.graphicsUVStartsAtTop ? new Vector4(1f, -1f, 0f, 1f) : new Vector4(1f, 1f, 0f, 0f));
		}

		private int RenderBuiltins(PostProcessRenderContext context, bool isFinalPass, int releaseTargetAfterUse = -1, int eye = -1)
		{
			PropertySheet propertySheet = context.propertySheets.Get(context.resources.shaders.uber);
			propertySheet.ClearKeywords();
			propertySheet.properties.Clear();
			context.uberSheet = propertySheet;
			context.bloomBufferNameID = -1;
			context.autoExposureTexture = RuntimeUtilities.whiteTexture;
			if (isFinalPass && context.stereoActive && context.stereoRenderingMode == PostProcessRenderContext.StereoRenderingMode.SinglePassInstanced)
			{
				propertySheet.EnableKeyword("STEREO_INSTANCING_ENABLED");
			}
			CommandBuffer command = context.command;
			command.BeginSample("BuiltinStack");
			int num = -1;
			RenderTargetIdentifier destination = context.destination;
			if (!isFinalPass)
			{
				num = m_TargetPool.Get();
				context.GetScreenSpaceTemporaryRT(command, num, 0, context.sourceFormat);
				context.destination = num;
				if (antialiasingMode == Antialiasing.FastApproximateAntialiasing && !fastApproximateAntialiasing.keepAlpha)
				{
					propertySheet.properties.SetFloat(ShaderIDs.LumaInAlpha, 1f);
				}
			}
			int num2 = RenderEffect<DepthOfFieldEffect>(context, true);
			int num3 = RenderEffect<MotionBlur>(context, true);
			if (ShouldGenerateLogHistogram(context))
			{
				m_LogHistogram.Generate(context);
			}
			RenderEffect<AutoExposure>(context);
			propertySheet.properties.SetTexture(ShaderIDs.AutoExposureTex, context.autoExposureTexture);
			RenderEffect<LensDistortion>(context);
			RenderEffect<ChromaticAberration>(context);
			RenderEffect<Bloom>(context);
			RenderEffect<Vignette>(context);
			RenderEffect<Grain>(context);
			if (!breakBeforeColorGrading)
			{
				RenderEffect<ColorGrading>(context);
			}
			if (isFinalPass)
			{
				propertySheet.EnableKeyword("FINALPASS");
				dithering.Render(context);
				ApplyFlip(context, propertySheet.properties);
			}
			else
			{
				ApplyDefaultFlip(propertySheet.properties);
			}
			if (context.stereoActive && context.stereoRenderingMode == PostProcessRenderContext.StereoRenderingMode.SinglePassInstanced)
			{
				propertySheet.properties.SetFloat(ShaderIDs.DepthSlice, eye);
				RuntimeUtilities.BlitFullscreenTriangleToTexArray(command, context.source, context.destination, propertySheet, 0, false, eye);
			}
			else if (isFinalPass && context.stereoActive && context.numberOfEyes > 1 && context.stereoRenderingMode == PostProcessRenderContext.StereoRenderingMode.SinglePass)
			{
				RuntimeUtilities.BlitFullscreenTriangleToDoubleWide(command, context.source, context.destination, propertySheet, 0, eye);
			}
			else
			{
				RuntimeUtilities.BlitFullscreenTriangle(command, context.source, context.destination, propertySheet, 0);
			}
			context.source = context.destination;
			context.destination = destination;
			if (releaseTargetAfterUse > -1)
			{
				command.ReleaseTemporaryRT(releaseTargetAfterUse);
			}
			if (num3 > -1)
			{
				command.ReleaseTemporaryRT(num3);
			}
			if (num2 > -1)
			{
				command.ReleaseTemporaryRT(num2);
			}
			if (context.bloomBufferNameID > -1)
			{
				command.ReleaseTemporaryRT(context.bloomBufferNameID);
			}
			command.EndSample("BuiltinStack");
			return num;
		}

		private void RenderFinalPass(PostProcessRenderContext context, int releaseTargetAfterUse = -1, int eye = -1)
		{
			CommandBuffer command = context.command;
			command.BeginSample("FinalPass");
			if (breakBeforeColorGrading)
			{
				PropertySheet propertySheet = context.propertySheets.Get(context.resources.shaders.discardAlpha);
				if (context.stereoActive && context.stereoRenderingMode == PostProcessRenderContext.StereoRenderingMode.SinglePassInstanced)
				{
					propertySheet.EnableKeyword("STEREO_INSTANCING_ENABLED");
				}
				if (context.stereoActive && context.stereoRenderingMode == PostProcessRenderContext.StereoRenderingMode.SinglePassInstanced)
				{
					propertySheet.properties.SetFloat(ShaderIDs.DepthSlice, eye);
					RuntimeUtilities.BlitFullscreenTriangleToTexArray(command, context.source, context.destination, propertySheet, 0, false, eye);
				}
				else if (context.stereoActive && context.numberOfEyes > 1 && context.stereoRenderingMode == PostProcessRenderContext.StereoRenderingMode.SinglePass)
				{
					RuntimeUtilities.BlitFullscreenTriangleToDoubleWide(command, context.source, context.destination, propertySheet, 0, eye);
				}
				else
				{
					RuntimeUtilities.BlitFullscreenTriangle(command, context.source, context.destination, propertySheet, 0);
				}
			}
			else
			{
				PropertySheet propertySheet2 = context.propertySheets.Get(context.resources.shaders.finalPass);
				propertySheet2.ClearKeywords();
				propertySheet2.properties.Clear();
				context.uberSheet = propertySheet2;
				int num = -1;
				if (context.stereoActive && context.stereoRenderingMode == PostProcessRenderContext.StereoRenderingMode.SinglePassInstanced)
				{
					propertySheet2.EnableKeyword("STEREO_INSTANCING_ENABLED");
				}
				if (antialiasingMode == Antialiasing.FastApproximateAntialiasing)
				{
					propertySheet2.EnableKeyword(fastApproximateAntialiasing.fastMode ? "FXAA_LOW" : "FXAA");
					if (fastApproximateAntialiasing.keepAlpha)
					{
						propertySheet2.EnableKeyword("FXAA_KEEP_ALPHA");
					}
				}
				else if (antialiasingMode == Antialiasing.SubpixelMorphologicalAntialiasing && subpixelMorphologicalAntialiasing.IsSupported())
				{
					num = m_TargetPool.Get();
					RenderTargetIdentifier destination = context.destination;
					context.GetScreenSpaceTemporaryRT(context.command, num, 0, context.sourceFormat);
					context.destination = num;
					subpixelMorphologicalAntialiasing.Render(context);
					context.source = num;
					context.destination = destination;
				}
				dithering.Render(context);
				ApplyFlip(context, propertySheet2.properties);
				if (context.stereoActive && context.stereoRenderingMode == PostProcessRenderContext.StereoRenderingMode.SinglePassInstanced)
				{
					propertySheet2.properties.SetFloat(ShaderIDs.DepthSlice, eye);
					RuntimeUtilities.BlitFullscreenTriangleToTexArray(command, context.source, context.destination, propertySheet2, 0, false, eye);
				}
				else if (context.stereoActive && context.numberOfEyes > 1 && context.stereoRenderingMode == PostProcessRenderContext.StereoRenderingMode.SinglePass)
				{
					RuntimeUtilities.BlitFullscreenTriangleToDoubleWide(command, context.source, context.destination, propertySheet2, 0, eye);
				}
				else
				{
					RuntimeUtilities.BlitFullscreenTriangle(command, context.source, context.destination, propertySheet2, 0);
				}
				if (num > -1)
				{
					command.ReleaseTemporaryRT(num);
				}
			}
			if (releaseTargetAfterUse > -1)
			{
				command.ReleaseTemporaryRT(releaseTargetAfterUse);
			}
			command.EndSample("FinalPass");
		}

		private int RenderEffect<T>(PostProcessRenderContext context, bool useTempTarget = false) where T : PostProcessEffectSettings
		{
			PostProcessBundle bundle = GetBundle<T>();
			if (!bundle.settings.IsEnabledAndSupported(context))
			{
				return -1;
			}
			if (m_IsRenderingInSceneView && !bundle.attribute.allowInSceneView)
			{
				return -1;
			}
			if (!useTempTarget)
			{
				bundle.renderer.Render(context);
				return -1;
			}
			RenderTargetIdentifier destination = context.destination;
			int num = m_TargetPool.Get();
			context.GetScreenSpaceTemporaryRT(context.command, num, 0, context.sourceFormat);
			context.destination = num;
			bundle.renderer.Render(context);
			context.source = num;
			context.destination = destination;
			return num;
		}

		private bool ShouldGenerateLogHistogram(PostProcessRenderContext context)
		{
			bool num = GetBundle<AutoExposure>().settings.IsEnabledAndSupported(context);
			bool flag = debugLayer.lightMeter.IsRequestedAndSupported(context);
			return num || flag;
		}
	}
}
