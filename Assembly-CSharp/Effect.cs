using Network;
using Oxide.Core;
using Rust;
using UnityEngine;

public class Effect : EffectData
{
	public enum Type : uint
	{
		Generic = 0u,
		Projectile = 1u,
		GenericGlobal = 2u
	}

	public static class client
	{
		public static void Run(Type fxtype, BaseEntity ent, uint boneID, Vector3 posLocal, Vector3 normLocal)
		{
		}

		public static void Run(string strName, BaseEntity ent, uint boneID, Vector3 posLocal, Vector3 normLocal)
		{
			string.IsNullOrEmpty(strName);
		}

		public static void Run(Type fxtype, Vector3 posWorld, Vector3 normWorld, Vector3 up = default(Vector3))
		{
		}

		public static void Run(string strName, Vector3 posWorld = default(Vector3), Vector3 normWorld = default(Vector3), Vector3 up = default(Vector3), Type overrideType = Type.Generic)
		{
			string.IsNullOrEmpty(strName);
		}

		public static void Run(string strName, GameObject obj)
		{
			string.IsNullOrEmpty(strName);
		}

		public static void DoAdditiveImpactEffect(HitInfo info, string effectName)
		{
			if (BaseNetworkableEx.IsValid(info.HitEntity))
			{
				Run(effectName, info.HitEntity, info.HitBone, info.HitPositionLocal + info.HitNormalLocal * 0.1f, info.HitNormalLocal);
			}
			else
			{
				Run(effectName, info.HitPositionWorld + info.HitNormalWorld * 0.1f, info.HitNormalWorld);
			}
		}

		public static void ImpactEffect(HitInfo info)
		{
			if (!info.DoHitEffects)
			{
				return;
			}
			string materialName = StringPool.Get(info.HitMaterial);
			string strName = EffectDictionary.GetParticle(info.damageTypes.GetMajorityDamageType(), materialName);
			string decal = EffectDictionary.GetDecal(info.damageTypes.GetMajorityDamageType(), materialName);
			if (TerrainMeta.WaterMap != null && info.HitMaterial != Projectile.WaterMaterialID() && info.HitMaterial != Projectile.FleshMaterialID() && info.HitPositionWorld.y < TerrainMeta.WaterMap.GetHeight(info.HitPositionWorld) && WaterLevel.Test(info.HitPositionWorld, waves: false))
			{
				return;
			}
			if (BaseNetworkableEx.IsValid(info.HitEntity))
			{
				GameObjectRef impactEffect = info.HitEntity.GetImpactEffect(info);
				if (impactEffect.isValid)
				{
					strName = impactEffect.resourcePath;
				}
				Run(strName, info.HitEntity, info.HitBone, info.HitPositionLocal, info.HitNormalLocal);
				if (info.DoDecals)
				{
					Run(decal, info.HitEntity, info.HitBone, info.HitPositionLocal, info.HitNormalLocal);
				}
			}
			else
			{
				Type overrideType = Type.Generic;
				Run(strName, info.HitPositionWorld, info.HitNormalWorld, default(Vector3), overrideType);
				Run(decal, info.HitPositionWorld, info.HitNormalWorld, default(Vector3), overrideType);
			}
			if ((bool)info.WeaponPrefab)
			{
				BaseMelee baseMelee = info.WeaponPrefab as BaseMelee;
				if (baseMelee != null)
				{
					string strikeEffectPath = baseMelee.GetStrikeEffectPath(materialName);
					if (BaseNetworkableEx.IsValid(info.HitEntity))
					{
						Run(strikeEffectPath, info.HitEntity, info.HitBone, info.HitPositionLocal, info.HitNormalLocal);
					}
					else
					{
						Run(strikeEffectPath, info.HitPositionWorld, info.HitNormalWorld);
					}
				}
			}
			if (info.damageTypes.Has(DamageType.Explosion))
			{
				DoAdditiveImpactEffect(info, "assets/bundled/prefabs/fx/impacts/additive/explosion.prefab");
			}
			if (info.damageTypes.Has(DamageType.Heat))
			{
				DoAdditiveImpactEffect(info, "assets/bundled/prefabs/fx/impacts/additive/fire.prefab");
			}
		}
	}

