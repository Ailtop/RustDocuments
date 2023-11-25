using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu(menuName = "Rust/Terrain Config")]
public class TerrainConfig : ScriptableObject
{
	[Serializable]
	public class SplatOverlay
	{
		public Color Color = new Color(1f, 1f, 1f, 0f);

		[Range(0f, 1f)]
		public float Smoothness;

		[Range(0f, 1f)]
		public float NormalIntensity = 1f;

		[Range(0f, 8f)]
		public float BlendFactor = 0.5f;

		[Range(0.01f, 32f)]
		public float BlendFalloff = 0.5f;
	}

	[Serializable]
	public class SplatType
	{
		public string Name = "";

		[FormerlySerializedAs("WarmColor")]
		public Color AridColor = Color.white;

		public SplatOverlay AridOverlay = new SplatOverlay();

		[FormerlySerializedAs("Color")]
		public Color TemperateColor = Color.white;

		public SplatOverlay TemperateOverlay = new SplatOverlay();

		[FormerlySerializedAs("ColdColor")]
		public Color TundraColor = Color.white;

		public SplatOverlay TundraOverlay = new SplatOverlay();

		[FormerlySerializedAs("ColdColor")]
		public Color ArcticColor = Color.white;

		public SplatOverlay ArcticOverlay = new SplatOverlay();

		public PhysicMaterial Material;

		public float SplatTiling = 5f;

		[Range(0f, 1f)]
		public float UVMIXMult = 0.15f;

		public float UVMIXStart;

		public float UVMIXDist = 100f;
	}

	public enum GroundType
	{
		None = 0,
		HardSurface = 1,
		Grass = 2,
		Sand = 3,
		Snow = 4,
		Dirt = 5,
		Gravel = 6
	}

	public bool CastShadows = true;

	public LayerMask GroundMask = 0;

	public LayerMask WaterMask = 0;

	public PhysicMaterial GenericMaterial;

	public PhysicMaterial WaterMaterial;

	public Material Material;

	public Material MarginMaterial;

	public Texture[] AlbedoArrays = new Texture[3];

	public Texture[] NormalArrays = new Texture[3];

	public float HeightMapErrorMin = 5f;

	public float HeightMapErrorMax = 100f;

	public float BaseMapDistanceMin = 100f;

	public float BaseMapDistanceMax = 500f;

	public float ShaderLodMin = 100f;

	public float ShaderLodMax = 600f;

	public SplatType[] Splats = new SplatType[8];

	private string snowMatName;

	private string grassMatName;

	private string sandMatName;

	private List<string> dirtMatNames;

	private List<string> stoneyMatNames;

	public Texture AlbedoArray => AlbedoArrays[Mathf.Clamp(QualitySettings.masterTextureLimit, 0, 2)];

	public Texture NormalArray => NormalArrays[Mathf.Clamp(QualitySettings.masterTextureLimit, 0, 2)];

	public PhysicMaterial[] GetPhysicMaterials()
	{
		PhysicMaterial[] array = new PhysicMaterial[Splats.Length];
		for (int i = 0; i < Splats.Length; i++)
		{
			array[i] = Splats[i].Material;
		}
		return array;
	}

	public Color[] GetAridColors()
	{
		Color[] array = new Color[Splats.Length];
		for (int i = 0; i < Splats.Length; i++)
		{
			array[i] = Splats[i].AridColor;
		}
		return array;
	}

	public void GetAridOverlayConstants(out Color[] color, out Vector4[] param)
	{
		color = new Color[Splats.Length];
		param = new Vector4[Splats.Length];
		for (int i = 0; i < Splats.Length; i++)
		{
			SplatOverlay aridOverlay = Splats[i].AridOverlay;
			color[i] = aridOverlay.Color.linear;
			param[i] = new Vector4(aridOverlay.Smoothness, aridOverlay.NormalIntensity, aridOverlay.BlendFactor, aridOverlay.BlendFalloff);
		}
	}

	public Color[] GetTemperateColors()
	{
		Color[] array = new Color[Splats.Length];
		for (int i = 0; i < Splats.Length; i++)
		{
			array[i] = Splats[i].TemperateColor;
		}
		return array;
	}

	public void GetTemperateOverlayConstants(out Color[] color, out Vector4[] param)
	{
		color = new Color[Splats.Length];
		param = new Vector4[Splats.Length];
		for (int i = 0; i < Splats.Length; i++)
		{
			SplatOverlay temperateOverlay = Splats[i].TemperateOverlay;
			color[i] = temperateOverlay.Color.linear;
			param[i] = new Vector4(temperateOverlay.Smoothness, temperateOverlay.NormalIntensity, temperateOverlay.BlendFactor, temperateOverlay.BlendFalloff);
		}
	}

