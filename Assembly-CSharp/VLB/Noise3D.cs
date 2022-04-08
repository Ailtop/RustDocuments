#define UNITY_ASSERTIONS
using UnityEngine;

namespace VLB;

public static class Noise3D
{
	private static bool ms_IsSupportedChecked;

	private static bool ms_IsSupported;

	private static Texture3D ms_NoiseTexture;

	private const HideFlags kHideFlags = HideFlags.HideAndDontSave;

	private const int kMinShaderLevel = 35;

	public static bool isSupported
	{
		get
		{
			if (!ms_IsSupportedChecked)
			{
				ms_IsSupported = SystemInfo.graphicsShaderLevel >= 35;
				if (!ms_IsSupported)
				{
					Debug.LogWarning(isNotSupportedString);
				}
				ms_IsSupportedChecked = true;
			}
			return ms_IsSupported;
		}
	}

	public static bool isProperlyLoaded => ms_NoiseTexture != null;

	public static string isNotSupportedString => $"3D Noise requires higher shader capabilities (Shader Model 3.5 / OpenGL ES 3.0), which are not available on the current platform: graphicsShaderLevel (current/required) = {SystemInfo.graphicsShaderLevel} / {35}";

	[RuntimeInitializeOnLoadMethod]
	private static void OnStartUp()
	{
		LoadIfNeeded();
	}

	public static void LoadIfNeeded()
	{
		if (!isSupported)
		{
			return;
		}
		if (ms_NoiseTexture == null)
		{
			ms_NoiseTexture = LoadTexture3D(Config.Instance.noise3DData, Config.Instance.noise3DSize);
			if ((bool)ms_NoiseTexture)
			{
				ms_NoiseTexture.hideFlags = HideFlags.HideAndDontSave;
			}
		}
		Shader.SetGlobalTexture("_VLB_NoiseTex3D", ms_NoiseTexture);
		Shader.SetGlobalVector("_VLB_NoiseGlobal", Config.Instance.globalNoiseParam);
	}

	private static Texture3D LoadTexture3D(TextAsset textData, int size)
	{
		if (textData == null)
		{
			Debug.LogErrorFormat("Fail to open Noise 3D Data");
			return null;
		}
		byte[] bytes = textData.bytes;
		Debug.Assert(bytes != null);
		int num = Mathf.Max(0, size * size * size);
		if (bytes.Length != num)
		{
			Debug.LogErrorFormat("Noise 3D Data file has not the proper size {0}x{0}x{0}", size);
			return null;
		}
		Texture3D texture3D = new Texture3D(size, size, size, TextureFormat.Alpha8, mipChain: false);
		Color[] array = new Color[num];
		for (int i = 0; i < num; i++)
		{
			array[i] = new Color32(0, 0, 0, bytes[i]);
		}
		texture3D.SetPixels(array);
		texture3D.Apply();
		return texture3D;
	}
}
