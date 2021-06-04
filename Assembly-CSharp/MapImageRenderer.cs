using System;
using System.Runtime.CompilerServices;
using UnityEngine;

public static class MapImageRenderer
{
	[IsReadOnly]
	private struct Array2D<T>
	{
		private readonly T[] _items;

		private readonly int _width;

		private readonly int _height;

		public ref T this[int x, int y]
		{
			get
			{
				int num = Mathf.Clamp(x, 0, _width - 1);
				int num2 = Mathf.Clamp(y, 0, _height - 1);
				return ref _items[num2 * _width + num];
			}
		}

		public Array2D(T[] items, int width, int height)
		{
			_items = items;
			_width = width;
			_height = height;
		}
	}

	private static readonly Vector3 StartColor = new Vector3(73f / 255f, 23f / 85f, 0.247058839f);

	private static readonly Vector4 WaterColor = new Vector4(0.16941601f, 0.317557573f, 0.362000018f, 1f);

	private static readonly Vector4 GravelColor = new Vector4(0.25f, 37f / 152f, 0.220394745f, 1f);

	private static readonly Vector4 DirtColor = new Vector4(0.6f, 0.479594618f, 0.33f, 1f);

	private static readonly Vector4 SandColor = new Vector4(0.7f, 0.65968585f, 504f / 955f, 1f);

	private static readonly Vector4 GrassColor = new Vector4(0.354863644f, 0.37f, 0.2035f, 1f);

	private static readonly Vector4 ForestColor = new Vector4(0.248437509f, 0.3f, 9f / 128f, 1f);

	private static readonly Vector4 RockColor = new Vector4(0.4f, 254f / 645f, 0.375193775f, 1f);

	private static readonly Vector4 SnowColor = new Vector4(0.862745166f, 0.9294118f, 0.941176534f, 1f);

	private static readonly Vector4 PebbleColor = new Vector4(7f / 51f, 71f / 255f, 0.2761563f, 1f);

	private static readonly Vector4 OffShoreColor = new Vector4(0.04090196f, 0.220600322f, 14f / 51f, 1f);

	private static readonly Vector3 SunDirection = Vector3.Normalize(new Vector3(0.95f, 2.87f, 2.37f));

	private const float SunPower = 0.65f;

	private const float Brightness = 1.05f;

	private const float Contrast = 0.94f;

	private const float OceanWaterLevel = 0f;

	private static readonly Vector3 Half = new Vector3(0.5f, 0.5f, 0.5f);