	public static class server
	{
		public static void Run(Type fxtype, BaseEntity ent, uint boneID, Vector3 posLocal, Vector3 normLocal, Connection sourceConnection = null, bool broadcast = false)
		{
			reusableInstace.Init(fxtype, ent, boneID, posLocal, normLocal, sourceConnection);
			reusableInstace.broadcast = broadcast;
			EffectNetwork.Send(reusableInstace);
		}

		public static void Run(string strName, BaseEntity ent, uint boneID, Vector3 posLocal, Vector3 normLocal, Connection sourceConnection = null, bool broadcast = false)
		{
			if (!string.IsNullOrEmpty(strName))
			{
				reusableInstace.Init(Type.Generic, ent, boneID, posLocal, normLocal, sourceConnection);
				reusableInstace.pooledString = strName;
				reusableInstace.broadcast = broadcast;
				EffectNetwork.Send(reusableInstace);
			}
		}

		public static void Run(Type fxtype, Vector3 posWorld, Vector3 normWorld, Connection sourceConnection = null, bool broadcast = false)
		{
			reusableInstace.Init(fxtype, posWorld, normWorld, sourceConnection);
			reusableInstace.broadcast = broadcast;
			EffectNetwork.Send(reusableInstace);
		}

		public static void Run(string strName, Vector3 posWorld = default(Vector3), Vector3 normWorld = default(Vector3), Connection sourceConnection = null, bool broadcast = false)
		{
			if (!string.IsNullOrEmpty(strName))
			{
				reusableInstace.Init(Type.Generic, posWorld, normWorld, sourceConnection);
				reusableInstace.pooledString = strName;
				reusableInstace.broadcast = broadcast;
				EffectNetwork.Send(reusableInstace);
			}
		}

		public static void DoAdditiveImpactEffect(HitInfo info, string effectName)
		{
			if (BaseNetworkableEx.IsValid(info.HitEntity))
			{
				Run(effectName, info.HitEntity, info.HitBone, info.HitPositionLocal, info.HitNormalLocal, info.Predicted);
			}
			else
			{
				Run(effectName, info.HitPositionWorld, info.HitNormalWorld, info.Predicted);
			}
		}

