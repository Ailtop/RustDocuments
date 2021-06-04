using System;
using ProtoBuf;
using UnityEngine;

public class HumanNPCNew : NPCPlayer, IAISenses, IAIAttack, IThinker
{
	[Header("Loot")]
	public LootContainer.LootSpawnSlot[] LootSpawnSlots;

	[Header("Damage")]
	public float aimConeScale = 2f;

	public float lastDismountTime;

	protected BaseAIBrain<HumanNPCNew> brain;

	[NonSerialized]
	protected bool lightsOn;

	private float nextZoneSearchTime;

	private AIInformationZone cachedInfoZone;

	private float targetAimedDuration;

	private float lastAimSetTime;

	private Vector3 aimOverridePosition = Vector3.zero;

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

	public override bool IsLoadBalanced()
	{
		return true;
	}

	public override void ServerInit()
	{
		base.ServerInit();
		brain = GetComponent<BaseAIBrain<HumanNPCNew>>();
		if (!base.isClient)
		{
			AIThinkManager.Add(this);
		}
	}

	internal override void DoServerDestroy()
	{
		AIThinkManager.Remove(this);
		base.DoServerDestroy();
	}

	public void LightCheck()
	{
		if ((TOD_Sky.Instance.IsNight && !lightsOn) || (TOD_Sky.Instance.IsDay && lightsOn))
		{
			LightToggle();
			lightsOn = !lightsOn;
		}
	}

	public override float GetAimConeScale()
	{
		return aimConeScale;
	}

	public override void EquipWeapon()
	{
		base.EquipWeapon();
	}

	public override void DismountObject()
	{
		base.DismountObject();
		lastDismountTime = Time.time;
	}

	public bool RecentlyDismounted()
	{
		return Time.time < lastDismountTime + 10f;
	}

	public virtual float GetIdealDistanceFromTarget()
	{
		return Mathf.Max(5f, EngagementRange() * 0.75f);
	}

	public AIInformationZone GetInformationZone(Vector3 pos)
	{
		if (VirtualInfoZone != null)
		{
			return VirtualInfoZone;
		}
		if (cachedInfoZone == null || Time.time > nextZoneSearchTime)
		{
			cachedInfoZone = AIInformationZone.GetForPoint(pos);
			nextZoneSearchTime = Time.time + 5f;
		}
		return cachedInfoZone;
	}

	public float EngagementRange()
	{
		AttackEntity attackEntity = GetAttackEntity();
		if ((bool)attackEntity)
		{
			return attackEntity.effectiveRange * (attackEntity.aiOnlyInRange ? 1f : 2f) * brain.AttackRangeMultiplier;
		}
		return brain.SenseRange;
	}

	public void SetDucked(bool flag)
	{
		modelState.ducked = flag;
		SendNetworkUpdate();
	}

	public virtual void TryThink()
	{
		ServerThink_Internal();
	}

	public override void ServerThink(float delta)
	{
		base.ServerThink(delta);
		if (brain.ShouldServerThink())
		{
			brain.DoThink();
		}
	}

	public void TickAttack(float delta, BaseCombatEntity target, bool targetIsLOS)
	{
		if (target == null)
		{
			return;
		}
		if (targetIsLOS)
		{
			if (Vector3.Dot(eyes.BodyForward(), target.CenterPoint() - eyes.position) > 0.85f)
			{
				targetAimedDuration += delta;
			}
		}
		else
		{
			targetAimedDuration = 0f;
		}
		if (targetAimedDuration > 0.5f)
		{
			bool flag = false;
			if ((object)this != null)
			{
				flag = ((IAIAttack)this).IsTargetInRange((BaseEntity)target);
			}
			else
			{
				AttackEntity attackEntity = GetAttackEntity();
				if ((bool)attackEntity)
				{
					flag = ((target != null) ? Vector3.Distance(base.transform.position, target.transform.position) : (-1f)) < attackEntity.effectiveRange * (attackEntity.aiOnlyInRange ? 1f : 2f);
				}
			}
			if (flag)
			{
				ShotTest();
			}
		}
		else
		{
			CancelBurst();
		}
	}

	public override void Hurt(HitInfo info)
	{
		if (base.isMounted)
		{
			info.damageTypes.ScaleAll(0.1f);
		}
		base.Hurt(info);
		BaseEntity initiator = info.Initiator;
		if (initiator != null && !initiator.EqualNetID(this))
		{
			brain.Senses.Memory.SetKnown(initiator, this, null);
		}
	}

	public float GetAimSwayScalar()
	{
		return 1f - Mathf.InverseLerp(1f, 3f, Time.time - lastGunShotTime);
	}

	public override Vector3 GetAimDirection()
	{
		if (brain != null && brain.Navigator != null && brain.Navigator.IsOverridingFacingDirection)
		{
			return brain.Navigator.FacingDirectionOverride;
		}
		return base.GetAimDirection();
	}

