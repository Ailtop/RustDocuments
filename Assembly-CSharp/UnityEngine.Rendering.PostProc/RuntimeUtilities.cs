#define UNITY_ASSERTIONS
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using UnityEngine.Assertions;
using UnityEngine.SceneManagement;

namespace UnityEngine.Rendering.PostProcessing
{
	public static class RuntimeUtilities
	{
		private static Texture2D m_WhiteTexture;

		private static Texture3D m_WhiteTexture3D;

		private static Texture2D m_BlackTexture;

		private static Texture3D m_BlackTexture3D;

		private static Texture2D m_TransparentTexture;

		private static Texture3D m_TransparentTexture3D;

		private static Dictionary<int, Texture2D> m_LutStrips = new Dictionary<int, Texture2D>();

		internal static PostProcessResources s_Resources;

		private static Mesh s_FullscreenTriangle;

		private static Material s_CopyStdMaterial;

		private static Material s_CopyStdFromDoubleWideMaterial;

		private static Material s_CopyMaterial;

		private static Material s_CopyFromTexArrayMaterial;

		private static PropertySheet s_CopySheet;

		private static PropertySheet s_CopyFromTexArraySheet;

		private static IEnumerable<Type> m_AssemblyTypes;

		public static Texture2D whiteTexture
		{
			get
			{
				if (m_WhiteTexture == null)
				{
					m_WhiteTexture = new Texture2D(1, 1, TextureFormat.ARGB32, false)
					{
						name = "White Texture"
					};
					m_WhiteTexture.SetPixel(0, 0, Color.white);
					m_WhiteTexture.Apply();
				}
				return m_WhiteTexture;
			}
		}

		public static Texture3D whiteTexture3D
		{
			get
			{
				if (m_WhiteTexture3D == null)
				{
					m_WhiteTexture3D = new Texture3D(1, 1, 1, TextureFormat.ARGB32, false)
					{
						name = "White Texture 3D"
					};
					m_WhiteTexture3D.SetPixels(new Color[1] { Color.white });
					m_WhiteTexture3D.Apply();
				}
				return m_WhiteTexture3D;
			}
		}

		public static Texture2D blackTexture
		{
			get
			{
				if (m_BlackTexture == null)
				{
					m_BlackTexture = new Texture2D(1, 1, TextureFormat.ARGB32, false)
					{
						name = "Black Texture"
					};
					m_BlackTexture.SetPixel(0, 0, Color.black);
					m_BlackTexture.Apply();
				}
				return m_BlackTexture;
			}
		}

		public static Texture3D blackTexture3D
		{
			get
			{
				if (m_BlackTexture3D == null)
				{
					m_BlackTexture3D = new Texture3D(1, 1, 1, TextureFormat.ARGB32, false)
					{
						name = "Black Texture 3D"
					};
					m_BlackTexture3D.SetPixels(new Color[1] { Color.black });
					m_BlackTexture3D.Apply();
				}
				return m_BlackTexture3D;
			}
		}

		public static Texture2D transparentTexture
		{
			get
			{
				if (m_TransparentTexture == null)
				{
					m_TransparentTexture = new Texture2D(1, 1, TextureFormat.ARGB32, false)
					{
						name = "Transparent Texture"
					};
					m_TransparentTexture.SetPixel(0, 0, Color.clear);
					m_TransparentTexture.Apply();
				}
				return m_TransparentTexture;
			}
		}

		public static Texture3D transparentTexture3D
		{
			get
			{
				if (m_TransparentTexture3D == null)
				{
					m_TransparentTexture3D = new Texture3D(1, 1, 1, TextureFormat.ARGB32, false)
					{
						name = "Transparent Texture 3D"
					};
					m_TransparentTexture3D.SetPixels(new Color[1] { Color.clear });
					m_TransparentTexture3D.Apply();
				}
				return m_TransparentTexture3D;
			}
		}

		public static Mesh fullscreenTriangle
		{
			get
			{
				if (s_FullscreenTriangle != null)
				{
					return s_FullscreenTriangle;
				}
				s_FullscreenTriangle = new Mesh
				{
					name = "Fullscreen Triangle"
				};
				s_FullscreenTriangle.SetVertices(new List<Vector3>
				{
					new Vector3(-1f, -1f, 0f),
					new Vector3(-1f, 3f, 0f),
					new Vector3(3f, -1f, 0f)
				});
				s_FullscreenTriangle.SetIndices(new int[3] { 0, 1, 2 }, MeshTopology.Triangles, 0, false);
				s_FullscreenTriangle.UploadMeshData(false);
				return s_FullscreenTriangle;
			}
		}

