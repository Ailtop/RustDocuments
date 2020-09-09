using Rust;
using System.Collections.Generic;
using UnityEngine;

public class EffectDictionary
{
	private static Dictionary<string, string[]> effectDictionary;

	public static string GetParticle(string impactType, string materialName)
	{
		return LookupEffect("impacts", impactType, materialName);
	}

	public static string GetParticle(DamageType damageType, string materialName)
	{
		switch (damageType)
		{
		case DamageType.Bullet:
			return GetParticle("bullet", materialName);
		case DamageType.Arrow:
			return GetParticle("bullet", materialName);
		case DamageType.Blunt:
			return GetParticle("blunt", materialName);
		case DamageType.Slash:
			return GetParticle("slash", materialName);
		case DamageType.Stab:
			return GetParticle("stab", materialName);
		default:
			return GetParticle("blunt", materialName);
		}
	}

	public static string GetDecal(string impactType, string materialName)
	{
		return LookupEffect("decals", impactType, materialName);
	}

	public static string GetDecal(DamageType damageType, string materialName)
	{
		switch (damageType)
		{
		case DamageType.Bullet:
			return GetDecal("bullet", materialName);
		case DamageType.Arrow:
			return GetDecal("bullet", materialName);
		case DamageType.Blunt:
			return GetDecal("blunt", materialName);
		case DamageType.Slash:
			return GetDecal("slash", materialName);
		case DamageType.Stab:
			return GetDecal("stab", materialName);
		default:
			return GetDecal("blunt", materialName);
		}
	}

	public static string GetDisplacement(string impactType, string materialName)
	{
		return LookupEffect("displacement", impactType, materialName);
	}

	private static string LookupEffect(string category, string effect, string material)
	{
		if (effectDictionary == null)
		{
			effectDictionary = GameManifest.LoadEffectDictionary();
		}
		string format = "assets/bundled/prefabs/fx/{0}/{1}/{2}";
		string[] value;
		if (!effectDictionary.TryGetValue(StringFormatCache.Get(format, category, effect, material), out value) && !effectDictionary.TryGetValue(StringFormatCache.Get(format, category, effect, "generic"), out value))
		{
			return string.Empty;
		}
		return value[Random.Range(0, value.Length)];
	}
}
