#define UNITY_ASSERTIONS
using System;
using System.Collections.Generic;
using ConVar;
using Facepunch;
using Network;
using Oxide.Core;
using ProtoBuf;
using Rust;
using Rust.Ai;
using UnityEngine;
using UnityEngine.Assertions;

public class BaseMelee : AttackEntity
{
	[Serializable]
	public class MaterialFX
	{
		public string materialName;

		public GameObjectRef fx;
	}

	[Header("Melee")]
	public DamageProperties damageProperties;

	public List<DamageTypeEntry> damageTypes;

	public float maxDistance = 1.5f;

	public float attackRadius = 0.3f;

	public bool isAutomatic = true;

	public bool blockSprintOnAttack = true;

	public bool canUntieCrates;

	[Header("Effects")]
	public GameObjectRef strikeFX;

	public bool useStandardHitEffects = true;

	[Header("NPCUsage")]
	public float aiStrikeDelay = 0.2f;

	public GameObjectRef swingEffect;

	public List<MaterialFX> materialStrikeFX = new List<MaterialFX>();

	[Range(0f, 1f)]
	[Header("Other")]
	public float heartStress = 0.5f;

	public ResourceDispenser.GatherProperties gathering;

	[Header("Throwing")]
	public bool canThrowAsProjectile;

	public bool canAiHearIt;

	public bool onlyThrowAsProjectile;