		public static Material copyStdMaterial
		{
			get
			{
				if (s_CopyStdMaterial != null)
				{
					return s_CopyStdMaterial;
				}
				Assert.IsNotNull(s_Resources);
				s_CopyStdMaterial = new Material(s_Resources.shaders.copyStd)
				{
					name = "PostProcess - CopyStd",
					hideFlags = HideFlags.HideAndDontSave
				};
				return s_CopyStdMaterial;
			}
		}

		public static Material copyStdFromDoubleWideMaterial
		{
			get
			{
				if (s_CopyStdFromDoubleWideMaterial != null)
				{
					return s_CopyStdFromDoubleWideMaterial;
				}
				Assert.IsNotNull(s_Resources);
				s_CopyStdFromDoubleWideMaterial = new Material(s_Resources.shaders.copyStdFromDoubleWide)
				{
					name = "PostProcess - CopyStdFromDoubleWide",
					hideFlags = HideFlags.HideAndDontSave
				};
				return s_CopyStdFromDoubleWideMaterial;
			}
		}

		public static Material copyMaterial
		{
			get
			{
				if (s_CopyMaterial != null)
				{
					return s_CopyMaterial;
				}
				Assert.IsNotNull(s_Resources);
				s_CopyMaterial = new Material(s_Resources.shaders.copy)
				{
					name = "PostProcess - Copy",
					hideFlags = HideFlags.HideAndDontSave
				};
				return s_CopyMaterial;
			}
		}

		public static Material copyFromTexArrayMaterial
		{
			get
			{
				if (s_CopyFromTexArrayMaterial != null)
				{
					return s_CopyFromTexArrayMaterial;
				}
				Assert.IsNotNull(s_Resources);
				s_CopyFromTexArrayMaterial = new Material(s_Resources.shaders.copyStdFromTexArray)
				{
					name = "PostProcess - CopyFromTexArray",
					hideFlags = HideFlags.HideAndDontSave
				};
				return s_CopyFromTexArrayMaterial;
			}
		}

		public static PropertySheet copySheet
		{
			get
			{
				if (s_CopySheet == null)
				{
					s_CopySheet = new PropertySheet(copyMaterial);
				}
				return s_CopySheet;
			}
		}

		public static PropertySheet copyFromTexArraySheet
		{
			get
			{
				if (s_CopyFromTexArraySheet == null)
				{
					s_CopyFromTexArraySheet = new PropertySheet(copyFromTexArrayMaterial);
				}
				return s_CopyFromTexArraySheet;
			}
		}

		public static bool scriptableRenderPipelineActive => GraphicsSettings.renderPipelineAsset != null;

		public static bool supportsDeferredShading
		{
			get
			{
				if (!scriptableRenderPipelineActive)
				{
					return GraphicsSettings.GetShaderMode(BuiltinShaderType.DeferredShading) != BuiltinShaderMode.Disabled;
				}
				return true;
			}
		}

		public static bool supportsDepthNormals
		{
			get
			{
				if (!scriptableRenderPipelineActive)
				{
					return GraphicsSettings.GetShaderMode(BuiltinShaderType.DepthNormals) != BuiltinShaderMode.Disabled;
				}
				return true;
			}
		}

		public static bool isSinglePassStereoEnabled => false;

		public static bool isVREnabled => false;

		public static bool isAndroidOpenGL
		{
			get
			{
				if (Application.platform == RuntimePlatform.Android)
				{
					return SystemInfo.graphicsDeviceType != GraphicsDeviceType.Vulkan;
				}
				return false;
			}
		}

		public static RenderTextureFormat defaultHDRRenderTextureFormat => RenderTextureFormat.DefaultHDR;

		public static bool isLinearColorSpace => QualitySettings.activeColorSpace == ColorSpace.Linear;

