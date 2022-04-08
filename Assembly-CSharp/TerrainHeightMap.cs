using System;
using UnityEngine;

public class TerrainHeightMap : TerrainMap<short>
{
	public Texture2D HeightTexture;

	public Texture2D NormalTexture;

	private float normY;

	public override void Setup()
	{
		if (HeightTexture != null)
		{
			if (HeightTexture.width == HeightTexture.height)
			{
				res = HeightTexture.width;
				src = (dst = new short[res * res]);
				Color32[] pixels = HeightTexture.GetPixels32();
				int i = 0;
				int num = 0;
				for (; i < res; i++)
				{
					int num2 = 0;
					while (num2 < res)
					{
						Color32 c = pixels[num];
						dst[i * res + num2] = BitUtility.DecodeShort(c);
						num2++;
						num++;
					}
				}
			}
			else
			{
				Debug.LogError("Invalid height texture: " + HeightTexture.name);
			}
		}
		else
		{
			res = terrain.terrainData.heightmapResolution;
			src = (dst = new short[res * res]);
		}
		normY = TerrainMeta.Size.x / TerrainMeta.Size.y / (float)res;
	}

	public void ApplyToTerrain()
	{
		float[,] heights = terrain.terrainData.GetHeights(0, 0, res, res);
		Parallel.For(0, res, delegate(int z)
		{
			for (int i = 0; i < res; i++)
			{
				heights[z, i] = GetHeight01(i, z);
			}
		});
		terrain.terrainData.SetHeights(0, 0, heights);
		TerrainCollider component = terrain.GetComponent<TerrainCollider>();
		if ((bool)component)
		{
			component.enabled = false;
			component.enabled = true;
		}
	}

	public void GenerateTextures(bool heightTexture = true, bool normalTexture = true)
	{
		if (heightTexture)
		{
			Color32[] heights = new Color32[res * res];
			Parallel.For(0, res, delegate(int z)
			{
				for (int j = 0; j < res; j++)
				{
					heights[z * res + j] = BitUtility.EncodeShort(src[z * res + j]);
				}
			});
			HeightTexture = new Texture2D(res, res, TextureFormat.RGBA32, mipChain: true, linear: true);
			HeightTexture.name = "HeightTexture";
			HeightTexture.wrapMode = TextureWrapMode.Clamp;
			HeightTexture.SetPixels32(heights);
		}
		if (!normalTexture)
		{
			return;
		}
		int normalres = (res - 1) / 2;
		Color32[] normals = new Color32[normalres * normalres];
		Parallel.For(0, normalres, delegate(int z)
		{
			float normZ = ((float)z + 0.5f) / (float)normalres;
			for (int i = 0; i < normalres; i++)
			{
				float normX = ((float)i + 0.5f) / (float)normalres;
				Vector3 normal = GetNormal(normX, normZ);
				float value = Vector3.Angle(Vector3.up, normal);
				float t = Mathf.InverseLerp(50f, 70f, value);
				normal = Vector3.Slerp(normal, Vector3.up, t);
				normals[z * normalres + i] = BitUtility.EncodeNormal(normal);
			}
		});
		NormalTexture = new Texture2D(normalres, normalres, TextureFormat.RGBA32, mipChain: false, linear: true);
		NormalTexture.name = "NormalTexture";
		NormalTexture.wrapMode = TextureWrapMode.Clamp;
		NormalTexture.SetPixels32(normals);
	}

	public void ApplyTextures()
	{
		HeightTexture.Apply(updateMipmaps: true, makeNoLongerReadable: false);
		NormalTexture.Apply(updateMipmaps: true, makeNoLongerReadable: false);
		NormalTexture.Compress(highQuality: false);
		HeightTexture.Apply(updateMipmaps: false, makeNoLongerReadable: true);
		NormalTexture.Apply(updateMipmaps: false, makeNoLongerReadable: true);
	}

	public float GetHeight(Vector3 worldPos)
	{
		return TerrainMeta.Position.y + GetHeight01(worldPos) * TerrainMeta.Size.y;
	}

	public float GetHeight(float normX, float normZ)
	{
		return TerrainMeta.Position.y + GetHeight01(normX, normZ) * TerrainMeta.Size.y;
	}

