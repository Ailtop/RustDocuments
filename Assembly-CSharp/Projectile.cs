using System;
using System.Collections.Generic;
using ConVar;
using Rust;
using UnityEngine;

public class Projectile : BaseMonoBehaviour
{
	public struct Modifier
	{
		public float damageScale;

		public float damageOffset;

		public float distanceScale;

		public float distanceOffset;

		public static Modifier Default = new Modifier
		{
			damageScale = 1f,
			damageOffset = 0f,
			distanceScale = 1f,
			distanceOffset = 0f
		};
	}

	public const float moveDeltaTime = 1f / 32f;

	public const float lifeTime = 8f;

	[Header("Attributes")]
	public Vector3 initialVelocity;

	public float drag;

	public float gravityModifier = 1f;

	public float thickness;

	[Tooltip("This projectile will raycast for this many units, and then become a projectile. This is typically done for bullets.")]
	public float initialDistance;

	[Header("Impact Rules")]
	public bool remainInWorld;

	[Range(0f, 1f)]
	public float stickProbability = 1f;

	[Range(0f, 1f)]
	public float breakProbability;

	[Range(0f, 1f)]
	public float conditionLoss;

	[Range(0f, 1f)]
	public float ricochetChance;

	public float penetrationPower = 1f;

	[Header("Damage")]
	public DamageProperties damageProperties;

	[Horizontal(2, -1)]
	public MinMax damageDistances = new MinMax(10f, 100f);

	[Horizontal(2, -1)]
	public MinMax damageMultipliers = new MinMax(1f, 0.8f);

	public List<DamageTypeEntry> damageTypes = new List<DamageTypeEntry>();

	[Header("Rendering")]
	public ScaleRenderer rendererToScale;

	public ScaleRenderer firstPersonRenderer;

	public bool createDecals = true;

	[Header("Effects")]
	public bool doDefaultHitEffects = true;

	[Header("Audio")]
	public SoundDefinition flybySound;

	public float flybySoundDistance = 7f;

	public SoundDefinition closeFlybySound;

	public float closeFlybyDistance = 3f;

	[Header("Tumble")]
	public float tumbleSpeed;

	public Vector3 tumbleAxis = Vector3.right;

	[Header("Swim")]
	public Vector3 swimScale;

	public Vector3 swimSpeed;

	[NonSerialized]
	public BasePlayer owner;

	[NonSerialized]
	public AttackEntity sourceWeaponPrefab;

	[NonSerialized]
	public Projectile sourceProjectilePrefab;

	[NonSerialized]
	public ItemModProjectile mod;

	[NonSerialized]
	public int projectileID;

	[NonSerialized]
	public int seed;

	[NonSerialized]
	public bool clientsideEffect;

	[NonSerialized]
	public bool clientsideAttack;

	[NonSerialized]
	public float integrity = 1f;

	[NonSerialized]
	public float maxDistance = float.PositiveInfinity;

	[NonSerialized]
	public Modifier modifier = Modifier.Default;

	[NonSerialized]
	public bool invisible;

	private static uint _fleshMaterialID;

	private static uint _waterMaterialID;

	private static uint cachedWaterString;

	public void CalculateDamage(HitInfo info, Modifier mod, float scale)
	{
		float num = damageMultipliers.Lerp(mod.distanceOffset + mod.distanceScale * damageDistances.x, mod.distanceOffset + mod.distanceScale * damageDistances.y, info.ProjectileDistance);
		float num2 = scale * (mod.damageOffset + mod.damageScale * num);
		foreach (DamageTypeEntry damageType in damageTypes)
		{
			info.damageTypes.Add(damageType.type, damageType.amount * num2);
		}
		if (ConVar.Global.developer > 0)
		{
			Debug.Log(" Projectile damage: " + info.damageTypes.Total() + " (scalar=" + num2 + ")");
		}
	}

	public static uint FleshMaterialID()
	{
		if (_fleshMaterialID == 0)
		{
			_fleshMaterialID = StringPool.Get("flesh");
		}
		return _fleshMaterialID;
	}

	public static uint WaterMaterialID()
	{
		if (_waterMaterialID == 0)
		{
			_waterMaterialID = StringPool.Get("Water");
		}
		return _waterMaterialID;
	}

	public static bool IsWaterMaterial(string hitMaterial)
	{
		if (cachedWaterString == 0)
		{
			cachedWaterString = StringPool.Get("Water");
		}
		if (StringPool.Get(hitMaterial) == cachedWaterString)
		{
			return true;
		}
		return false;
	}

	public static bool ShouldStopProjectile(RaycastHit hit)
	{
		BaseEntity entity = RaycastHitEx.GetEntity(hit);
		if (entity != null && !entity.ShouldBlockProjectiles())
		{
			return false;
		}
		return true;
	}
}