		public static Texture2D GetLutStrip(int size)
		{
			Texture2D value;
			if (!m_LutStrips.TryGetValue(size, out value))
			{
				int num = size * size;
				Color[] array = new Color[num * size];
				float num2 = 1f / ((float)size - 1f);
				for (int i = 0; i < size; i++)
				{
					int num3 = i * size;
					float b = (float)i * num2;
					for (int j = 0; j < size; j++)
					{
						float g = (float)j * num2;
						for (int k = 0; k < size; k++)
						{
							float r = (float)k * num2;
							array[j * num + num3 + k] = new Color(r, g, b);
						}
					}
				}
				TextureFormat textureFormat = TextureFormat.RGBAHalf;
				if (!textureFormat.IsSupported())
				{
					textureFormat = TextureFormat.ARGB32;
				}
				value = new Texture2D(size * size, size, textureFormat, false, true)
				{
					name = "Strip Lut" + size,
					hideFlags = HideFlags.DontSave,
					filterMode = FilterMode.Bilinear,
					wrapMode = TextureWrapMode.Clamp,
					anisoLevel = 0
				};
				value.SetPixels(array);
				value.Apply();
				m_LutStrips.Add(size, value);
			}
			return value;
		}

		public static void SetRenderTargetWithLoadStoreAction(this CommandBuffer cmd, RenderTargetIdentifier rt, RenderBufferLoadAction loadAction, RenderBufferStoreAction storeAction)
		{
			cmd.SetRenderTarget(rt, loadAction, storeAction);
		}

		public static void SetRenderTargetWithLoadStoreAction(this CommandBuffer cmd, RenderTargetIdentifier color, RenderBufferLoadAction colorLoadAction, RenderBufferStoreAction colorStoreAction, RenderTargetIdentifier depth, RenderBufferLoadAction depthLoadAction, RenderBufferStoreAction depthStoreAction)
		{
			cmd.SetRenderTarget(color, colorLoadAction, colorStoreAction, depth, depthLoadAction, depthStoreAction);
		}

		public static void BlitFullscreenTriangle(this CommandBuffer cmd, RenderTargetIdentifier source, RenderTargetIdentifier destination, bool clear = false, Rect? viewport = null)
		{
			cmd.SetGlobalTexture(ShaderIDs.MainTex, source);
			SetRenderTargetWithLoadStoreAction(cmd, destination, (!viewport.HasValue) ? RenderBufferLoadAction.DontCare : RenderBufferLoadAction.Load, RenderBufferStoreAction.Store);
			if (viewport.HasValue)
			{
				cmd.SetViewport(viewport.Value);
			}
			if (clear)
			{
				cmd.ClearRenderTarget(true, true, Color.clear);
			}
			cmd.DrawMesh(fullscreenTriangle, Matrix4x4.identity, copyMaterial, 0, 0);
		}

		public static void BlitFullscreenTriangle(this CommandBuffer cmd, RenderTargetIdentifier source, RenderTargetIdentifier destination, PropertySheet propertySheet, int pass, RenderBufferLoadAction loadAction, Rect? viewport = null)
		{
			cmd.SetGlobalTexture(ShaderIDs.MainTex, source);
			bool num = loadAction == RenderBufferLoadAction.Clear;
			if (num)
			{
				loadAction = RenderBufferLoadAction.DontCare;
			}
			SetRenderTargetWithLoadStoreAction(cmd, destination, (!viewport.HasValue) ? loadAction : RenderBufferLoadAction.Load, RenderBufferStoreAction.Store);
			if (viewport.HasValue)
			{
				cmd.SetViewport(viewport.Value);
			}
			if (num)
			{
				cmd.ClearRenderTarget(true, true, Color.clear);
			}
			cmd.DrawMesh(fullscreenTriangle, Matrix4x4.identity, propertySheet.material, 0, pass, propertySheet.properties);
		}

		public static void BlitFullscreenTriangle(this CommandBuffer cmd, RenderTargetIdentifier source, RenderTargetIdentifier destination, PropertySheet propertySheet, int pass, bool clear = false, Rect? viewport = null)
		{
			BlitFullscreenTriangle(cmd, source, destination, propertySheet, pass, clear ? RenderBufferLoadAction.Clear : RenderBufferLoadAction.DontCare, viewport);
		}

		public static void BlitFullscreenTriangleFromDoubleWide(this CommandBuffer cmd, RenderTargetIdentifier source, RenderTargetIdentifier destination, Material material, int pass, int eye)
		{
			Vector4 value = new Vector4(0.5f, 1f, 0f, 0f);
			if (eye == 1)
			{
				value.z = 0.5f;
			}
			cmd.SetGlobalVector(ShaderIDs.UVScaleOffset, value);
			BuiltinBlit(cmd, source, destination, material, pass);
		}