		public static void ImpactEffect(HitInfo info)
		{
			if (Interface.CallHook("OnImpactEffectCreate", info) != null || ((bool)info.InitiatorPlayer && info.InitiatorPlayer.limitNetworking) || !info.DoHitEffects)
			{
				return;
			}
			string materialName = StringPool.Get(info.HitMaterial);
			if (TerrainMeta.WaterMap != null && info.HitMaterial != Projectile.WaterMaterialID() && info.HitMaterial != Projectile.FleshMaterialID() && info.HitPositionWorld.y < TerrainMeta.WaterMap.GetHeight(info.HitPositionWorld) && WaterLevel.Test(info.HitPositionWorld, waves: false))
			{
				return;
			}
			string strName = EffectDictionary.GetParticle(info.damageTypes.GetMajorityDamageType(), materialName);
			string decal = EffectDictionary.GetDecal(info.damageTypes.GetMajorityDamageType(), materialName);
			if (BaseNetworkableEx.IsValid(info.HitEntity))
			{
				GameObjectRef impactEffect = info.HitEntity.GetImpactEffect(info);
				if (impactEffect.isValid)
				{
					strName = impactEffect.resourcePath;
				}
				Bounds bounds = info.HitEntity.bounds;
				float num = info.HitEntity.BoundsPadding();
				bounds.extents += new Vector3(num, num, num);
				if (!bounds.Contains(info.HitPositionLocal))
				{
					BasePlayer initiatorPlayer = info.InitiatorPlayer;
					if (initiatorPlayer != null && initiatorPlayer.GetType() == typeof(BasePlayer))
					{
						float num2 = Mathf.Sqrt(bounds.SqrDistance(info.HitPositionLocal));
						AntiHack.Log(initiatorPlayer, AntiHackType.EffectHack, $"Tried to run an impact effect outside of entity '{info.HitEntity.ShortPrefabName}' bounds by {num2}m");
					}
					return;
				}
				Run(strName, info.HitEntity, info.HitBone, info.HitPositionLocal, info.HitNormalLocal, info.Predicted);
				Run(decal, info.HitEntity, info.HitBone, info.HitPositionLocal, info.HitNormalLocal, info.Predicted);
			}
			else
			{
				Run(strName, info.HitPositionWorld, info.HitNormalWorld, info.Predicted);
				Run(decal, info.HitPositionWorld, info.HitNormalWorld, info.Predicted);
			}
			if ((bool)info.WeaponPrefab)
			{
				BaseMelee baseMelee = info.WeaponPrefab as BaseMelee;
				if (baseMelee != null)
				{
					string strikeEffectPath = baseMelee.GetStrikeEffectPath(materialName);
					if (BaseNetworkableEx.IsValid(info.HitEntity))
					{
						Run(strikeEffectPath, info.HitEntity, info.HitBone, info.HitPositionLocal, info.HitNormalLocal, info.Predicted);
					}
					else
					{
						Run(strikeEffectPath, info.HitPositionWorld, info.HitNormalWorld, info.Predicted);
					}
				}
			}
			if (info.damageTypes.Has(DamageType.Explosion))
			{
				DoAdditiveImpactEffect(info, "assets/bundled/prefabs/fx/impacts/additive/explosion.prefab");
			}
			if (info.damageTypes.Has(DamageType.Heat))
			{
				DoAdditiveImpactEffect(info, "assets/bundled/prefabs/fx/impacts/additive/fire.prefab");
			}
		}
	}

	public Vector3 Up;

	public Vector3 worldPos;

	public Vector3 worldNrm;

	public bool attached;

	public Transform transform;

	public GameObject gameObject;

	public string pooledString;

	public bool broadcast;

	private static Effect reusableInstace = new Effect();

	public Effect()
	{
	}

	public Effect(string effectName, Vector3 posWorld, Vector3 normWorld, Connection sourceConnection = null)
	{
		Init(Type.Generic, posWorld, normWorld, sourceConnection);
		pooledString = effectName;
	}

	public Effect(string effectName, BaseEntity ent, uint boneID, Vector3 posLocal, Vector3 normLocal, Connection sourceConnection = null)
	{
		Init(Type.Generic, ent, boneID, posLocal, normLocal, sourceConnection);
		pooledString = effectName;
	}

	public void Init(Type fxtype, BaseEntity ent, uint boneID, Vector3 posLocal, Vector3 normLocal, Connection sourceConnection = null)
	{
		Clear();
		type = (uint)fxtype;
		attached = true;
		origin = posLocal;
		normal = normLocal;
		gameObject = null;
		Up = Vector3.zero;
		if (ent != null && !BaseNetworkableEx.IsValid(ent))
		{
			Debug.LogWarning("Effect.Init - invalid entity");
		}
		entity = (BaseNetworkableEx.IsValid(ent) ? ent.net.ID : default(NetworkableId));
		source = sourceConnection?.userid ?? 0;
		bone = boneID;
	}

	public void Init(Type fxtype, Vector3 posWorld, Vector3 normWorld, Connection sourceConnection = null)
	{
		Clear();
		type = (uint)fxtype;
		attached = false;
		worldPos = posWorld;
		worldNrm = normWorld;
		gameObject = null;
		Up = Vector3.zero;
		entity = default(NetworkableId);
		origin = worldPos;
		normal = worldNrm;
		bone = 0u;
		source = sourceConnection?.userid ?? 0;
	}

	public void Clear()
	{
		worldPos = Vector3.zero;
		worldNrm = Vector3.zero;
		attached = false;
		transform = null;
		gameObject = null;
		pooledString = null;
		broadcast = false;
	}
}