	public Color[] GetTundraColors()
	{
		Color[] array = new Color[Splats.Length];
		for (int i = 0; i < Splats.Length; i++)
		{
			array[i] = Splats[i].TundraColor;
		}
		return array;
	}

	public void GetTundraOverlayConstants(out Color[] color, out Vector4[] param)
	{
		color = new Color[Splats.Length];
		param = new Vector4[Splats.Length];
		for (int i = 0; i < Splats.Length; i++)
		{
			SplatOverlay tundraOverlay = Splats[i].TundraOverlay;
			color[i] = tundraOverlay.Color.linear;
			param[i] = new Vector4(tundraOverlay.Smoothness, tundraOverlay.NormalIntensity, tundraOverlay.BlendFactor, tundraOverlay.BlendFalloff);
		}
	}

	public Color[] GetArcticColors()
	{
		Color[] array = new Color[Splats.Length];
		for (int i = 0; i < Splats.Length; i++)
		{
			array[i] = Splats[i].ArcticColor;
		}
		return array;
	}

	public void GetArcticOverlayConstants(out Color[] color, out Vector4[] param)
	{
		color = new Color[Splats.Length];
		param = new Vector4[Splats.Length];
		for (int i = 0; i < Splats.Length; i++)
		{
			SplatOverlay arcticOverlay = Splats[i].ArcticOverlay;
			color[i] = arcticOverlay.Color.linear;
			param[i] = new Vector4(arcticOverlay.Smoothness, arcticOverlay.NormalIntensity, arcticOverlay.BlendFactor, arcticOverlay.BlendFalloff);
		}
	}

	public float[] GetSplatTiling()
	{
		float[] array = new float[Splats.Length];
		for (int i = 0; i < Splats.Length; i++)
		{
			array[i] = Splats[i].SplatTiling;
		}
		return array;
	}

	public float GetMaxSplatTiling()
	{
		float num = float.MinValue;
		for (int i = 0; i < Splats.Length; i++)
		{
			if (Splats[i].SplatTiling > num)
			{
				num = Splats[i].SplatTiling;
			}
		}
		return num;
	}

	public float GetMinSplatTiling()
	{
		float num = float.MaxValue;
		for (int i = 0; i < Splats.Length; i++)
		{
			if (Splats[i].SplatTiling < num)
			{
				num = Splats[i].SplatTiling;
			}
		}
		return num;
	}

	public Vector3[] GetPackedUVMIX()
	{
		Vector3[] array = new Vector3[Splats.Length];
		for (int i = 0; i < Splats.Length; i++)
		{
			array[i] = new Vector3(Splats[i].UVMIXMult, Splats[i].UVMIXStart, Splats[i].UVMIXDist);
		}
		return array;
	}

	public GroundType GetCurrentGroundType(bool isGrounded, RaycastHit hit)
	{
		if (string.IsNullOrEmpty(grassMatName))
		{
			dirtMatNames = new List<string>();
			stoneyMatNames = new List<string>();
			SplatType[] splats = Splats;
			foreach (SplatType obj in splats)
			{
				string text = obj.Name.ToLower();
				string item = obj.Material.name;
				switch (text)
				{
				case "grass":
					grassMatName = item;
					break;
				case "snow":
					snowMatName = item;
					break;
				case "sand":
					sandMatName = item;
					break;
				case "dirt":
				case "forest":
				case "tundra":
					dirtMatNames.Add(item);
					break;
				case "stones":
				case "gravel":
					stoneyMatNames.Add(item);
					break;
				}
			}
		}
		if (!isGrounded)
		{
			return GroundType.None;
		}
		if (hit.collider == null)
		{
			return GroundType.HardSurface;
		}
		PhysicMaterial materialAt = ColliderEx.GetMaterialAt(hit.collider, hit.point);
		if (materialAt == null)
		{
			return GroundType.HardSurface;
		}
		string text2 = materialAt.name;
		if (text2 == grassMatName)
		{
			return GroundType.Grass;
		}
		if (text2 == sandMatName)
		{
			return GroundType.Sand;
		}
		if (text2 == snowMatName)
		{
			return GroundType.Snow;
		}
		for (int j = 0; j < dirtMatNames.Count; j++)
		{
			if (dirtMatNames[j] == text2)
			{
				return GroundType.Dirt;
			}
		}
		for (int k = 0; k < stoneyMatNames.Count; k++)
		{
			if (stoneyMatNames[k] == text2)
			{
				return GroundType.Gravel;
			}
		}
		return GroundType.HardSurface;
	}
}
