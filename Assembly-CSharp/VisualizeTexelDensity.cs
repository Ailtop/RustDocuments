using UnityEngine;

[ExecuteInEditMode]
[ImageEffectAllowedInSceneView]
[AddComponentMenu("Rendering/Visualize Texture Density")]
public class VisualizeTexelDensity : MonoBehaviour
{
	public Shader shader;

	public string shaderTag = "RenderType";

	[Range(1f, 1024f)]
	public int texelsPerMeter = 256;

	[Range(0f, 1f)]
	public float overlayOpacity = 0.5f;

	public bool showHUD = true;

	private Camera mainCamera;

	private bool initialized;

	private int screenWidth;

	private int screenHeight;

	private Camera texelDensityCamera;

	private RenderTexture texelDensityRT;

	private Texture texelDensityGradTex;

	private Material texelDensityOverlayMat;

	private static VisualizeTexelDensity instance;

	public static VisualizeTexelDensity Instance => instance;

	private void Awake()
	{
		instance = this;
		mainCamera = GetComponent<Camera>();
	}

	private void OnEnable()
	{
		mainCamera = GetComponent<Camera>();
		screenWidth = Screen.width;
		screenHeight = Screen.height;
		LoadResources();
		initialized = true;
	}

	private void OnDisable()
	{
		SafeDestroyViewTexelDensity();
		SafeDestroyViewTexelDensityRT();
		initialized = false;
	}

	private void LoadResources()
	{
		if (texelDensityGradTex == null)
		{
			texelDensityGradTex = Resources.Load("TexelDensityGrad") as Texture;
		}
		if (texelDensityOverlayMat == null)
		{
			texelDensityOverlayMat = new Material(Shader.Find("Hidden/TexelDensityOverlay"))
			{
				hideFlags = HideFlags.DontSave
			};
		}
	}

	private void SafeDestroyViewTexelDensity()
	{
		if (texelDensityCamera != null)
		{
			Object.DestroyImmediate(texelDensityCamera.gameObject);
			texelDensityCamera = null;
		}
		if (texelDensityGradTex != null)
		{
			Resources.UnloadAsset(texelDensityGradTex);
			texelDensityGradTex = null;
		}
		if (texelDensityOverlayMat != null)
		{
			Object.DestroyImmediate(texelDensityOverlayMat);
			texelDensityOverlayMat = null;
		}
	}

	private void SafeDestroyViewTexelDensityRT()
	{
		if (texelDensityRT != null)
		{
			Graphics.SetRenderTarget(null);
			texelDensityRT.Release();
			Object.DestroyImmediate(texelDensityRT);
			texelDensityRT = null;
		}
	}

	private void UpdateViewTexelDensity(bool screenResized)
	{
		if (texelDensityCamera == null)
		{
			GameObject gameObject = new GameObject("Texel Density Camera", typeof(Camera))
			{
				hideFlags = HideFlags.HideAndDontSave
			};
			gameObject.transform.parent = mainCamera.transform;
			gameObject.transform.localPosition = Vector3.zero;
			gameObject.transform.localRotation = Quaternion.identity;
			texelDensityCamera = gameObject.GetComponent<Camera>();
			texelDensityCamera.CopyFrom(mainCamera);
			texelDensityCamera.renderingPath = RenderingPath.Forward;
			texelDensityCamera.allowMSAA = false;
			texelDensityCamera.allowHDR = false;
			texelDensityCamera.clearFlags = CameraClearFlags.Skybox;
			texelDensityCamera.depthTextureMode = DepthTextureMode.None;
			texelDensityCamera.SetReplacementShader(shader, shaderTag);
			texelDensityCamera.enabled = false;
		}
		if (texelDensityRT == null || screenResized || !texelDensityRT.IsCreated())
		{
			texelDensityCamera.targetTexture = null;
			SafeDestroyViewTexelDensityRT();
			texelDensityRT = new RenderTexture(screenWidth, screenHeight, 24, RenderTextureFormat.ARGB32)
			{
				hideFlags = HideFlags.DontSave
			};
			texelDensityRT.name = "TexelDensityRT";
			texelDensityRT.filterMode = FilterMode.Point;
			texelDensityRT.wrapMode = TextureWrapMode.Clamp;
			texelDensityRT.Create();
		}
		if (texelDensityCamera.targetTexture != texelDensityRT)
		{
			texelDensityCamera.targetTexture = texelDensityRT;
		}
		Shader.SetGlobalFloat("global_TexelsPerMeter", texelsPerMeter);
		Shader.SetGlobalTexture("global_TexelDensityGrad", texelDensityGradTex);
		texelDensityCamera.fieldOfView = mainCamera.fieldOfView;
		texelDensityCamera.nearClipPlane = mainCamera.nearClipPlane;
		texelDensityCamera.farClipPlane = mainCamera.farClipPlane;
		texelDensityCamera.cullingMask = mainCamera.cullingMask;
	}