		public static void BlitFullscreenTriangleToDoubleWide(this CommandBuffer cmd, RenderTargetIdentifier source, RenderTargetIdentifier destination, PropertySheet propertySheet, int pass, int eye)
		{
			Vector4 value = new Vector4(0.5f, 1f, -0.5f, 0f);
			if (eye == 1)
			{
				value.z = 0.5f;
			}
			propertySheet.EnableKeyword("STEREO_DOUBLEWIDE_TARGET");
			propertySheet.properties.SetVector(ShaderIDs.PosScaleOffset, value);
			BlitFullscreenTriangle(cmd, source, destination, propertySheet, 0);
		}

		public static void BlitFullscreenTriangleFromTexArray(this CommandBuffer cmd, RenderTargetIdentifier source, RenderTargetIdentifier destination, PropertySheet propertySheet, int pass, bool clear = false, int depthSlice = -1)
		{
			cmd.SetGlobalTexture(ShaderIDs.MainTex, source);
			cmd.SetGlobalFloat(ShaderIDs.DepthSlice, depthSlice);
			SetRenderTargetWithLoadStoreAction(cmd, destination, RenderBufferLoadAction.DontCare, RenderBufferStoreAction.Store);
			if (clear)
			{
				cmd.ClearRenderTarget(true, true, Color.clear);
			}
			cmd.DrawMesh(fullscreenTriangle, Matrix4x4.identity, propertySheet.material, 0, pass, propertySheet.properties);
		}

		public static void BlitFullscreenTriangleToTexArray(this CommandBuffer cmd, RenderTargetIdentifier source, RenderTargetIdentifier destination, PropertySheet propertySheet, int pass, bool clear = false, int depthSlice = -1)
		{
			cmd.SetGlobalTexture(ShaderIDs.MainTex, source);
			cmd.SetGlobalFloat(ShaderIDs.DepthSlice, depthSlice);
			cmd.SetRenderTarget(destination, 0, CubemapFace.Unknown, -1);
			if (clear)
			{
				cmd.ClearRenderTarget(true, true, Color.clear);
			}
			cmd.DrawMesh(fullscreenTriangle, Matrix4x4.identity, propertySheet.material, 0, pass, propertySheet.properties);
		}

		public static void BlitFullscreenTriangle(this CommandBuffer cmd, RenderTargetIdentifier source, RenderTargetIdentifier destination, RenderTargetIdentifier depth, PropertySheet propertySheet, int pass, bool clear = false, Rect? viewport = null)
		{
			cmd.SetGlobalTexture(ShaderIDs.MainTex, source);
			RenderBufferLoadAction renderBufferLoadAction = ((!viewport.HasValue) ? RenderBufferLoadAction.DontCare : RenderBufferLoadAction.Load);
			if (clear)
			{
				SetRenderTargetWithLoadStoreAction(cmd, destination, renderBufferLoadAction, RenderBufferStoreAction.Store, depth, renderBufferLoadAction, RenderBufferStoreAction.Store);
				cmd.ClearRenderTarget(true, true, Color.clear);
			}
			else
			{
				SetRenderTargetWithLoadStoreAction(cmd, destination, renderBufferLoadAction, RenderBufferStoreAction.Store, depth, RenderBufferLoadAction.Load, RenderBufferStoreAction.Store);
			}
			if (viewport.HasValue)
			{
				cmd.SetViewport(viewport.Value);
			}
			cmd.DrawMesh(fullscreenTriangle, Matrix4x4.identity, propertySheet.material, 0, pass, propertySheet.properties);
		}

		public static void BlitFullscreenTriangle(this CommandBuffer cmd, RenderTargetIdentifier source, RenderTargetIdentifier[] destinations, RenderTargetIdentifier depth, PropertySheet propertySheet, int pass, bool clear = false, Rect? viewport = null)
		{
			cmd.SetGlobalTexture(ShaderIDs.MainTex, source);
			cmd.SetRenderTarget(destinations, depth);
			if (viewport.HasValue)
			{
				cmd.SetViewport(viewport.Value);
			}
			if (clear)
			{
				cmd.ClearRenderTarget(true, true, Color.clear);
			}
			cmd.DrawMesh(fullscreenTriangle, Matrix4x4.identity, propertySheet.material, 0, pass, propertySheet.properties);
		}

