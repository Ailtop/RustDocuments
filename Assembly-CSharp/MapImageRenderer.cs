using System;
using UnityEngine;

public static class MapImageRenderer
{
	private readonly struct Array2D<T>
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

	private static readonly Vector3 StartColor = new Vector3(0.28627452f, 23f / 85f, 0.24705884f);

	private static readonly Vector4 WaterColor = new Vector4(0.16941601f, 0.31755757f, 0.36200002f, 1f);

	private static readonly Vector4 GravelColor = new Vector4(0.25f, 37f / 152f, 0.22039475f, 1f);

	private static readonly Vector4 DirtColor = new Vector4(0.6f, 0.47959462f, 0.33f, 1f);

	private static readonly Vector4 SandColor = new Vector4(0.7f, 0.65968585f, 0.5277487f, 1f);

	private static readonly Vector4 GrassColor = new Vector4(0.35486364f, 0.37f, 0.2035f, 1f);

	private static readonly Vector4 ForestColor = new Vector4(0.24843751f, 0.3f, 9f / 128f, 1f);

	private static readonly Vector4 RockColor = new Vector4(0.4f, 0.39379844f, 0.37519377f, 1f);

	private static readonly Vector4 SnowColor = new Vector4(0.86274517f, 0.9294118f, 0.94117653f, 1f);

	private static readonly Vector4 PebbleColor = new Vector4(7f / 51f, 0.2784314f, 0.2761563f, 1f);

	private static readonly Vector4 OffShoreColor = new Vector4(0.04090196f, 0.22060032f, 14f / 51f, 1f);

	private static readonly Vector3 SunDirection = Vector3.Normalize(new Vector3(0.95f, 2.87f, 2.37f));

	private const float SunPower = 0.65f;

	private const float Brightness = 1.05f;

	private const float Contrast = 0.94f;

	private const float OceanWaterLevel = 0f;

	private static readonly Vector3 Half = new Vector3(0.5f, 0.5f, 0.5f);

	public static byte[] Render(out int imageWidth, out int imageHeight, out Color background, float scale = 0.5f, bool lossy = true)
	{
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
		TerrainHeightMap terrainHeightMap = instance.GetComponent<TerrainHeightMap>();
		TerrainSplatMap terrainSplatMap = instance.GetComponent<TerrainSplatMap>();
		if (component == null || component2 == null || terrainHeightMap == null || terrainSplatMap == null)
		{
			return null;
		}
		int mapRes = (int)((float)World.Size * Mathf.Clamp(scale, 0.1f, 4f));
		float invMapRes = 1f / (float)mapRes;
		if (mapRes <= 0)
		{
			return null;
		}
		imageWidth = mapRes + 1000;
		imageHeight = mapRes + 1000;
		Color[] array = new Color[imageWidth * imageHeight];
		Array2D<Color> output = new Array2D<Color>(array, imageWidth, imageHeight);
		Parallel.For(0, imageHeight, delegate(int y)
		{
			y -= 500;
			float y2 = (float)y * invMapRes;
			int num = mapRes + 500;
			for (int i = -500; i < num; i++)
			{
				float x2 = (float)i * invMapRes;
				Vector3 startColor = StartColor;
				float height = GetHeight(x2, y2);
				float num2 = Math.Max(Vector3.Dot(GetNormal(x2, y2), SunDirection), 0f);
				startColor = Vector3.Lerp(startColor, GravelColor, GetSplat(x2, y2, 128) * GravelColor.w);
				startColor = Vector3.Lerp(startColor, PebbleColor, GetSplat(x2, y2, 64) * PebbleColor.w);
				startColor = Vector3.Lerp(startColor, RockColor, GetSplat(x2, y2, 8) * RockColor.w);
				startColor = Vector3.Lerp(startColor, DirtColor, GetSplat(x2, y2, 1) * DirtColor.w);
				startColor = Vector3.Lerp(startColor, GrassColor, GetSplat(x2, y2, 16) * GrassColor.w);
				startColor = Vector3.Lerp(startColor, ForestColor, GetSplat(x2, y2, 32) * ForestColor.w);
				startColor = Vector3.Lerp(startColor, SandColor, GetSplat(x2, y2, 4) * SandColor.w);
				startColor = Vector3.Lerp(startColor, SnowColor, GetSplat(x2, y2, 2) * SnowColor.w);
				float num3 = 0f - height;
				if (num3 > 0f)
				{
					startColor = Vector3.Lerp(startColor, WaterColor, Mathf.Clamp(0.5f + num3 / 5f, 0f, 1f));
					startColor = Vector3.Lerp(startColor, OffShoreColor, Mathf.Clamp(num3 / 50f, 0f, 1f));
					num2 = 0.5f;
				}
				startColor += (num2 - 0.5f) * 0.65f * startColor;
				startColor = (startColor - Half) * 0.94f + Half;
				startColor *= 1.05f;
				output[i + 500, y + 500] = new Color(startColor.x, startColor.y, startColor.z);
			}
		});
		background = output[0, 0];
		return EncodeToFile(imageWidth, imageHeight, array, lossy);
		float GetHeight(float x, float y)
		{
			return terrainHeightMap.GetHeight(x, y);
		}
		Vector3 GetNormal(float x, float y)
		{
			return terrainHeightMap.GetNormal(x, y);
		}
		float GetSplat(float x, float y, int mask)
		{
			return terrainSplatMap.GetSplat(x, y, mask);
		}
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