	private bool CheckScreenResized(int width, int height)
	{
		if (screenWidth != width || screenHeight != height)
		{
			screenWidth = width;
			screenHeight = height;
			return true;
		}
		return false;
	}

	private void OnRenderImage(RenderTexture source, RenderTexture destination)
	{
		if (initialized)
		{
			UpdateViewTexelDensity(CheckScreenResized(source.width, source.height));
			texelDensityCamera.Render();
			texelDensityOverlayMat.SetTexture("_TexelDensityMap", texelDensityRT);
			texelDensityOverlayMat.SetFloat("_Opacity", overlayOpacity);
			Graphics.Blit(source, destination, texelDensityOverlayMat, 0);
		}
		else
		{
			Graphics.Blit(source, destination);
		}
	}

	private void DrawGUIText(float x, float y, Vector2 size, string text, GUIStyle fontStyle)
	{
		fontStyle.normal.textColor = Color.black;
		GUI.Label(new Rect(x - 1f, y + 1f, size.x, size.y), text, fontStyle);
		GUI.Label(new Rect(x + 1f, y - 1f, size.x, size.y), text, fontStyle);
		GUI.Label(new Rect(x + 1f, y + 1f, size.x, size.y), text, fontStyle);
		GUI.Label(new Rect(x - 1f, y - 1f, size.x, size.y), text, fontStyle);
		fontStyle.normal.textColor = Color.white;
		GUI.Label(new Rect(x, y, size.x, size.y), text, fontStyle);
	}

	private void OnGUI()
	{
		if (initialized && showHUD)
		{
			string text = "Texels Per Meter";
			string text2 = "0";
			string text3 = texelsPerMeter.ToString();
			string text4 = (texelsPerMeter << 1) + "+";
			float num = texelDensityGradTex.width;
			float num2 = texelDensityGradTex.height * 2;
			float num3 = (Screen.width - texelDensityGradTex.width) / 2;
			float num4 = 32f;
			GL.PushMatrix();
			GL.LoadPixelMatrix(0f, Screen.width, Screen.height, 0f);
			Graphics.DrawTexture(new Rect(num3 - 2f, num4 - 2f, num + 4f, num2 + 4f), Texture2D.whiteTexture);
			Graphics.DrawTexture(new Rect(num3, num4, num, num2), texelDensityGradTex);
			GL.PopMatrix();
			GUIStyle gUIStyle = new GUIStyle();
			gUIStyle.fontSize = 13;
			Vector2 size = gUIStyle.CalcSize(new GUIContent(text));
			Vector2 size2 = gUIStyle.CalcSize(new GUIContent(text2));
			Vector2 size3 = gUIStyle.CalcSize(new GUIContent(text3));
			Vector2 size4 = gUIStyle.CalcSize(new GUIContent(text4));
			DrawGUIText(((float)Screen.width - size.x) / 2f, num4 - size.y - 5f, size, text, gUIStyle);
			DrawGUIText(num3, num4 + num2 + 6f, size2, text2, gUIStyle);
			DrawGUIText(((float)Screen.width - size3.x) / 2f, num4 + num2 + 6f, size3, text3, gUIStyle);
			DrawGUIText(num3 + num - size4.x, num4 + num2 + 6f, size4, text4, gUIStyle);
		}
	}
}