	public override void SetAimDirection(Vector3 newAim)
	{
		if (newAim == Vector3.zero)
		{
			return;
		}
		float num = Time.time - lastAimSetTime;
		lastAimSetTime = Time.time;
		AttackEntity attackEntity = GetAttackEntity();
		if ((bool)attackEntity)
		{
			newAim = attackEntity.ModifyAIAim(newAim, GetAimSwayScalar());
		}
		if (base.isMounted)
		{
			BaseMountable baseMountable = GetMounted();
			Vector3 eulerAngles = baseMountable.transform.eulerAngles;
			Quaternion quaternion = Quaternion.Euler(Quaternion.LookRotation(newAim, baseMountable.transform.up).eulerAngles);
			Vector3 eulerAngles2 = Quaternion.LookRotation(base.transform.InverseTransformDirection(quaternion * Vector3.forward), base.transform.up).eulerAngles;
			eulerAngles2 = BaseMountable.ConvertVector(eulerAngles2);
			Quaternion quaternion2 = Quaternion.Euler(Mathf.Clamp(eulerAngles2.x, baseMountable.pitchClamp.x, baseMountable.pitchClamp.y), Mathf.Clamp(eulerAngles2.y, baseMountable.yawClamp.x, baseMountable.yawClamp.y), eulerAngles.z);
			newAim = BaseMountable.ConvertVector(Quaternion.LookRotation(base.transform.TransformDirection(quaternion2 * Vector3.forward), base.transform.up).eulerAngles);
		}
		else
		{
			BaseEntity baseEntity = GetParentEntity();
			if ((bool)baseEntity)
			{
				Vector3 forward = new Vector3(y: baseEntity.transform.InverseTransformDirection(newAim).y, x: newAim.x, z: newAim.z);
				eyes.rotation = Quaternion.Lerp(eyes.rotation, Quaternion.LookRotation(forward, baseEntity.transform.up), num * 25f);
				viewAngles = eyes.bodyRotation.eulerAngles;
				ServerRotation = eyes.bodyRotation;
				return;
			}
		}
		eyes.rotation = (base.isMounted ? Quaternion.Slerp(eyes.rotation, Quaternion.Euler(newAim), num * 70f) : Quaternion.Lerp(eyes.rotation, Quaternion.LookRotation(newAim, base.transform.up), num * 25f));
		viewAngles = eyes.rotation.eulerAngles;
		ServerRotation = eyes.rotation;
	}

	public void SetStationaryAimPoint(Vector3 aimAt)
	{
		aimOverridePosition = aimAt;
	}

	public void ClearStationaryAimPoint()
	{
		aimOverridePosition = Vector3.zero;
	}

	public override bool ShouldDropActiveItem()
	{
		return false;
	}

	public override BaseCorpse CreateCorpse()
	{
		using (TimeWarning.New("Create corpse"))
		{
			NPCPlayerCorpse nPCPlayerCorpse = DropCorpse("assets/prefabs/npc/scientist/scientist_corpse.prefab") as NPCPlayerCorpse;
			if ((bool)nPCPlayerCorpse)
			{
				nPCPlayerCorpse.transform.position = nPCPlayerCorpse.transform.position + Vector3.down * NavAgent.baseOffset;
				nPCPlayerCorpse.SetLootableIn(2f);
				nPCPlayerCorpse.SetFlag(Flags.Reserved5, HasPlayerFlag(PlayerFlags.DisplaySash));
				nPCPlayerCorpse.SetFlag(Flags.Reserved2, true);
				nPCPlayerCorpse.TakeFrom(inventory.containerMain, inventory.containerWear, inventory.containerBelt);
				nPCPlayerCorpse.playerName = OverrideCorpseName();
				nPCPlayerCorpse.playerSteamID = userID;
				nPCPlayerCorpse.Spawn();
				nPCPlayerCorpse.TakeChildren(this);
				ItemContainer[] containers = nPCPlayerCorpse.containers;
				for (int i = 0; i < containers.Length; i++)
				{
					containers[i].Clear();
				}
				if (LootSpawnSlots.Length != 0)
				{
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
			}
			return nPCPlayerCorpse;
		}
	}

	protected virtual string OverrideCorpseName()
	{
		return base.displayName;
	}

	public override void AttackerInfo(PlayerLifeStory.DeathInfo info)
	{
		base.AttackerInfo(info);
		info.inflictorName = inventory.containerBelt.GetSlot(0).info.shortname;
		info.attackerName = "scientist";
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
		if (entity == null)
		{
			return false;
		}
		return entity.prefabID == prefabID;
	}

	public bool CanAttack(BaseEntity entity)
	{
		return true;
	}

	public bool IsTargetInRange(BaseEntity entity)
	{
		return Vector3.Distance(entity.transform.position, base.transform.position) <= EngagementRange();
	}

	public bool CanSeeTarget(BaseEntity entity)
	{
		BasePlayer basePlayer = entity as BasePlayer;
		if (basePlayer == null)
		{
			return true;
		}
		return IsPlayerVisibleToUs(basePlayer);
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
		return 5f;
	}

	public bool IsOnCooldown()
	{
		return false;
	}

	public bool StartAttacking(BaseEntity entity)
	{
		return true;
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
		BaseEntity result = null;
		float num = -1f;
		foreach (BaseEntity player in brain.Senses.Players)
		{
			if (!(player == null) && !(player.Health() <= 0f))
			{
				float value = Vector3.Distance(player.transform.position, base.transform.position);
				float num2 = 1f - Mathf.InverseLerp(1f, brain.SenseRange, value);
				float value2 = Vector3.Dot((player.transform.position - eyes.position).normalized, eyes.BodyForward());
				num2 += Mathf.InverseLerp(brain.VisionCone, 1f, value2) / 2f;
				num2 += (brain.Senses.Memory.IsLOS(player) ? 2f : 0f);
				if (num2 > num)
				{
					result = player;
					num = num2;
				}
			}
		}
		return result;
	}

	public void AttackTick(float delta, BaseEntity target, bool targetIsLOS)
	{
		BaseCombatEntity target2 = target as BaseCombatEntity;
		TickAttack(delta, target2, targetIsLOS);
	}

	public override bool IsOnGround()
	{
		return true;
	}
}
