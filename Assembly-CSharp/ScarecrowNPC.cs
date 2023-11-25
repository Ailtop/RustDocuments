using ConVar;
using Oxide.Core;
using ProtoBuf;
using Rust;
using UnityEngine;

public class ScarecrowNPC : NPCPlayer, IAISenses, IAIAttack, IThinker
{
	public float BaseAttackRate = 2f;

	[Header("Loot")]
	public LootContainer.LootSpawnSlot[] LootSpawnSlots;

	public LootContainer.LootSpawnSlot[] bonusLootSlots;

	public static float NextBeanCanAllowedTime;

	public bool BlockClothingOnCorpse;

	public bool RoamAroundHomePoint;

	public GameObjectRef soulReleaseEffect;

	public bool wasSoulReleased;

	public ScarecrowBrain Brain { get; set; }

	public override BaseNpc.AiStatistics.FamilyEnum Family => BaseNpc.AiStatistics.FamilyEnum.Murderer;

	public override float StartHealth()
	{
		return startHealth;
	}

	public override float StartMaxHealth()
	{
		return startHealth;
	}

	public override float MaxHealth()
	{
		return startHealth;
	}

	public override void ServerInit()
	{
		base.ServerInit();
		Brain = GetComponent<ScarecrowBrain>();
		if (!base.isClient)
		{
			AIThinkManager.Add(this);
			wasSoulReleased = false;
		}
	}

	internal override void DoServerDestroy()
	{
		AIThinkManager.Remove(this);
		base.DoServerDestroy();
	}

	public virtual void TryThink()
	{
		ServerThink_Internal();
	}

	public override void ServerThink(float delta)
	{
		base.ServerThink(delta);
		if (Brain.ShouldServerThink())
		{
			Brain.DoThink();
		}
	}

	public override string Categorize()
	{
		return "Scarecrow";
	}

	public override void EquipWeapon(bool skipDeployDelay = false)
	{
		base.EquipWeapon(skipDeployDelay);
		HeldEntity heldEntity = GetHeldEntity();
		if (heldEntity != null && heldEntity is Chainsaw chainsaw)
		{
			chainsaw.ServerNPCStart();
		}
	}

	public float EngagementRange()
	{
		AttackEntity attackEntity = GetAttackEntity();
		if ((bool)attackEntity)
		{
			return attackEntity.effectiveRange * (attackEntity.aiOnlyInRange ? 1f : 2f) * Brain.AttackRangeMultiplier;
		}
		return Brain.SenseRange;
	}

	public bool IsThreat(BaseEntity entity)
	{
		return IsTarget(entity);
	}

	public bool IsTarget(BaseEntity entity)
	{
		if (entity is BasePlayer)
		{
			return !entity.IsNpc;
		}
		return false;
	}

	public bool IsFriendly(BaseEntity entity)
	{
		return false;
	}

	public bool CanAttack(BaseEntity entity)
	{
		if (entity == null)
		{
			return false;
		}
		if (NeedsToReload())
		{
			return false;
		}
		if (IsOnCooldown())
		{
			return false;
		}
		if (!IsTargetInRange(entity, out var _))
		{
			return false;
		}
		if (InSafeZone() || (entity is BasePlayer basePlayer && basePlayer.InSafeZone()))
		{
			return false;
		}
		if (!CanSeeTarget(entity))
		{
			return false;
		}
		return true;
	}

	public bool IsTargetInRange(BaseEntity entity, out float dist)
	{
		dist = Vector3.Distance(entity.transform.position, base.transform.position);
		return dist <= EngagementRange();
	}

	public bool CanSeeTarget(BaseEntity entity)
	{
		if (entity == null)
		{
			return false;
		}
		return entity.IsVisible(GetEntity().CenterPoint(), entity.CenterPoint());
	}

	public bool NeedsToReload()
	{
		return false;
	}

	public bool Reload()
	{
		return true;
	}

	public float CooldownDuration()
	{
		return BaseAttackRate;
	}

	public bool IsOnCooldown()
	{
		AttackEntity attackEntity = GetAttackEntity();
		if ((bool)attackEntity)
		{
			return attackEntity.HasAttackCooldown();
		}
		return true;
	}

	public bool StartAttacking(BaseEntity target)
	{
		BaseCombatEntity baseCombatEntity = target as BaseCombatEntity;
		if (baseCombatEntity == null)
		{
			return false;
		}
		Attack(baseCombatEntity);
		return true;
	}

	private void Attack(BaseCombatEntity target)
	{
		if (!(target == null))
		{
			Vector3 vector = target.ServerPosition - ServerPosition;
			if (vector.magnitude > 0.001f)
			{
				ServerRotation = Quaternion.LookRotation(vector.normalized);
			}
			AttackEntity attackEntity = GetAttackEntity();
			if ((bool)attackEntity)
			{
				attackEntity.ServerUse();
			}
		}
	}