	public float GetHeightFast(Vector2 uv)
	{
		int num = res - 1;
		float num2 = uv.x * (float)num;
		float num3 = uv.y * (float)num;
		int num4 = (int)num2;
		int num5 = (int)num3;
		float num6 = num2 - (float)num4;
		float num7 = num3 - (float)num5;
		num4 = ((num4 >= 0) ? num4 : 0);
		num5 = ((num5 >= 0) ? num5 : 0);
		num4 = ((num4 <= num) ? num4 : num);
		num5 = ((num5 <= num) ? num5 : num);
		int num8 = ((num2 < (float)num) ? 1 : 0);
		int num9 = ((num3 < (float)num) ? res : 0);
		int num10 = num5 * res + num4;
		int num11 = num10 + num8;
		int num12 = num10 + num9;
		int num13 = num12 + num8;
		float num14 = (float)src[num10] * 3.051944E-05f;
		float num15 = (float)src[num11] * 3.051944E-05f;
		float num16 = (float)src[num12] * 3.051944E-05f;
		float num17 = (float)src[num13] * 3.051944E-05f;
		float num18 = (num15 - num14) * num6 + num14;
		float num19 = ((num17 - num16) * num6 + num16 - num18) * num7 + num18;
		return TerrainMeta.Position.y + num19 * TerrainMeta.Size.y;
	}

	public float GetHeight(int x, int z)
	{
		return TerrainMeta.Position.y + GetHeight01(x, z) * TerrainMeta.Size.y;
	}

	public float GetHeight01(Vector3 worldPos)
	{
		float normX = TerrainMeta.NormalizeX(worldPos.x);
		float normZ = TerrainMeta.NormalizeZ(worldPos.z);
		return GetHeight01(normX, normZ);
	}

	public float GetHeight01(float normX, float normZ)
	{
		int num = res - 1;
		float num2 = normX * (float)num;
		float num3 = normZ * (float)num;
		int num4 = Mathf.Clamp((int)num2, 0, num);
		int num5 = Mathf.Clamp((int)num3, 0, num);
		int x = Mathf.Min(num4 + 1, num);
		int z = Mathf.Min(num5 + 1, num);
		float height = GetHeight01(num4, num5);
		float height2 = GetHeight01(x, num5);
		float height3 = GetHeight01(num4, z);
		float height4 = GetHeight01(x, z);
		float t = num2 - (float)num4;
		float t2 = num3 - (float)num5;
		float a = Mathf.Lerp(height, height2, t);
		float b = Mathf.Lerp(height3, height4, t);
		return Mathf.Lerp(a, b, t2);
	}

	public float GetTriangulatedHeight01(float normX, float normZ)
	{
		int num = res - 1;
		float num2 = normX * (float)num;
		float num3 = normZ * (float)num;
		int num4 = Mathf.Clamp((int)num2, 0, num);
		int num5 = Mathf.Clamp((int)num3, 0, num);
		int x = Mathf.Min(num4 + 1, num);
		int z = Mathf.Min(num5 + 1, num);
		float num6 = num2 - (float)num4;
		float num7 = num3 - (float)num5;
		float height = GetHeight01(num4, num5);
		float height2 = GetHeight01(x, z);
		if (num6 > num7)
		{
			float height3 = GetHeight01(x, num5);
			return height + (height3 - height) * num6 + (height2 - height3) * num7;
		}
		float height4 = GetHeight01(num4, z);
		return height + (height2 - height4) * num6 + (height4 - height) * num7;
	}

	public float GetHeight01(int x, int z)
	{
		return BitUtility.Short2Float(src[z * res + x]);
	}

	private float GetSrcHeight01(int x, int z)
	{
		return BitUtility.Short2Float(src[z * res + x]);
	}

	private float GetDstHeight01(int x, int z)
	{
		return BitUtility.Short2Float(dst[z * res + x]);
	}

	public Vector3 GetNormal(Vector3 worldPos)
	{
		float normX = TerrainMeta.NormalizeX(worldPos.x);
		float normZ = TerrainMeta.NormalizeZ(worldPos.z);
		return GetNormal(normX, normZ);
	}

	public Vector3 GetNormal(float normX, float normZ)
	{
		int num = res - 1;
		float num2 = normX * (float)num;
		float num3 = normZ * (float)num;
		int num4 = Mathf.Clamp((int)num2, 0, num);
		int num5 = Mathf.Clamp((int)num3, 0, num);
		int x = Mathf.Min(num4 + 1, num);
		int z = Mathf.Min(num5 + 1, num);
		Vector3 normal = GetNormal(num4, num5);
		Vector3 normal2 = GetNormal(x, num5);
		Vector3 normal3 = GetNormal(num4, z);
		Vector3 normal4 = GetNormal(x, z);
		float t = num2 - (float)num4;
		float t2 = num3 - (float)num5;
		Vector3 a = Vector3.Slerp(normal, normal2, t);
		Vector3 b = Vector3.Slerp(normal3, normal4, t);
		return Vector3.Slerp(a, b, t2).normalized;
	}

