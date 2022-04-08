using UnityEngine;

namespace UnityStandardAssets.CinematicEffects;

public static class ImageEffectHelper
{
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
		if (!SystemInfo.supportsImageEffects || !SystemInfo.supportsRenderTextures)
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
}