	public void StopAttacking()
	{
	}

	public float GetAmmoFraction()
	{
		return AmmoFractionRemaining();
	}

	public BaseEntity GetBestTarget()
	{
		return null;
	}

	public void AttackTick(float delta, BaseEntity target, bool targetIsLOS)
	{
	}

	public override bool ShouldDropActiveItem()
	{
		return false;
	}

	public override BaseCorpse CreateCorpse()
	{
		using (TimeWarning.New("Create corpse"))
		{
			string strCorpsePrefab = "assets/prefabs/npc/murderer/murderer_corpse.prefab";
			NPCPlayerCorpse nPCPlayerCorpse = DropCorpse(strCorpsePrefab) as NPCPlayerCorpse;
			if ((bool)nPCPlayerCorpse)
			{
				nPCPlayerCorpse.transform.position = nPCPlayerCorpse.transform.position + Vector3.down * NavAgent.baseOffset;
				nPCPlayerCorpse.SetLootableIn(2f);
				nPCPlayerCorpse.SetFlag(Flags.Reserved5, HasPlayerFlag(PlayerFlags.DisplaySash));
				nPCPlayerCorpse.SetFlag(Flags.Reserved2, b: true);
				nPCPlayerCorpse.TakeFrom(this, inventory.containerMain, inventory.containerWear, inventory.containerBelt);
				nPCPlayerCorpse.playerName = "Scarecrow";
				nPCPlayerCorpse.playerSteamID = userID;
				nPCPlayerCorpse.Spawn();
				ItemContainer[] containers = nPCPlayerCorpse.containers;
				for (int i = 0; i < containers.Length; i++)
				{
					containers[i].Clear();
				}
				if (LootSpawnSlots.Length != 0)
				{
					object obj = Interface.CallHook("OnCorpsePopulate", this, nPCPlayerCorpse);
					if (obj is BaseCorpse)
					{
						return (BaseCorpse)obj;
					}
					LootContainer.LootSpawnSlot[] lootSpawnSlots = LootSpawnSlots;
					for (int i = 0; i < lootSpawnSlots.Length; i++)
					{
						LootContainer.LootSpawnSlot lootSpawnSlot = lootSpawnSlots[i];
						for (int j = 0; j < lootSpawnSlot.numberToSpawn; j++)
						{
							if (UnityEngine.Random.Range(0f, 1f) <= lootSpawnSlot.probability)
							{
								lootSpawnSlot.definition.SpawnIntoContainer(nPCPlayerCorpse.containers[0]);
							}
						}
					}
				}
				if (wasSoulReleased)
				{
					LootContainer.LootSpawnSlot[] lootSpawnSlots = bonusLootSlots;
					for (int i = 0; i < lootSpawnSlots.Length; i++)
					{
						LootContainer.LootSpawnSlot lootSpawnSlot2 = lootSpawnSlots[i];
						for (int k = 0; k < lootSpawnSlot2.numberToSpawn; k++)
						{
							if (UnityEngine.Random.Range(0f, 1f) <= lootSpawnSlot2.probability)
							{
								lootSpawnSlot2.definition.SpawnIntoContainer(nPCPlayerCorpse.containers[0]);
							}
						}
					}
				}
			}
			return nPCPlayerCorpse;
		}
	}

	public override void Hurt(HitInfo info)
	{
		bool flag = info.damageTypes.Has(DamageType.Slash) && info.damageTypes.Has(DamageType.Stab) && info.damageTypes.Has(DamageType.Generic) && info.damageTypes.Get(DamageType.Generic) <= 0.1f;
		if (flag)
		{
			if (info.ProjectilePrefab != null && !info.ProjectilePrefab.name.Contains("vamp"))
			{
				flag = false;
			}
			if (info.WeaponPrefab != null && !info.WeaponPrefab.name.Contains("vamp"))
			{
				flag = false;
			}
		}
		if (flag)
		{
			wasSoulReleased = true;
			info.damageTypes.ScaleAll(1000f);
			Effect.server.Run(soulReleaseEffect.resourcePath, this, StringPool.Get("spine3"), Vector3.zero, Vector3.forward);
		}
		else if (!info.isHeadshot)
		{
			if ((info.InitiatorPlayer != null && !info.InitiatorPlayer.IsNpc) || (info.InitiatorPlayer == null && info.Initiator != null && info.Initiator.IsNpc))
			{
				info.damageTypes.ScaleAll(Halloween.scarecrow_body_dmg_modifier);
			}
			else
			{
				info.damageTypes.ScaleAll(2f);
			}
		}
		base.Hurt(info);
	}

	public override void AttackerInfo(PlayerLifeStory.DeathInfo info)
	{
		base.AttackerInfo(info);
		info.inflictorName = inventory.containerBelt.GetSlot(0).info.shortname;
		info.attackerName = base.ShortPrefabName;
	}
}