	public Vector3 GetNormal(int x, int z)
	{
		int max = res - 1;
		int x2 = Mathf.Clamp(x - 1, 0, max);
		int z2 = Mathf.Clamp(z - 1, 0, max);
		int x3 = Mathf.Clamp(x + 1, 0, max);
		int z3 = Mathf.Clamp(z + 1, 0, max);
		float num = (GetHeight01(x3, z2) - GetHeight01(x2, z2)) * 0.5f;
		return new Vector3(z: 0f - (GetHeight01(x2, z3) - GetHeight01(x2, z2)) * 0.5f, x: 0f - num, y: normY).normalized;
	}

	private Vector3 GetNormalSobel(int x, int z)
	{
		int num = res - 1;
		Vector3 vector = new Vector3(TerrainMeta.Size.x / (float)num, TerrainMeta.Size.y, TerrainMeta.Size.z / (float)num);
		int x2 = Mathf.Clamp(x - 1, 0, num);
		int z2 = Mathf.Clamp(z - 1, 0, num);
		int x3 = Mathf.Clamp(x + 1, 0, num);
		int z3 = Mathf.Clamp(z + 1, 0, num);
		float num2 = GetHeight01(x2, z2) * -1f;
		num2 += GetHeight01(x2, z) * -2f;
		num2 += GetHeight01(x2, z3) * -1f;
		num2 += GetHeight01(x3, z2) * 1f;
		num2 += GetHeight01(x3, z) * 2f;
		num2 += GetHeight01(x3, z3) * 1f;
		num2 *= vector.y;
		num2 /= vector.x;
		float num3 = GetHeight01(x2, z2) * -1f;
		num3 += GetHeight01(x, z2) * -2f;
		num3 += GetHeight01(x3, z2) * -1f;
		num3 += GetHeight01(x2, z3) * 1f;
		num3 += GetHeight01(x, z3) * 2f;
		num3 += GetHeight01(x3, z3) * 1f;
		num3 *= vector.y;
		num3 /= vector.z;
		return new Vector3(0f - num2, 8f, 0f - num3).normalized;
	}

	public float GetSlope(Vector3 worldPos)
	{
		return Vector3.Angle(Vector3.up, GetNormal(worldPos));
	}

	public float GetSlope(float normX, float normZ)
	{
		return Vector3.Angle(Vector3.up, GetNormal(normX, normZ));
	}

	public float GetSlope(int x, int z)
	{
		return Vector3.Angle(Vector3.up, GetNormal(x, z));
	}

	public float GetSlope01(Vector3 worldPos)
	{
		return GetSlope(worldPos) * (1f / 90f);
	}

	public float GetSlope01(float normX, float normZ)
	{
		return GetSlope(normX, normZ) * (1f / 90f);
	}

	public float GetSlope01(int x, int z)
	{
		return GetSlope(x, z) * (1f / 90f);
	}

	public void SetHeight(Vector3 worldPos, float height)
	{
		float normX = TerrainMeta.NormalizeX(worldPos.x);
		float normZ = TerrainMeta.NormalizeZ(worldPos.z);
		SetHeight(normX, normZ, height);
	}

	public void SetHeight(float normX, float normZ, float height)
	{
		int x = Index(normX);
		int z = Index(normZ);
		SetHeight(x, z, height);
	}

	public void SetHeight(int x, int z, float height)
	{
		dst[z * res + x] = BitUtility.Float2Short(height);
	}

	public void SetHeight(Vector3 worldPos, float height, float opacity)
	{
		float normX = TerrainMeta.NormalizeX(worldPos.x);
		float normZ = TerrainMeta.NormalizeZ(worldPos.z);
		SetHeight(normX, normZ, height, opacity);
	}

	public void SetHeight(float normX, float normZ, float height, float opacity)
	{
		int x = Index(normX);
		int z = Index(normZ);
		SetHeight(x, z, height, opacity);
	}

	public void SetHeight(int x, int z, float height, float opacity)
	{
		float height2 = Mathf.SmoothStep(GetSrcHeight01(x, z), height, opacity);
		SetHeight(x, z, height2);
	}

	public void AddHeight(Vector3 worldPos, float delta)
	{
		float normX = TerrainMeta.NormalizeX(worldPos.x);
		float normZ = TerrainMeta.NormalizeZ(worldPos.z);
		AddHeight(normX, normZ, delta);
	}

	public void AddHeight(float normX, float normZ, float delta)
	{
		int x = Index(normX);
		int z = Index(normZ);
		AddHeight(x, z, delta);
	}