		public static void BuiltinBlit(this CommandBuffer cmd, RenderTargetIdentifier source, RenderTargetIdentifier destination)
		{
			cmd.SetRenderTarget(destination, RenderBufferLoadAction.DontCare, RenderBufferStoreAction.Store);
			destination = BuiltinRenderTextureType.CurrentActive;
			cmd.Blit(source, destination);
		}

		public static void BuiltinBlit(this CommandBuffer cmd, RenderTargetIdentifier source, RenderTargetIdentifier destination, Material mat, int pass = 0)
		{
			cmd.SetRenderTarget(destination, RenderBufferLoadAction.DontCare, RenderBufferStoreAction.Store);
			destination = BuiltinRenderTextureType.CurrentActive;
			cmd.Blit(source, destination, mat, pass);
		}

		public static void CopyTexture(CommandBuffer cmd, RenderTargetIdentifier source, RenderTargetIdentifier destination)
		{
			if (SystemInfo.copyTextureSupport > CopyTextureSupport.None)
			{
				cmd.CopyTexture(source, destination);
			}
			else
			{
				BlitFullscreenTriangle(cmd, source, destination);
			}
		}

		public static bool isFloatingPointFormat(RenderTextureFormat format)
		{
			if (format != RenderTextureFormat.DefaultHDR && format != RenderTextureFormat.ARGBHalf && format != RenderTextureFormat.ARGBFloat && format != RenderTextureFormat.RGFloat && format != RenderTextureFormat.RGHalf && format != RenderTextureFormat.RFloat && format != RenderTextureFormat.RHalf)
			{
				return format == RenderTextureFormat.RGB111110Float;
			}
			return true;
		}

		public static void Destroy(Object obj)
		{
			if (obj != null)
			{
				Object.Destroy(obj);
			}
		}

		public static bool IsResolvedDepthAvailable(Camera camera)
		{
			GraphicsDeviceType graphicsDeviceType = SystemInfo.graphicsDeviceType;
			if (camera.actualRenderingPath == RenderingPath.DeferredShading)
			{
				if (graphicsDeviceType != GraphicsDeviceType.Direct3D11 && graphicsDeviceType != GraphicsDeviceType.Direct3D12)
				{
					return graphicsDeviceType == GraphicsDeviceType.XboxOne;
				}
				return true;
			}
			return false;
		}

		public static void DestroyProfile(PostProcessProfile profile, bool destroyEffects)
		{
			if (destroyEffects)
			{
				foreach (PostProcessEffectSettings setting in profile.settings)
				{
					Destroy(setting);
				}
			}
			Destroy(profile);
		}

		public static void DestroyVolume(PostProcessVolume volume, bool destroyProfile, bool destroyGameObject = false)
		{
			if (destroyProfile)
			{
				DestroyProfile(volume.profileRef, true);
			}
			GameObject gameObject = volume.gameObject;
			Destroy(volume);
			if (destroyGameObject)
			{
				Destroy(gameObject);
			}
		}

		public static bool IsPostProcessingActive(PostProcessLayer layer)
		{
			if (layer != null)
			{
				return layer.enabled;
			}
			return false;
		}

		public static bool IsTemporalAntialiasingActive(PostProcessLayer layer)
		{
			if (IsPostProcessingActive(layer) && layer.antialiasingMode == PostProcessLayer.Antialiasing.TemporalAntialiasing)
			{
				return layer.temporalAntialiasing.IsSupported();
			}
			return false;
		}

		public static IEnumerable<T> GetAllSceneObjects<T>() where T : Component
		{
			Queue<Transform> queue = new Queue<Transform>();
			GameObject[] rootGameObjects = SceneManager.GetActiveScene().GetRootGameObjects();
			GameObject[] array = rootGameObjects;
			foreach (GameObject gameObject in array)
			{
				queue.Enqueue(gameObject.transform);
				T component = gameObject.GetComponent<T>();
				if ((Object)component != (Object)null)
				{
					yield return component;
				}
			}
			while (queue.Count > 0)
			{
				foreach (Transform item in queue.Dequeue())
				{
					queue.Enqueue(item);
					T component2 = item.GetComponent<T>();
					if ((Object)component2 != (Object)null)
					{
						yield return component2;
					}
				}
			}
		}

		public static void CreateIfNull<T>(ref T obj) where T : class, new()
		{
			if (obj == null)
			{
				obj = new T();
			}
		}

		public static float Exp2(float x)
		{
			return Mathf.Exp(x * 0.6931472f);
		}

