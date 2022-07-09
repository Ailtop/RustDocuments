using System;
using UnityEngine;

[ExecuteInEditMode]
[RequireComponent(typeof(Camera))]
[AddComponentMenu("KriptoFX/Explosion_Bloom")]
[ImageEffectAllowedInSceneView]
public class Explosion_Bloom : MonoBehaviour
{
	[Serializable]
	public struct Settings
	{
		[SerializeField]
		[Tooltip("Filters out pixels under this level of brightness.")]
		public float threshold;

		[SerializeField]
		[Range(0f, 1f)]
		[Tooltip("Makes transition between under/over-threshold gradual.")]
		public float softKnee;

		[SerializeField]
		[Range(1f, 7f)]
		[Tooltip("Changes extent of veiling effects in a screen resolution-independent fashion.")]
		public float radius;

		[SerializeField]
		[Tooltip("Blend factor of the result image.")]
		public float intensity;

		[SerializeField]
		[Tooltip("Controls filter quality and buffer resolution.")]
		public bool highQuality;

		[SerializeField]
		[Tooltip("Reduces flashing noise with an additional filter.")]
		public bool antiFlicker;

		public float thresholdGamma
		{
			get
			{
				return Mathf.Max(0f, threshold);
			}
			set
			{
				threshold = value;
			}
		}

		public float thresholdLinear
		{
			get
			{
				return Mathf.GammaToLinearSpace(thresholdGamma);
			}
			set
			{
				threshold = Mathf.LinearToGammaSpace(value);
			}
		}

		public static Settings defaultSettings
		{
			get
			{
				Settings result = default(Settings);
				result.threshold = 2f;
				result.softKnee = 0f;
				result.radius = 7f;
				result.intensity = 0.7f;
				result.highQuality = true;
				result.antiFlicker = true;
				return result;
			}
		}
	}

	[SerializeField]
	public Settings settings = Settings.defaultSettings;

	[SerializeField]
	[HideInInspector]
	private Shader m_Shader;

	private Material m_Material;

	private const int kMaxIterations = 16;

	private RenderTexture[] m_blurBuffer1 = new RenderTexture[16];

	private RenderTexture[] m_blurBuffer2 = new RenderTexture[16];

	private int m_Threshold;

	private int m_Curve;

	private int m_PrefilterOffs;

	private int m_SampleScale;

	private int m_Intensity;

	private int m_BaseTex;

	public Shader shader
	{
		get
		{
			if (m_Shader == null)
			{
				m_Shader = Shader.Find("Hidden/KriptoFX/PostEffects/Explosion_Bloom");
			}
			return m_Shader;
		}
	}

	public Material material
	{
		get
		{
			if (m_Material == null)
			{
				m_Material = CheckShaderAndCreateMaterial(shader);
			}
			return m_Material;
		}
	}

	public static bool supportsDX11
	{
		get
		{
			if (SystemInfo.graphicsShaderLevel >= 50)
			{
				return SystemInfo.supportsComputeShaders;
			}
			return false;
		}
	}

	public static bool IsSupported(Shader s, bool needDepth, bool needHdr, MonoBehaviour effect)
	{
		if (s == null || !s.isSupported)
		{
			Debug.LogWarningFormat("Missing shader for image effect {0}", effect);
			return false;
		}
		if (!SystemInfo.supportsImageEffects)
		{
			Debug.LogWarningFormat("Image effects aren't supported on this device ({0})", effect);
			return false;
		}
		if (needDepth && !SystemInfo.SupportsRenderTextureFormat(RenderTextureFormat.Depth))
		{
			Debug.LogWarningFormat("Depth textures aren't supported on this device ({0})", effect);
			return false;
		}
		if (needHdr && !SystemInfo.SupportsRenderTextureFormat(RenderTextureFormat.ARGBHalf))
		{
			Debug.LogWarningFormat("Floating point textures aren't supported on this device ({0})", effect);
			return false;
		}
		return true;
	}