	public override bool OnRpcMessage(BasePlayer player, uint rpc, Message msg)
	{
		using (TimeWarning.New("BaseMelee.OnRpcMessage"))
		{
			if (rpc == 3168282921u && player != null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (ConVar.Global.developer > 2)
				{
					Debug.Log("SV_RPCMessage: " + player?.ToString() + " - CLProject ");
				}
				using (TimeWarning.New("CLProject"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.FromOwner.Test(3168282921u, "CLProject", this, player))
						{
							return true;
						}
						if (!RPC_Server.IsActiveItem.Test(3168282921u, "CLProject", this, player))
						{
							return true;
						}
					}
					try
					{
						using (TimeWarning.New("Call"))
						{
							RPCMessage rPCMessage = default(RPCMessage);
							rPCMessage.connection = msg.connection;
							rPCMessage.player = player;
							rPCMessage.read = msg.read;
							RPCMessage msg2 = rPCMessage;
							CLProject(msg2);
						}
					}
					catch (Exception exception)
					{
						Debug.LogException(exception);
						player.Kick("RPC Error in CLProject");
					}
				}
				return true;
			}
			if (rpc == 4088326849u && player != null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (ConVar.Global.developer > 2)
				{
					Debug.Log("SV_RPCMessage: " + player?.ToString() + " - PlayerAttack ");
				}
				using (TimeWarning.New("PlayerAttack"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.IsActiveItem.Test(4088326849u, "PlayerAttack", this, player))
						{
							return true;
						}
					}
					try
					{
						using (TimeWarning.New("Call"))
						{
							RPCMessage rPCMessage = default(RPCMessage);
							rPCMessage.connection = msg.connection;
							rPCMessage.player = player;
							rPCMessage.read = msg.read;
							RPCMessage msg3 = rPCMessage;
							PlayerAttack(msg3);
						}
					}
					catch (Exception exception2)
					{
						Debug.LogException(exception2);
						player.Kick("RPC Error in PlayerAttack");
					}
				}
				return true;
			}
		}
		return base.OnRpcMessage(player, rpc, msg);
	}

	public override void GetAttackStats(HitInfo info)
	{
		info.damageTypes.Add(damageTypes);
		info.CanGather = gathering.Any();
	}

	public virtual void DoAttackShared(HitInfo info)
	{
		if (Interface.CallHook("OnPlayerAttack", GetOwnerPlayer(), info) != null)
		{
			return;
		}
		GetAttackStats(info);
		if (info.HitEntity != null)
		{
			using (TimeWarning.New("OnAttacked", 50))
			{
				info.HitEntity.OnAttacked(info);
			}
		}
		if (info.DoHitEffects)
		{
			if (base.isServer)
			{
				using (TimeWarning.New("ImpactEffect", 20))
				{
					Effect.server.ImpactEffect(info);
				}
			}
			else
			{
				using (TimeWarning.New("ImpactEffect", 20))
				{
					Effect.client.ImpactEffect(info);
				}
			}
		}
		if (base.isServer && !base.IsDestroyed)
		{
			using (TimeWarning.New("UpdateItemCondition", 50))
			{
				UpdateItemCondition(info);
			}
			StartAttackCooldown(repeatDelay);
		}
	}

	public ResourceDispenser.GatherPropertyEntry GetGatherInfoFromIndex(ResourceDispenser.GatherType index)
	{
		return gathering.GetFromIndex(index);
	}

	public virtual bool CanHit(HitTest info)
	{
		return true;
	}

	public float TotalDamage()
	{
		float num = 0f;
		foreach (DamageTypeEntry damageType in damageTypes)
		{
			if (!(damageType.amount <= 0f))
			{
				num += damageType.amount;
			}
		}
		return num;
	}

	public bool IsItemBroken()
	{
		return GetOwnerItem()?.isBroken ?? true;
	}

	public void LoseCondition(float amount)
	{
		Item ownerItem = GetOwnerItem();
		if (ownerItem != null && !base.UsingInfiniteAmmoCheat)
		{
			ownerItem.LoseCondition(amount);
		}
	}

	public virtual float GetConditionLoss()
	{
		return 1f;
	}

	public void UpdateItemCondition(HitInfo info)
	{
		Item ownerItem = GetOwnerItem();
		if (ownerItem == null || !ownerItem.hasCondition || info == null || !info.DidHit || info.DidGather)
		{
			return;
		}
		float conditionLoss = GetConditionLoss();
		float num = 0f;
		foreach (DamageTypeEntry damageType in damageTypes)
		{
			if (!(damageType.amount <= 0f))
			{
				num += Mathf.Clamp(damageType.amount - info.damageTypes.Get(damageType.type), 0f, damageType.amount);
			}
		}
		conditionLoss += num * 0.2f;
		if (!base.UsingInfiniteAmmoCheat)
		{
			ownerItem.LoseCondition(conditionLoss);
		}
	}

	[RPC_Server.IsActiveItem]
	[RPC_Server]
	public void PlayerAttack(RPCMessage msg)
	{
		BasePlayer player = msg.player;
		if (!VerifyClientAttack(player))
		{
			SendNetworkUpdate();
			return;
		}
		using (TimeWarning.New("PlayerAttack", 50))
		{
			using PlayerAttack playerAttack = ProtoBuf.PlayerAttack.Deserialize(msg.read);
			if (playerAttack == null)
			{
				return;
			}
			HitInfo hitInfo = Facepunch.Pool.Get<HitInfo>();
			hitInfo.LoadFromAttack(playerAttack.attack, serverSide: true);
			hitInfo.Initiator = player;
			hitInfo.Weapon = this;
			hitInfo.WeaponPrefab = this;
			hitInfo.Predicted = msg.connection;
			hitInfo.damageProperties = damageProperties;
			if (Interface.CallHook("OnMeleeAttack", player, hitInfo) != null)
			{
				return;
			}
			if (hitInfo.IsNaNOrInfinity())
			{
				string shortPrefabName = base.ShortPrefabName;
				AntiHack.Log(player, AntiHackType.MeleeHack, "Contains NaN (" + shortPrefabName + ")");
				player.stats.combat.LogInvalid(hitInfo, "melee_nan");
				return;
			}
			BaseEntity hitEntity = hitInfo.HitEntity;
			BasePlayer basePlayer = hitInfo.HitEntity as BasePlayer;
			bool flag = basePlayer != null;
			bool flag2 = flag && basePlayer.IsSleeping();
			bool flag3 = flag && basePlayer.IsWounded();
			bool flag4 = flag && basePlayer.isMounted;
			bool flag5 = flag && basePlayer.HasParent();
			bool flag6 = hitEntity != null;
			bool flag7 = flag6 && hitEntity.IsNpc;
			bool flag8;
			int num5;
			Vector3 center;
			Vector3 position;
			Vector3 pointStart;
			Vector3 hitPositionWorld;
			Vector3 vector;
			int num16;
			if (ConVar.AntiHack.melee_protection > 0)
			{
				flag8 = true;
				float num = 1f + ConVar.AntiHack.melee_forgiveness;
				float melee_clientframes = ConVar.AntiHack.melee_clientframes;
				float melee_serverframes = ConVar.AntiHack.melee_serverframes;
				float num2 = melee_clientframes / 60f;
				float num3 = melee_serverframes * Mathx.Max(UnityEngine.Time.deltaTime, UnityEngine.Time.smoothDeltaTime, UnityEngine.Time.fixedDeltaTime);
				float num4 = (player.desyncTimeClamped + num2 + num3) * num;
				num5 = 2162688;
				if (ConVar.AntiHack.melee_terraincheck)
				{
					num5 |= 0x800000;
				}
				if (ConVar.AntiHack.melee_vehiclecheck)
				{
					num5 |= 0x8000000;
				}
				if (flag && hitInfo.boneArea == (HitArea)(-1))
				{
					string shortPrefabName2 = base.ShortPrefabName;
					string shortPrefabName3 = basePlayer.ShortPrefabName;
					AntiHack.Log(player, AntiHackType.MeleeHack, "Bone is invalid  (" + shortPrefabName2 + " on " + shortPrefabName3 + " bone " + hitInfo.HitBone + ")");
					player.stats.combat.LogInvalid(hitInfo, "melee_bone");
					flag8 = false;
				}
				if (ConVar.AntiHack.melee_protection >= 2)
				{
					if (flag6)
					{
						float num6 = hitEntity.MaxVelocity() + hitEntity.GetParentVelocity().magnitude;
						float num7 = hitEntity.BoundsPadding() + num4 * num6;
						float num8 = hitEntity.Distance(hitInfo.HitPositionWorld);
						if (num8 > num7)
						{
							string shortPrefabName4 = base.ShortPrefabName;
							string shortPrefabName5 = hitEntity.ShortPrefabName;
							AntiHack.Log(player, AntiHackType.MeleeHack, "Entity too far away (" + shortPrefabName4 + " on " + shortPrefabName5 + " with " + num8 + "m > " + num7 + "m in " + num4 + "s)");
							player.stats.combat.LogInvalid(hitInfo, "melee_target");
							flag8 = false;
						}
					}
					if (ConVar.AntiHack.melee_protection >= 4 && flag8 && flag && !flag7 && !flag2 && !flag3 && !flag4 && !flag5)
					{
						float magnitude = basePlayer.GetParentVelocity().magnitude;
						float num9 = basePlayer.BoundsPadding() + num4 * magnitude + ConVar.AntiHack.tickhistoryforgiveness;
						float num10 = basePlayer.tickHistory.Distance(basePlayer, hitInfo.HitPositionWorld);
						if (num10 > num9)
						{
							string shortPrefabName6 = base.ShortPrefabName;
							string shortPrefabName7 = basePlayer.ShortPrefabName;
							AntiHack.Log(player, AntiHackType.ProjectileHack, "Player too far away (" + shortPrefabName6 + " on " + shortPrefabName7 + " with " + num10 + "m > " + num9 + "m in " + num4 + "s)");
							player.stats.combat.LogInvalid(hitInfo, "player_distance");
							flag8 = false;
						}
					}
				}
				if (ConVar.AntiHack.melee_protection >= 1)
				{
					if (ConVar.AntiHack.melee_protection >= 4)
					{
						float magnitude2 = player.GetParentVelocity().magnitude;
						float num11 = player.BoundsPadding() + num4 * magnitude2 + num * maxDistance;
						float num12 = player.tickHistory.Distance(player, hitInfo.HitPositionWorld);
						if (num12 > num11)
						{
							string shortPrefabName8 = base.ShortPrefabName;
							string text = (flag6 ? hitEntity.ShortPrefabName : "world");
							AntiHack.Log(player, AntiHackType.MeleeHack, "Initiator too far away (" + shortPrefabName8 + " on " + text + " with " + num12 + "m > " + num11 + "m in " + num4 + "s)");
							player.stats.combat.LogInvalid(hitInfo, "melee_initiator");
							flag8 = false;
						}
					}
					else
					{
						float num13 = player.MaxVelocity() + player.GetParentVelocity().magnitude;
						float num14 = player.BoundsPadding() + num4 * num13 + num * maxDistance;
						float num15 = player.Distance(hitInfo.HitPositionWorld);
						if (num15 > num14)
						{
							string shortPrefabName9 = base.ShortPrefabName;
							string text2 = (flag6 ? hitEntity.ShortPrefabName : "world");
							AntiHack.Log(player, AntiHackType.MeleeHack, "Initiator too far away (" + shortPrefabName9 + " on " + text2 + " with " + num15 + "m > " + num14 + "m in " + num4 + "s)");
							player.stats.combat.LogInvalid(hitInfo, "melee_initiator");
							flag8 = false;
						}
					}
				}
				if (ConVar.AntiHack.melee_protection >= 3)
				{
					if (flag6)
					{
						center = player.eyes.center;
						position = player.eyes.position;
						pointStart = hitInfo.PointStart;
						hitPositionWorld = hitInfo.HitPositionWorld;
						hitPositionWorld -= (hitPositionWorld - pointStart).normalized * 0.001f;
						vector = hitInfo.PositionOnRay(hitPositionWorld);
						Vector3 vector2 = Vector3.zero;
						Vector3 vector3 = Vector3.zero;
						Vector3 vector4 = Vector3.zero;
						if (ConVar.AntiHack.melee_backtracking > 0f)
						{
							vector2 = (position - center).normalized * ConVar.AntiHack.melee_backtracking;
							vector3 = (pointStart - position).normalized * ConVar.AntiHack.melee_backtracking;
							vector4 = (vector - pointStart).normalized * ConVar.AntiHack.melee_backtracking;
						}
						if (GamePhysics.LineOfSight(center - vector2, position + vector2, num5) && GamePhysics.LineOfSight(position - vector3, pointStart + vector3, num5) && GamePhysics.LineOfSight(pointStart - vector4, vector, num5))
						{
							num16 = (GamePhysics.LineOfSight(vector, hitPositionWorld, num5) ? 1 : 0);
							if (num16 != 0)
							{
								player.stats.Add("hit_" + hitEntity.Categorize() + "_direct_los", 1, Stats.Server);
								goto IL_07c1;
							}
						}
						else
						{
							num16 = 0;
						}
						player.stats.Add("hit_" + hitEntity.Categorize() + "_indirect_los", 1, Stats.Server);
						goto IL_07c1;
					}
					goto IL_08ae;
				}
				goto IL_0a14;
			}
			goto IL_0a26;
			IL_07c1:
			if (num16 == 0)
			{
				string shortPrefabName10 = base.ShortPrefabName;
				string shortPrefabName11 = hitEntity.ShortPrefabName;
				string[] obj = new string[14]
				{
					"Line of sight (", shortPrefabName10, " on ", shortPrefabName11, ") ", null, null, null, null, null,
					null, null, null, null
				};
				Vector3 vector5 = center;
				obj[5] = vector5.ToString();
				obj[6] = " ";
				vector5 = position;
				obj[7] = vector5.ToString();
				obj[8] = " ";
				vector5 = pointStart;
				obj[9] = vector5.ToString();
				obj[10] = " ";
				vector5 = vector;
				obj[11] = vector5.ToString();
				obj[12] = " ";
				vector5 = hitPositionWorld;
				obj[13] = vector5.ToString();
				AntiHack.Log(player, AntiHackType.MeleeHack, string.Concat(obj));
				player.stats.combat.LogInvalid(hitInfo, "melee_los");
				flag8 = false;
			}
			goto IL_08ae;
			IL_0a26:
			player.metabolism.UseHeart(heartStress * 0.2f);
			using (TimeWarning.New("DoAttackShared", 50))
			{
				DoAttackShared(hitInfo);
				return;
			}
			IL_0a14:
			if (!flag8)
			{
				AntiHack.AddViolation(player, AntiHackType.MeleeHack, ConVar.AntiHack.melee_penalty);
				return;
			}
			goto IL_0a26;
			IL_08ae:
			if (flag8 && flag && !flag7)
			{
				Vector3 hitPositionWorld2 = hitInfo.HitPositionWorld;
				Vector3 position2 = basePlayer.eyes.position;
				Vector3 vector6 = basePlayer.CenterPoint();
				float melee_losforgiveness = ConVar.AntiHack.melee_losforgiveness;
				bool flag9 = GamePhysics.LineOfSight(hitPositionWorld2, position2, num5, 0f, melee_losforgiveness) && GamePhysics.LineOfSight(position2, hitPositionWorld2, num5, melee_losforgiveness, 0f);
				if (!flag9)
				{
					flag9 = GamePhysics.LineOfSight(hitPositionWorld2, vector6, num5, 0f, melee_losforgiveness) && GamePhysics.LineOfSight(vector6, hitPositionWorld2, num5, melee_losforgiveness, 0f);
				}
				if (!flag9)
				{
					string shortPrefabName12 = base.ShortPrefabName;
					string shortPrefabName13 = basePlayer.ShortPrefabName;
					string[] obj2 = new string[12]
					{
						"Line of sight (", shortPrefabName12, " on ", shortPrefabName13, ") ", null, null, null, null, null,
						null, null
					};
					Vector3 vector5 = hitPositionWorld2;
					obj2[5] = vector5.ToString();
					obj2[6] = " ";
					vector5 = position2;
					obj2[7] = vector5.ToString();
					obj2[8] = " or ";
					vector5 = hitPositionWorld2;
					obj2[9] = vector5.ToString();
					obj2[10] = " ";
					vector5 = vector6;
					obj2[11] = vector5.ToString();
					AntiHack.Log(player, AntiHackType.MeleeHack, string.Concat(obj2));
					player.stats.combat.LogInvalid(hitInfo, "melee_los");
					flag8 = false;
				}
			}
			goto IL_0a14;
		}
	}

	public override bool CanBeUsedInWater()
	{
		return true;
	}

	public virtual string GetStrikeEffectPath(string materialName)
	{
		for (int i = 0; i < materialStrikeFX.Count; i++)
		{
			if (materialStrikeFX[i].materialName == materialName && materialStrikeFX[i].fx.isValid)
			{
				return materialStrikeFX[i].fx.resourcePath;
			}
		}
		return strikeFX.resourcePath;
	}

	public override void ServerUse()
	{
		if (base.isClient || HasAttackCooldown())
		{
			return;
		}
		BasePlayer ownerPlayer = GetOwnerPlayer();
		if (!(ownerPlayer == null))
		{
			StartAttackCooldown(repeatDelay * 2f);
			ownerPlayer.SignalBroadcast(Signal.Attack, string.Empty);
			if (swingEffect.isValid)
			{
				Effect.server.Run(swingEffect.resourcePath, base.transform.position, Vector3.forward, ownerPlayer.net.connection);
			}
			if (IsInvoking(ServerUse_Strike))
			{
				CancelInvoke(ServerUse_Strike);
			}
			Invoke(ServerUse_Strike, aiStrikeDelay);
		}
	}

	public virtual void ServerUse_OnHit(HitInfo info)
	{
	}

	public void ServerUse_Strike()
	{
		BasePlayer ownerPlayer = GetOwnerPlayer();
		if (ownerPlayer == null)
		{
			return;
		}
		Vector3 position = ownerPlayer.eyes.position;
		Vector3 vector = ownerPlayer.eyes.BodyForward();
		for (int i = 0; i < 2; i++)
		{
			List<RaycastHit> obj = Facepunch.Pool.GetList<RaycastHit>();
			GamePhysics.TraceAll(new Ray(position - vector * ((i == 0) ? 0f : 0.2f), vector), (i == 0) ? 0f : attackRadius, obj, effectiveRange + 0.2f, 1220225809);
			bool flag = false;
			for (int j = 0; j < obj.Count; j++)
			{
				RaycastHit hit = obj[j];
				BaseEntity entity = RaycastHitEx.GetEntity(hit);
				if (entity == null || (entity != null && (entity == ownerPlayer || entity.EqualNetID(ownerPlayer))) || (entity != null && entity.isClient) || entity.Categorize() == ownerPlayer.Categorize())
				{
					continue;
				}
				float num = 0f;
				foreach (DamageTypeEntry damageType in damageTypes)
				{
					num += damageType.amount;
				}
				entity.OnAttacked(new HitInfo(ownerPlayer, entity, DamageType.Slash, num * npcDamageScale));
				HitInfo obj2 = Facepunch.Pool.Get<HitInfo>();
				obj2.HitEntity = entity;
				obj2.HitPositionWorld = hit.point;
				obj2.HitNormalWorld = -vector;
				if (entity is BaseNpc || entity is BasePlayer)
				{
					obj2.HitMaterial = StringPool.Get("Flesh");
				}
				else
				{
					obj2.HitMaterial = StringPool.Get((RaycastHitEx.GetCollider(hit).sharedMaterial != null) ? AssetNameCache.GetName(RaycastHitEx.GetCollider(hit).sharedMaterial) : "generic");
				}
				ServerUse_OnHit(obj2);
				Effect.server.ImpactEffect(obj2);
				Facepunch.Pool.Free(ref obj2);
				flag = true;
				if (!(entity != null) || entity.ShouldBlockProjectiles())
				{
					break;
				}
			}
			Facepunch.Pool.FreeList(ref obj);
			if (flag)
			{
				break;
			}
		}
	}

	public override Vector3 GetInheritedVelocity(BasePlayer player, Vector3 direction)
	{
		return player.GetInheritedThrowVelocity(direction);
	}

	[RPC_Server.FromOwner]
	[RPC_Server.IsActiveItem]
	[RPC_Server]
	private void CLProject(RPCMessage msg)
	{
		BasePlayer player = msg.player;
		if (!VerifyClientAttack(player))
		{
			SendNetworkUpdate();
		}
		else
		{
			if (player == null || player.IsHeadUnderwater())
			{
				return;
			}
			if (!canThrowAsProjectile)
			{
				AntiHack.Log(player, AntiHackType.ProjectileHack, "Not throwable (" + base.ShortPrefabName + ")");
				player.stats.combat.LogInvalid(player, this, "not_throwable");
				return;
			}
			Item item = GetItem();
			if (item == null)
			{
				AntiHack.Log(player, AntiHackType.ProjectileHack, "Item not found (" + base.ShortPrefabName + ")");
				player.stats.combat.LogInvalid(player, this, "item_missing");
				return;
			}
			ItemModProjectile component = item.info.GetComponent<ItemModProjectile>();
			if (component == null)
			{
				AntiHack.Log(player, AntiHackType.ProjectileHack, "Item mod not found (" + base.ShortPrefabName + ")");
				player.stats.combat.LogInvalid(player, this, "mod_missing");
				return;
			}
			ProjectileShoot projectileShoot = ProjectileShoot.Deserialize(msg.read);
			if (projectileShoot.projectiles.Count != 1)
			{
				AntiHack.Log(player, AntiHackType.ProjectileHack, "Projectile count mismatch (" + base.ShortPrefabName + ")");
				player.stats.combat.LogInvalid(player, this, "count_mismatch");
				return;
			}
			player.CleanupExpiredProjectiles();
			Guid projectileGroupId = Guid.NewGuid();
			foreach (ProjectileShoot.Projectile projectile in projectileShoot.projectiles)
			{
				if (player.HasFiredProjectile(projectile.projectileID))
				{
					AntiHack.Log(player, AntiHackType.ProjectileHack, "Duplicate ID (" + projectile.projectileID + ")");
					player.stats.combat.LogInvalid(player, this, "duplicate_id");
					continue;
				}
				Vector3 positionOffset = Vector3.zero;
				if (ConVar.AntiHack.projectile_positionoffset && (player.isMounted || player.HasParent()))
				{
					if (!ValidateEyePos(player, projectile.startPos, checkLineOfSight: false))
					{
						continue;
					}
					Vector3 position = player.eyes.position;
					positionOffset = position - projectile.startPos;
					projectile.startPos = position;
				}
				else if (!ValidateEyePos(player, projectile.startPos))
				{
					continue;
				}
				player.NoteFiredProjectile(projectile.projectileID, projectile.startPos, projectile.startVel, this, item.info, projectileGroupId, positionOffset, item);
				Effect effect = new Effect();
				effect.Init(Effect.Type.Projectile, projectile.startPos, projectile.startVel, msg.connection);
				effect.scale = 1f;
				effect.pooledString = component.projectileObject.resourcePath;
				effect.number = projectile.seed;
				EffectNetwork.Send(effect);
			}
			projectileShoot?.Dispose();
			item.SetParent(null);
			Interface.CallHook("OnMeleeThrown", player, item);
			if (!canAiHearIt)
			{
				return;
			}
			float num = 0f;
			if (component.projectileObject != null)
			{
				GameObject gameObject = component.projectileObject.Get();
				if (gameObject != null)
				{
					Projectile component2 = gameObject.GetComponent<Projectile>();
					if (component2 != null)
					{
						foreach (DamageTypeEntry damageType in component2.damageTypes)
						{
							num += damageType.amount;
						}
					}
				}
			}
			if (player != null)
			{
				Sensation sensation = default(Sensation);
				sensation.Type = SensationType.ThrownWeapon;
				sensation.Position = player.transform.position;
				sensation.Radius = 50f;
				sensation.DamagePotential = num;
				sensation.InitiatorPlayer = player;
				sensation.Initiator = player;
				Sense.Stimulate(sensation);
			}
		}
	}
}
