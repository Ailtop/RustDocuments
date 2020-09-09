using System;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace CompanionServer
{
	public static class MapImageRenderer
	{
		[IsReadOnly]
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

		private static readonly Vector3 StartColor = new Vector3(0.324313372f, 0.397058845f, 0.195609868f);

		private static readonly Vector4 WaterColor = new Vector4(0.269668937f, 0.4205476f, 0.5660378f, 1f);

		private static readonly Vector4 GravelColor = new Vector4(0.139705867f, 0.132621378f, 0.114024632f, 0.372f);

		private static readonly Vector4 DirtColor = new Vector4(0.322227329f, 0.375f, 0.228860289f, 1f);

		private static readonly Vector4 SandColor = new Vector4(1f, 0.8250507f, 61f / 136f, 1f);

		private static readonly Vector4 GrassColor = new Vector4(23f / 51f, 47f / 85f, 23f / 85f, 1f);

		private static readonly Vector4 ForestColor = new Vector4(47f / 85f, 0.440000027f, 23f / 85f, 1f);

		private static readonly Vector4 RockColor = new Vector4(0.42344287f, 33f / 68f, 0.314013839f, 1f);

		private static readonly Vector4 SnowColor = new Vector4(55f / 68f, 55f / 68f, 55f / 68f, 1f);

		private static readonly Vector4 PebbleColor = new Vector4(31f / 255f, 107f / 255f, 32f / 51f, 1f);

		private static readonly Vector4 OffShoreColor = new Vector4(0.166295841f, 0.259337664f, 37f / 106f, 1f);

		private static readonly Vector3 SunDirection = Vector3.Normalize(new Vector3(0.95f, 2.87f, 2.37f));

		private const float SunPower = 0.5f;

		private const float Brightness = 1f;

		private const float Contrast = 0.87f;

		private const float OceanWaterLevel = 0f;

		private static readonly Vector3 Half = new Vector3(0.5f, 0.5f, 0.5f);

		public static byte[] Render(out int imageWidth, out int imageHeight, out Color background)
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
			int mapRes = (int)(World.Size / 2u);
			float invMapRes = 1f / (float)mapRes;
			if (mapRes <= 0)
			{
				return null;
			}
			imageWidth = mapRes + 1000;
			imageHeight = mapRes + 1000;
			Color[] array = new Color[imageWidth * imageHeight];
			Array2D<Color> output = new Array2D<Color>(array, imageWidth, imageHeight);
			_003C_003Ec__DisplayClass17_0 CS_0024_003C_003E8__locals0;
			Parallel.For(0, imageHeight, delegate(int y)
			{
				y -= 500;
				float y2 = (float)y * invMapRes;
				int num = mapRes + 500;
				for (int i = -500; i < num; i++)
				{
					float x = (float)i * invMapRes;
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
					startColor += (num3 - 0.5f) * 0.5f * startColor;
					startColor = (startColor - Half) * 0.87f + Half;
					startColor *= 1f;
					output[i + 500, y + 500] = new Color(startColor.x, startColor.y, startColor.z);
				}
			});
			background = output[0, 0];
			return EncodeToJPG(imageWidth, imageHeight, array);
		}

		private static byte[] EncodeToJPG(int width, int height, Color[] pixels)
		{
			Texture2D texture2D = null;
			try
			{
				texture2D = new Texture2D(width, height);
				texture2D.SetPixels(pixels);
				texture2D.Apply();
				return texture2D.EncodeToJPG(85);
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
}