	public static Material CheckShaderAndCreateMaterial(Shader s)
	{
		if (s == null || !s.isSupported)
		{
			return null;
		}
		return new Material(s)
		{
			hideFlags = HideFlags.DontSave
		};
	}

	private void Awake()
	{
		m_Threshold = Shader.PropertyToID("_Threshold");
		m_Curve = Shader.PropertyToID("_Curve");
		m_PrefilterOffs = Shader.PropertyToID("_PrefilterOffs");
		m_SampleScale = Shader.PropertyToID("_SampleScale");
		m_Intensity = Shader.PropertyToID("_Intensity");
		m_BaseTex = Shader.PropertyToID("_BaseTex");
	}

	private void OnEnable()
	{
		if (!IsSupported(shader, needDepth: true, needHdr: false, this))
		{
			base.enabled = false;
		}
	}

	private void OnDisable()
	{
		if (m_Material != null)
		{
			UnityEngine.Object.DestroyImmediate(m_Material);
		}
		m_Material = null;
	}

	private void OnRenderImage(RenderTexture source, RenderTexture destination)
	{
		bool isMobilePlatform = Application.isMobilePlatform;
		int num = source.width;
		int num2 = source.height;
		if (!settings.highQuality)
		{
			num /= 2;
			num2 /= 2;
		}
		RenderTextureFormat format = (isMobilePlatform ? RenderTextureFormat.Default : RenderTextureFormat.DefaultHDR);
		float num3 = Mathf.Log(num2, 2f) + settings.radius - 8f;
		int num4 = (int)num3;
		int num5 = Mathf.Clamp(num4, 1, 16);
		float thresholdLinear = settings.thresholdLinear;
		material.SetFloat(m_Threshold, thresholdLinear);
		float num6 = thresholdLinear * settings.softKnee + 1E-05f;
		Vector3 vector = new Vector3(thresholdLinear - num6, num6 * 2f, 0.25f / num6);
		material.SetVector(m_Curve, vector);
		bool flag = !settings.highQuality && settings.antiFlicker;
		material.SetFloat(m_PrefilterOffs, flag ? (-0.5f) : 0f);
		material.SetFloat(m_SampleScale, 0.5f + num3 - (float)num4);
		material.SetFloat(m_Intensity, Mathf.Max(0f, settings.intensity));
		RenderTexture temporary = RenderTexture.GetTemporary(num, num2, 0, format);
		Graphics.Blit(source, temporary, material, settings.antiFlicker ? 1 : 0);
		RenderTexture renderTexture = temporary;
		for (int i = 0; i < num5; i++)
		{
			m_blurBuffer1[i] = RenderTexture.GetTemporary(renderTexture.width / 2, renderTexture.height / 2, 0, format);
			Graphics.Blit(renderTexture, m_blurBuffer1[i], material, (i == 0) ? (settings.antiFlicker ? 3 : 2) : 4);
			renderTexture = m_blurBuffer1[i];
		}
		for (int num7 = num5 - 2; num7 >= 0; num7--)
		{
			RenderTexture renderTexture2 = m_blurBuffer1[num7];
			material.SetTexture(m_BaseTex, renderTexture2);
			m_blurBuffer2[num7] = RenderTexture.GetTemporary(renderTexture2.width, renderTexture2.height, 0, format);
			Graphics.Blit(renderTexture, m_blurBuffer2[num7], material, settings.highQuality ? 6 : 5);
			renderTexture = m_blurBuffer2[num7];
		}
		int num8 = 7;
		num8 += (settings.highQuality ? 1 : 0);
		material.SetTexture(m_BaseTex, source);
		Graphics.Blit(renderTexture, destination, material, num8);
		for (int j = 0; j < 16; j++)
		{
			if (m_blurBuffer1[j] != null)
			{
				RenderTexture.ReleaseTemporary(m_blurBuffer1[j]);
			}
			if (m_blurBuffer2[j] != null)
			{
				RenderTexture.ReleaseTemporary(m_blurBuffer2[j]);
			}
			m_blurBuffer1[j] = null;
			m_blurBuffer2[j] = null;
		}
		RenderTexture.ReleaseTemporary(temporary);
	}
}