	public void AddHeight(int x, int z, float delta)
	{
		float height = Mathf.Clamp01(GetDstHeight01(x, z) + delta);
		SetHeight(x, z, height);
	}

	public void LowerHeight(Vector3 worldPos, float height, float opacity)
	{
		float normX = TerrainMeta.NormalizeX(worldPos.x);
		float normZ = TerrainMeta.NormalizeZ(worldPos.z);
		LowerHeight(normX, normZ, height, opacity);
	}

	public void LowerHeight(float normX, float normZ, float height, float opacity)
	{
		int x = Index(normX);
		int z = Index(normZ);
		LowerHeight(x, z, height, opacity);
	}

	public void LowerHeight(int x, int z, float height, float opacity)
	{
		float height2 = Mathf.Min(GetDstHeight01(x, z), Mathf.SmoothStep(GetSrcHeight01(x, z), height, opacity));
		SetHeight(x, z, height2);
	}

	public void RaiseHeight(Vector3 worldPos, float height, float opacity)
	{
		float normX = TerrainMeta.NormalizeX(worldPos.x);
		float normZ = TerrainMeta.NormalizeZ(worldPos.z);
		RaiseHeight(normX, normZ, height, opacity);
	}

	public void RaiseHeight(float normX, float normZ, float height, float opacity)
	{
		int x = Index(normX);
		int z = Index(normZ);
		RaiseHeight(x, z, height, opacity);
	}

	public void RaiseHeight(int x, int z, float height, float opacity)
	{
		float height2 = Mathf.Max(GetDstHeight01(x, z), Mathf.SmoothStep(GetSrcHeight01(x, z), height, opacity));
		SetHeight(x, z, height2);
	}

	public void SetHeight(Vector3 worldPos, float opacity, float radius, float fade = 0f)
	{
		float normX = TerrainMeta.NormalizeX(worldPos.x);
		float normZ = TerrainMeta.NormalizeZ(worldPos.z);
		float height = TerrainMeta.NormalizeY(worldPos.y);
		SetHeight(normX, normZ, height, opacity, radius, fade);
	}

	public void SetHeight(float normX, float normZ, float height, float opacity, float radius, float fade = 0f)
	{
		Action<int, int, float> action = delegate(int x, int z, float lerp)
		{
			if (lerp > 0f)
			{
				SetHeight(x, z, height, lerp * opacity);
			}
		};
		ApplyFilter(normX, normZ, radius, fade, action);
	}

	public void LowerHeight(Vector3 worldPos, float opacity, float radius, float fade = 0f)
	{
		float normX = TerrainMeta.NormalizeX(worldPos.x);
		float normZ = TerrainMeta.NormalizeZ(worldPos.z);
		float height = TerrainMeta.NormalizeY(worldPos.y);
		LowerHeight(normX, normZ, height, opacity, radius, fade);
	}

	public void LowerHeight(float normX, float normZ, float height, float opacity, float radius, float fade = 0f)
	{
		Action<int, int, float> action = delegate(int x, int z, float lerp)
		{
			if (lerp > 0f)
			{
				LowerHeight(x, z, height, lerp * opacity);
			}
		};
		ApplyFilter(normX, normZ, radius, fade, action);
	}

	public void RaiseHeight(Vector3 worldPos, float opacity, float radius, float fade = 0f)
	{
		float normX = TerrainMeta.NormalizeX(worldPos.x);
		float normZ = TerrainMeta.NormalizeZ(worldPos.z);
		float height = TerrainMeta.NormalizeY(worldPos.y);
		RaiseHeight(normX, normZ, height, opacity, radius, fade);
	}

	public void RaiseHeight(float normX, float normZ, float height, float opacity, float radius, float fade = 0f)
	{
		Action<int, int, float> action = delegate(int x, int z, float lerp)
		{
			if (lerp > 0f)
			{
				RaiseHeight(x, z, height, lerp * opacity);
			}
		};
		ApplyFilter(normX, normZ, radius, fade, action);
	}

	public void AddHeight(Vector3 worldPos, float delta, float radius, float fade = 0f)
	{
		float normX = TerrainMeta.NormalizeX(worldPos.x);
		float normZ = TerrainMeta.NormalizeZ(worldPos.z);
		AddHeight(normX, normZ, delta, radius, fade);
	}

	public void AddHeight(float normX, float normZ, float delta, float radius, float fade = 0f)
	{
		Action<int, int, float> action = delegate(int x, int z, float lerp)
		{
			if (lerp > 0f)
			{
				AddHeight(x, z, lerp * delta);
			}
		};
		ApplyFilter(normX, normZ, radius, fade, action);
	}
}