		public static Matrix4x4 GetJitteredPerspectiveProjectionMatrix(Camera camera, Vector2 offset)
		{
			float nearClipPlane = camera.nearClipPlane;
			float farClipPlane = camera.farClipPlane;
			float num = Mathf.Tan((float)Math.PI / 360f * camera.fieldOfView) * nearClipPlane;
			float num2 = num * camera.aspect;
			offset.x *= num2 / (0.5f * (float)camera.pixelWidth);
			offset.y *= num / (0.5f * (float)camera.pixelHeight);
			Matrix4x4 projectionMatrix = camera.projectionMatrix;
			projectionMatrix[0, 2] += offset.x / num2;
			projectionMatrix[1, 2] += offset.y / num;
			return projectionMatrix;
		}

		public static Matrix4x4 GetJitteredOrthographicProjectionMatrix(Camera camera, Vector2 offset)
		{
			float orthographicSize = camera.orthographicSize;
			float num = orthographicSize * camera.aspect;
			offset.x *= num / (0.5f * (float)camera.pixelWidth);
			offset.y *= orthographicSize / (0.5f * (float)camera.pixelHeight);
			float left = offset.x - num;
			float right = offset.x + num;
			float top = offset.y + orthographicSize;
			float bottom = offset.y - orthographicSize;
			return Matrix4x4.Ortho(left, right, bottom, top, camera.nearClipPlane, camera.farClipPlane);
		}

		public static Matrix4x4 GenerateJitteredProjectionMatrixFromOriginal(PostProcessRenderContext context, Matrix4x4 origProj, Vector2 jitter)
		{
			FrustumPlanes decomposeProjection = origProj.decomposeProjection;
			float num = Math.Abs(decomposeProjection.top) + Math.Abs(decomposeProjection.bottom);
			float num2 = Math.Abs(decomposeProjection.left) + Math.Abs(decomposeProjection.right);
			Vector2 vector = new Vector2(jitter.x * num2 / (float)context.screenWidth, jitter.y * num / (float)context.screenHeight);
			decomposeProjection.left += vector.x;
			decomposeProjection.right += vector.x;
			decomposeProjection.top += vector.y;
			decomposeProjection.bottom += vector.y;
			return Matrix4x4.Frustum(decomposeProjection);
		}

		public static IEnumerable<Type> GetAllAssemblyTypes()
		{
			if (m_AssemblyTypes == null)
			{
				m_AssemblyTypes = AppDomain.CurrentDomain.GetAssemblies().SelectMany(delegate(Assembly t)
				{
					Type[] result = new Type[0];
					try
					{
						result = t.GetTypes();
						return result;
					}
					catch
					{
						return result;
					}
				});
			}
			return m_AssemblyTypes;
		}

		public static T GetAttribute<T>(this Type type) where T : Attribute
		{
			Assert.IsTrue(type.IsDefined(typeof(T), false), "Attribute not found");
			return (T)type.GetCustomAttributes(typeof(T), false)[0];
		}

		public static Attribute[] GetMemberAttributes<TType, TValue>(Expression<Func<TType, TValue>> expr)
		{
			Expression expression = expr;
			if (expression is LambdaExpression)
			{
				expression = ((LambdaExpression)expression).Body;
			}
			ExpressionType nodeType = expression.NodeType;
			if (nodeType == ExpressionType.MemberAccess)
			{
				return ((FieldInfo)((MemberExpression)expression).Member).GetCustomAttributes(false).Cast<Attribute>().ToArray();
			}
			throw new InvalidOperationException();
		}

		public static string GetFieldPath<TType, TValue>(Expression<Func<TType, TValue>> expr)
		{
			ExpressionType nodeType = expr.Body.NodeType;
			if (nodeType == ExpressionType.MemberAccess)
			{
				MemberExpression memberExpression = expr.Body as MemberExpression;
				List<string> list = new List<string>();
				while (memberExpression != null)
				{
					list.Add(memberExpression.Member.Name);
					memberExpression = memberExpression.Expression as MemberExpression;
				}
				StringBuilder stringBuilder = new StringBuilder();
				for (int num = list.Count - 1; num >= 0; num--)
				{
					stringBuilder.Append(list[num]);
					if (num > 0)
					{
						stringBuilder.Append('.');
					}
				}
				return stringBuilder.ToString();
			}
			throw new InvalidOperationException();
		}
	}
}