	public static byte[] Render(out int imageWidth, out int imageHeight, out Color background, float scale = 0.5f, bool lossy = true)
	{
		_003C_003Ec__DisplayClass17_0 CS_0024_003C_003E8__locals0 = new _003C_003Ec__DisplayClass17_0();
		imageWidth = 0;
		imageHeight = 0;
		background = OffShoreColor;
		TerrainTexturing instance = TerrainTexturing.Instance;
		if (instance == null)
		{
			return null;
		}
		Terrain component = instance.GetComponent<Terrain>();
		TerrainMeta component2 = instance.GetComponent<TerrainMeta>();
		CS_0024_003C_003E8__locals0.terrainHeightMap = instance.GetComponent<TerrainHeightMap>();
		CS_0024_003C_003E8__locals0.terrainSplatMap = instance.GetComponent<TerrainSplatMap>();
		if (component == null || component2 == null || CS_0024_003C_003E8__locals0.terrainHeightMap == null || CS_0024_003C_003E8__locals0.terrainSplatMap == null)
		{
			return null;
		}
		CS_0024_003C_003E8__locals0.mapRes = (int)((float)World.Size * Mathf.Clamp(scale, 0.1f, 4f));
		CS_0024_003C_003E8__locals0.invMapRes = 1f / (float)CS_0024_003C_003E8__locals0.mapRes;
		if (CS_0024_003C_003E8__locals0.mapRes <= 0)
		{
			return null;
		}
		imageWidth = CS_0024_003C_003E8__locals0.mapRes + 1000;
		imageHeight = CS_0024_003C_003E8__locals0.mapRes + 1000;
		Color[] array = new Color[imageWidth * imageHeight];
		CS_0024_003C_003E8__locals0.output = new Array2D<Color>(array, imageWidth, imageHeight);
		Parallel.For(0, imageHeight, delegate(int y)
		{
			y -= 500;
			float y2 = (float)y * CS_0024_003C_003E8__locals0.invMapRes;
			int num = CS_0024_003C_003E8__locals0.mapRes + 500;
			for (int i = -500; i < num; i++)
			{
				float x = (float)i * CS_0024_003C_003E8__locals0.invMapRes;
				Vector3 startColor = StartColor;
				float num2 = CS_0024_003C_003E8__locals0._003CRender_003Eg__GetHeight_007C0(x, y2);
				float num3 = Math.Max(Vector3.Dot(CS_0024_003C_003E8__locals0._003CRender_003Eg__GetNormal_007C1(x, y2), SunDirection), 0f);
				startColor = Vector3.Lerp(startColor, GravelColor, CS_0024_003C_003E8__locals0._003CRender_003Eg__GetSplat_007C2(x, y2, 128) * GravelColor.w);
				startColor = Vector3.Lerp(startColor, PebbleColor, CS_0024_003C_003E8__locals0._003CRender_003Eg__GetSplat_007C2(x, y2, 64) * PebbleColor.w);
				startColor = Vector3.Lerp(startColor, RockColor, CS_0024_003C_003E8__locals0._003CRender_003Eg__GetSplat_007C2(x, y2, 8) * RockColor.w);
				startColor = Vector3.Lerp(startColor, DirtColor, CS_0024_003C_003E8__locals0._003CRender_003Eg__GetSplat_007C2(x, y2, 1) * DirtColor.w);
				startColor = Vector3.Lerp(startColor, GrassColor, CS_0024_003C_003E8__locals0._003CRender_003Eg__GetSplat_007C2(x, y2, 16) * GrassColor.w);
				startColor = Vector3.Lerp(startColor, ForestColor, CS_0024_003C_003E8__locals0._003CRender_003Eg__GetSplat_007C2(x, y2, 32) * ForestColor.w);
				startColor = Vector3.Lerp(startColor, SandColor, CS_0024_003C_003E8__locals0._003CRender_003Eg__GetSplat_007C2(x, y2, 4) * SandColor.w);
				startColor = Vector3.Lerp(startColor, SnowColor, CS_0024_003C_003E8__locals0._003CRender_003Eg__GetSplat_007C2(x, y2, 2) * SnowColor.w);
				float num4 = 0f - num2;
				if (num4 > 0f)
				{
					startColor = Vector3.Lerp(startColor, WaterColor, Mathf.Clamp(0.5f + num4 / 5f, 0f, 1f));
					startColor = Vector3.Lerp(startColor, OffShoreColor, Mathf.Clamp(num4 / 50f, 0f, 1f));
					num3 = 0.5f;
				}
				startColor += (num3 - 0.5f) * 0.65f * startColor;
				startColor = (startColor - Half) * 0.94f + Half;
				startColor *= 1.05f;
				CS_0024_003C_003E8__locals0.output[i + 500, y + 500] = new Color(startColor.x, startColor.y, startColor.z);
			}
		});
		background = CS_0024_003C_003E8__locals0.output[0, 0];
		return EncodeToFile(imageWidth, imageHeight, array, lossy);
	}

	private static byte[] EncodeToFile(int width, int height, Color[] pixels, bool lossy)
	{
		Texture2D texture2D = null;
		try
		{
			texture2D = new Texture2D(width, height);
			texture2D.SetPixels(pixels);
			texture2D.Apply();
			return lossy ? texture2D.EncodeToJPG(85) : texture2D.EncodeToPNG();
		}
		finally
		{
			if (texture2D != null)
			{
				UnityEngine.Object.Destroy(texture2D);
			}
		}
	}

	private static Vector3 UnpackNormal(Vector4 value)
	{
		value.x *= value.w;
		Vector3 result = default(Vector3);
		result.x = value.x * 2f - 1f;
		result.y = value.y * 2f - 1f;
		Vector2 vector = new Vector2(result.x, result.y);
		result.z = Mathf.Sqrt(1f - Mathf.Clamp(Vector2.Dot(vector, vector), 0f, 1f));
		return result;
	}
}
