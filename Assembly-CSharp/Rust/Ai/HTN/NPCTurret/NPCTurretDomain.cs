using System;
using System.Collections;
using System.Collections.Generic;
using Apex.AI;
using Apex.AI.Components;
using Apex.Ai.HTN;
using Apex.Serialization;
using ConVar;
using Rust.Ai.HTN.NPCTurret.Reasoners;
using Rust.Ai.HTN.NPCTurret.Sensors;
using Rust.Ai.HTN.Reasoning;
using Rust.Ai.HTN.Sensors;
using UnityEngine;
using UnityEngine.AI;

namespace Rust.Ai.HTN.NPCTurret
{
	public class NPCTurretDomain : HTNDomain
	{
		public class NPCTurretWorldStateEffect : EffectBase<NPCTurretContext>
		{
			[ApexSerialization]
			public Facts Fact;

			[ApexSerialization]
			public byte Value;

			public override void Apply(NPCTurretContext context, bool fromPlanner, bool temporary)
			{
				if (fromPlanner)
				{
					context.PushFactChangeDuringPlanning(Fact, Value, temporary);
				}
				else
				{
					context.SetFact(Fact, Value);
				}
			}

			public override void Reverse(NPCTurretContext context, bool fromPlanner)
			{
				if (fromPlanner)
				{
					context.PopFactChangeDuringPlanning(Fact);
				}
				else
				{
					context.WorldState[(uint)Fact] = context.PreviousWorldState[(uint)Fact];
				}
			}
		}

		public class NPCTurretWorldStateBoolEffect : EffectBase<NPCTurretContext>
		{
			[ApexSerialization]
			public Facts Fact;

			[ApexSerialization]
			public bool Value;

			public override void Apply(NPCTurretContext context, bool fromPlanner, bool temporary)
			{
				if (fromPlanner)
				{
					context.PushFactChangeDuringPlanning(Fact, Value, temporary);
				}
				else
				{
					context.SetFact(Fact, Value);
				}
			}

			public override void Reverse(NPCTurretContext context, bool fromPlanner)
			{
				if (fromPlanner)
				{
					context.PopFactChangeDuringPlanning(Fact);
				}
				else
				{
					context.WorldState[(uint)Fact] = context.PreviousWorldState[(uint)Fact];
				}
			}
		}

		public class NPCTurretWorldStateIncrementEffect : EffectBase<NPCTurretContext>
		{
			[ApexSerialization]
			public Facts Fact;

			[ApexSerialization]
			public byte Value;

			public override void Apply(NPCTurretContext context, bool fromPlanner, bool temporary)
			{
				if (fromPlanner)
				{
					byte b = context.PeekFactChangeDuringPlanning(Fact);
					context.PushFactChangeDuringPlanning(Fact, b + Value, temporary);
				}
				else
				{
					context.SetFact(Fact, context.GetFact(Fact) + Value);
				}
			}

			public override void Reverse(NPCTurretContext context, bool fromPlanner)
			{
				if (fromPlanner)
				{
					context.PopFactChangeDuringPlanning(Fact);
				}
				else
				{
					context.WorldState[(uint)Fact] = context.PreviousWorldState[(uint)Fact];
				}
			}
		}

		public class NPCTurretHealEffect : EffectBase<NPCTurretContext>
		{
			[ApexSerialization]
			public HealthState Health;

			public override void Apply(NPCTurretContext context, bool fromPlanner, bool temporary)
			{
				if (fromPlanner)
				{
					context.PushFactChangeDuringPlanning(Facts.HealthState, Health, temporary);
				}
				else
				{
					context.SetFact(Facts.HealthState, Health);
				}
			}

			public override void Reverse(NPCTurretContext context, bool fromPlanner)
			{
				if (fromPlanner)
				{
					context.PopFactChangeDuringPlanning(Facts.HealthState);
				}
				else
				{
					context.SetFact(Facts.HealthState, context.GetPreviousFact(Facts.HealthState));
				}
			}
		}

		public class NPCTurretHoldItemOfTypeEffect : EffectBase<NPCTurretContext>
		{
			[ApexSerialization]
			public ItemType Value;

			public override void Apply(NPCTurretContext context, bool fromPlanner, bool temporary)
			{
				if (fromPlanner)
				{
					context.PushFactChangeDuringPlanning(Facts.HeldItemType, Value, temporary);
				}
				else
				{
					context.SetFact(Facts.HeldItemType, Value);
				}
			}

			public override void Reverse(NPCTurretContext context, bool fromPlanner)
			{
				if (fromPlanner)
				{
					context.PopFactChangeDuringPlanning(Facts.HeldItemType);
				}
				else
				{
					context.SetFact(Facts.HeldItemType, context.GetPreviousFact(Facts.HeldItemType));
				}
			}
		}

		public class NPCTurretChangeFirearmOrder : EffectBase<NPCTurretContext>
		{
			[ApexSerialization]
			public FirearmOrders Order;

			public override void Apply(NPCTurretContext context, bool fromPlanner, bool temporary)
			{
				if (fromPlanner)
				{
					context.PushFactChangeDuringPlanning(Facts.FirearmOrder, Order, temporary);
				}
				else
				{
					context.SetFact(Facts.FirearmOrder, Order);
				}
			}

			public override void Reverse(NPCTurretContext context, bool fromPlanner)
			{
				if (fromPlanner)
				{
					context.PopFactChangeDuringPlanning(Facts.FirearmOrder);
				}
				else
				{
					context.SetFact(Facts.FirearmOrder, context.GetPreviousFact(Facts.FirearmOrder));
				}
			}
		}

		public class NPCTurretIdle_JustStandAround : OperatorBase<NPCTurretContext>
		{
			public override void Execute(NPCTurretContext context)
			{
				ResetWorldState(context);
				context.SetFact(Facts.IsIdle, true);
				context.Domain.ReloadFirearm();
			}

			public override OperatorStateType Tick(NPCTurretContext context, PrimitiveTaskSelector task)
			{
				return OperatorStateType.Running;
			}

			public override void Abort(NPCTurretContext context, PrimitiveTaskSelector task)
			{
				context.SetFact(Facts.IsIdle, false);
			}

			private void ResetWorldState(NPCTurretContext context)
			{
			}
		}

		public class NPCTurretApplyFirearmOrder : OperatorBase<NPCTurretContext>
		{
			public override void Execute(NPCTurretContext context)
			{
			}

			public override OperatorStateType Tick(NPCTurretContext context, PrimitiveTaskSelector task)
			{
				ApplyExpectedEffects(context, task);
				return OperatorStateType.Complete;
			}

			public override void Abort(NPCTurretContext context, PrimitiveTaskSelector task)
			{
			}
		}

		public class NPCTurretHoldItemOfType : OperatorBase<NPCTurretContext>
		{
			[ApexSerialization]
			private ItemType _item;

			[ApexSerialization]
			private float _switchTime = 0.2f;

			public override void Execute(NPCTurretContext context)
			{
				SwitchToItem(context, _item);
				context.Body.StartCoroutine(WaitAsync(context));
			}

			public override OperatorStateType Tick(NPCTurretContext context, PrimitiveTaskSelector task)
			{
				if (context.IsFact(Facts.IsWaiting))
				{
					return OperatorStateType.Running;
				}
				ApplyExpectedEffects(context, task);
				return OperatorStateType.Complete;
			}

			private IEnumerator WaitAsync(NPCTurretContext context)
			{
				context.SetFact(Facts.IsWaiting, true);
				yield return CoroutineEx.waitForSeconds(_switchTime);
				context.SetFact(Facts.IsWaiting, false);
			}

			public override void Abort(NPCTurretContext context, PrimitiveTaskSelector task)
			{
				_item = (ItemType)context.GetPreviousFact(Facts.HeldItemType);
				SwitchToItem(context, _item);
				context.SetFact(Facts.IsWaiting, false);
			}

			public static void SwitchToItem(NPCTurretContext context, ItemType _item)
			{
				context.Body.inventory.AllItemsNoAlloc(ref BaseNpcContext.InventoryLookupCache);
				foreach (Item item in BaseNpcContext.InventoryLookupCache)
				{
					if (_item == ItemType.HealingItem && item.info.category == ItemCategory.Medical && item.CanBeHeld())
					{
						context.Body.UpdateActiveItem(item.uid);
						context.SetFact(Facts.HeldItemType, _item);
						break;
					}
					if (_item == ItemType.MeleeWeapon && item.info.category == ItemCategory.Weapon && item.GetHeldEntity() is BaseMelee)
					{
						context.Body.UpdateActiveItem(item.uid);
						context.SetFact(Facts.HeldItemType, _item);
						break;
					}
					if (_item == ItemType.ProjectileWeapon && item.info.category == ItemCategory.Weapon && item.GetHeldEntity() is BaseProjectile)
					{
						context.Body.UpdateActiveItem(item.uid);
						context.SetFact(Facts.HeldItemType, _item);
						break;
					}
					if (_item == ItemType.ThrowableWeapon && item.info.category == ItemCategory.Weapon && item.GetHeldEntity() is ThrownWeapon)
					{
						context.Body.UpdateActiveItem(item.uid);
						context.SetFact(Facts.HeldItemType, _item);
						break;
					}
					if (_item == ItemType.LightSourceItem && item.info.category == ItemCategory.Tool && item.CanBeHeld())
					{
						context.Body.UpdateActiveItem(item.uid);
						context.SetFact(Facts.HeldItemType, _item);
						break;
					}
					if (_item == ItemType.ResearchItem && item.info.category == ItemCategory.Tool && item.CanBeHeld())
					{
						context.Body.UpdateActiveItem(item.uid);
						context.SetFact(Facts.HeldItemType, _item);
						break;
					}
				}
			}
		}

		public class NPCTurretReloadFirearmOperator : OperatorBase<NPCTurretContext>
		{
			public override void Execute(NPCTurretContext context)
			{
				context.Domain.ReloadFirearm();
			}

			public override OperatorStateType Tick(NPCTurretContext context, PrimitiveTaskSelector task)
			{
				if (context.IsFact(Facts.IsReloading))
				{
					return OperatorStateType.Running;
				}
				ApplyExpectedEffects(context, task);
				return OperatorStateType.Complete;
			}

			public override void Abort(NPCTurretContext context, PrimitiveTaskSelector task)
			{
			}
		}

		public delegate void OnPlanAborted(NPCTurretDomain domain);

		public delegate void OnPlanCompleted(NPCTurretDomain domain);

		public class NPCTurretHasWorldState : ContextualScorerBase<NPCTurretContext>
		{
			[ApexSerialization]
			public Facts Fact;

			[ApexSerialization]
			public byte Value;

			public override float Score(NPCTurretContext c)
			{
				if (c.GetWorldState(Fact) != Value)
				{
					return 0f;
				}
				return score;
			}
		}

		public class NPCTurretHasWorldStateBool : ContextualScorerBase<NPCTurretContext>
		{
			[ApexSerialization]
			public Facts Fact;

			[ApexSerialization]
			public bool Value;

			public override float Score(NPCTurretContext c)
			{
				byte b = (byte)(Value ? 1u : 0u);
				if (c.GetWorldState(Fact) != b)
				{
					return 0f;
				}
				return score;
			}
		}

		public class NPCTurretHasWorldStateGreaterThan : ContextualScorerBase<NPCTurretContext>
		{
			[ApexSerialization]
			public Facts Fact;

			[ApexSerialization]
			public byte Value;

			public override float Score(NPCTurretContext c)
			{
				if (c.GetWorldState(Fact) <= Value)
				{
					return 0f;
				}
				return score;
			}
		}

		public class NPCTurretHasWorldStateLessThan : ContextualScorerBase<NPCTurretContext>
		{
			[ApexSerialization]
			public Facts Fact;

			[ApexSerialization]
			public byte Value;

			public override float Score(NPCTurretContext c)
			{
				if (c.GetWorldState(Fact) >= Value)
				{
					return 0f;
				}
				return score;
			}
		}

		public class NPCTurretHasWorldStateEnemyRange : ContextualScorerBase<NPCTurretContext>
		{
			[ApexSerialization]
			public EnemyRange Value;

			public override float Score(NPCTurretContext c)
			{
				if ((uint)c.GetWorldState(Facts.EnemyRange) != (uint)Value)
				{
					return 0f;
				}
				return score;
			}
		}

		public class NPCTurretHasWorldStateAmmo : ContextualScorerBase<NPCTurretContext>
		{
			[ApexSerialization]
			public AmmoState Value;

			public override float Score(NPCTurretContext c)
			{
				if ((uint)c.GetWorldState(Facts.AmmoState) != (uint)Value)
				{
					return 0f;
				}
				return score;
			}
		}

		public class NPCTurretHasWorldStateHealth : ContextualScorerBase<NPCTurretContext>
		{
			[ApexSerialization]
			public HealthState Value;

			public override float Score(NPCTurretContext c)
			{
				if ((uint)c.GetWorldState(Facts.HealthState) != (uint)Value)
				{
					return 0f;
				}
				return score;
			}
		}

		public class NPCTurretHasItem : ContextualScorerBase<NPCTurretContext>
		{
			[ApexSerialization]
			public ItemType Value;

			public override float Score(NPCTurretContext c)
			{
				c.Body.inventory.AllItemsNoAlloc(ref BaseNpcContext.InventoryLookupCache);
				foreach (Item item in BaseNpcContext.InventoryLookupCache)
				{
					if (Value == ItemType.HealingItem && item.info.category == ItemCategory.Medical)
					{
						return score;
					}
					if (Value == ItemType.MeleeWeapon && item.info.category == ItemCategory.Weapon && item.GetHeldEntity() is BaseMelee)
					{
						return score;
					}
					if (Value == ItemType.ProjectileWeapon && item.info.category == ItemCategory.Weapon && item.GetHeldEntity() is BaseProjectile)
					{
						return score;
					}
					if (Value == ItemType.ThrowableWeapon && item.info.category == ItemCategory.Weapon && item.GetHeldEntity() is ThrownWeapon)
					{
						return score;
					}
					if (Value == ItemType.LightSourceItem && item.info.category == ItemCategory.Tool)
					{
						return score;
					}
					if (Value == ItemType.ResearchItem && item.info.category == ItemCategory.Tool)
					{
						return score;
					}
				}
				return 0f;
			}
		}

		public class NPCTurretIsHoldingItem : ContextualScorerBase<NPCTurretContext>
		{
			[ApexSerialization]
			public ItemType Value;

			public override float Score(NPCTurretContext c)
			{
				if ((uint)c.GetWorldState(Facts.HeldItemType) == (uint)Value)
				{
					return score;
				}
				return 0f;
			}
		}

		public class NPCTurretHasFirearmOrder : ContextualScorerBase<NPCTurretContext>
		{
			[ApexSerialization]
			public FirearmOrders Order;

			public override float Score(NPCTurretContext c)
			{
				return score;
			}
		}

		public class NPCTurretCanRememberPrimaryEnemyTarget : ContextualScorerBase<NPCTurretContext>
		{
			public override float Score(NPCTurretContext c)
			{
				if (!(c.Memory.PrimaryKnownEnemyPlayer.PlayerInfo.Player != null))
				{
					return 0f;
				}
				return score;
			}
		}

		[ReadOnly]
		[SerializeField]
		private bool _isRegisteredWithAgency;

		private Vector3 missOffset;

		private float missToHeadingAlignmentTime;

		private float repeatMissTime;

		private bool recalculateMissOffset = true;

		private bool isMissing;

		public OnPlanAborted OnPlanAbortedEvent;

		public OnPlanCompleted OnPlanCompletedEvent;

		[Header("Context")]
		[SerializeField]
		private NPCTurretContext _context;

		[Header("Navigation")]
		[ReadOnly]
		[SerializeField]
		private Vector3 _spawnPosition;

		[Header("Sensors")]
		[ReadOnly]
		[SerializeField]
		private List<INpcSensor> _sensors = new List<INpcSensor>
		{
			new PlayersInRangeSensor
			{
				TickFrequency = 0.5f
			},
			new PlayersOutsideRangeSensor
			{
				TickFrequency = 0.1f
			},
			new PlayersDistanceSensor
			{
				TickFrequency = 0.1f
			},
			new PlayersViewAngleSensor
			{
				TickFrequency = 0.1f
			},
			new EnemyPlayersInRangeSensor
			{
				TickFrequency = 0.1f
			},
			new EnemyPlayersLineOfSightSensor
			{
				TickFrequency = 0.25f
			},
			new EnemyPlayersHearingSensor
			{
				TickFrequency = 0.1f
			},
			new AnimalsInRangeSensor
			{
				TickFrequency = 1f
			},
			new AnimalDistanceSensor
			{
				TickFrequency = 0.25f
			}
		};

		[Header("Reasoners")]
		[ReadOnly]
		[SerializeField]
		private List<INpcReasoner> _reasoners = new List<INpcReasoner>
		{
			new EnemyPlayerLineOfSightReasoner
			{
				TickFrequency = 0.1f
			},
			new EnemyTargetReasoner
			{
				TickFrequency = 0.1f
			},
			new FireTacticReasoner
			{
				TickFrequency = 0.1f
			},
			new OrientationReasoner
			{
				TickFrequency = 0.01f
			},
			new FirearmPoseReasoner
			{
				TickFrequency = 0.1f
			},
			new HealthReasoner
			{
				TickFrequency = 0.1f
			},
			new AmmoReasoner
			{
				TickFrequency = 0.1f
			},
			new AnimalReasoner
			{
				TickFrequency = 0.25f
			},
			new AlertnessReasoner
			{
				TickFrequency = 0.1f
			}
		};

		[Header("Firearm Utility")]
		[ReadOnly]
		[SerializeField]
		private float _lastFirearmUsageTime;

		[ReadOnly]
		[SerializeField]
		private bool _isFiring;

		[ReadOnly]
		[SerializeField]
		public bool ReducedLongRangeAccuracy;

		[ReadOnly]
		[SerializeField]
		public bool BurstAtLongRange;

		private HTNUtilityAiClient _aiClient;

		public Vector3 SpawnPosition => _spawnPosition;

		public NPCTurretContext NPCTurretContext => _context;

		public override BaseNpcContext NpcContext => _context;

		public override IHTNContext PlannerContext => _context;

		public override IUtilityAI PlannerAi => _aiClient.ai;

		public override IUtilityAIClient PlannerAiClient => _aiClient;

		public override NavMeshAgent NavAgent => null;

		public override List<INpcSensor> Sensors => _sensors;

		public override List<INpcReasoner> Reasoners => _reasoners;

		private void InitializeAgency()
		{
			if (!(SingletonComponent<AiManager>.Instance == null) && SingletonComponent<AiManager>.Instance.enabled && ConVar.AI.npc_enable && !_isRegisteredWithAgency)
			{
				_isRegisteredWithAgency = true;
				SingletonComponent<AiManager>.Instance.HTNAgency.Add(_context.Body);
			}
		}

		private void RemoveAgency()
		{
			if (!(SingletonComponent<AiManager>.Instance == null) && _isRegisteredWithAgency)
			{
				_isRegisteredWithAgency = false;
				SingletonComponent<AiManager>.Instance.HTNAgency.Remove(_context.Body);
			}
		}

		public override void Resume()
		{
		}

		public override void Pause()
		{
		}

		private void TickFirearm(float time)
		{
			if (_context.GetFact(Facts.HasEnemyTarget) != 0 && !_isFiring && _context.IsBodyAlive())
			{
				switch (_context.GetFact(Facts.FirearmOrder))
				{
				case 1:
					TickFirearm(time, 0f);
					break;
				case 2:
					TickFirearm(time, 0.2f);
					break;
				case 3:
					TickFirearm(time, 0.5f);
					break;
				}
			}
		}

		private void TickFirearm(float time, float interval)
		{
			AttackEntity attackEntity = ReloadFirearmIfEmpty();
			if (attackEntity == null || !(attackEntity is BaseProjectile))
			{
				NPCTurretHoldItemOfType.SwitchToItem(_context, ItemType.ProjectileWeapon);
			}
			if (time - _lastFirearmUsageTime < interval || attackEntity == null)
			{
				return;
			}
			NpcPlayerInfo primaryEnemyPlayerTarget = _context.GetPrimaryEnemyPlayerTarget();
			if (primaryEnemyPlayerTarget.Player == null || (!primaryEnemyPlayerTarget.BodyVisible && !primaryEnemyPlayerTarget.HeadVisible) || !CanUseFirearmAtRange(primaryEnemyPlayerTarget.SqrDistance))
			{
				return;
			}
			BaseProjectile baseProjectile = attackEntity as BaseProjectile;
			if (!baseProjectile || !(baseProjectile.NextAttackTime > time))
			{
				switch (_context.GetFact(Facts.FireTactic))
				{
				default:
					FireSingle(attackEntity, time);
					break;
				case 0:
					FireBurst(baseProjectile, time);
					break;
				case 2:
					FireFullAuto(baseProjectile, time);
					break;
				}
			}
		}

		private void FireFullAuto(BaseProjectile proj, float time)
		{
			if (!(proj == null))
			{
				StartCoroutine(HoldTriggerLogic(proj, time, 4f));
			}
		}

		private void FireBurst(BaseProjectile proj, float time)
		{
			if (!(proj == null))
			{
				StartCoroutine(HoldTriggerLogic(proj, time, UnityEngine.Random.Range(proj.attackLengthMin, proj.attackLengthMax)));
			}
		}

		private void FireSingle(AttackEntity attackEnt, float time)
		{
			attackEnt.ServerUse(ConVar.AI.npc_htn_player_base_damage_modifier);
			_lastFirearmUsageTime = time + attackEnt.attackSpacing * 0.5f;
		}

		private IEnumerator HoldTriggerLogic(BaseProjectile proj, float startTime, float triggerDownInterval)
		{
			_isFiring = true;
			_lastFirearmUsageTime = startTime + triggerDownInterval + proj.attackSpacing;
			float damageModifier = (BurstAtLongRange ? 0.75f : 1f);
			while (UnityEngine.Time.time - startTime < triggerDownInterval && _context.IsBodyAlive() && _context.IsFact(Facts.CanSeeEnemy))
			{
				proj.ServerUse(ConVar.AI.npc_htn_player_base_damage_modifier * damageModifier);
				yield return CoroutineEx.waitForSeconds(proj.repeatDelay);
				if (proj.primaryMagazine.contents <= 0)
				{
					break;
				}
				if (BurstAtLongRange)
				{
					damageModifier *= 0.15f;
				}
			}
			_isFiring = false;
		}

		public AttackEntity GetFirearm()
		{
			return _context.Body.GetHeldEntity() as AttackEntity;
		}

		public BaseProjectile GetFirearmProj()
		{
			AttackEntity firearm = GetFirearm();
			if ((bool)firearm)
			{
				return firearm as BaseProjectile;
			}
			return null;
		}

		public BaseProjectile ReloadFirearmProjIfEmpty()
		{
			BaseProjectile firearmProj = GetFirearmProj();
			ReloadFirearmIfEmpty(firearmProj);
			return firearmProj;
		}

		public AttackEntity ReloadFirearmIfEmpty()
		{
			AttackEntity firearm = GetFirearm();
			if ((bool)firearm)
			{
				BaseProjectile proj = firearm as BaseProjectile;
				ReloadFirearmIfEmpty(proj);
			}
			return firearm;
		}

		public void ReloadFirearmIfEmpty(BaseProjectile proj)
		{
			if ((bool)proj && proj.primaryMagazine.contents <= 0)
			{
				ReloadFirearm(proj);
			}
		}

		public BaseProjectile ReloadFirearm()
		{
			BaseProjectile firearmProj = GetFirearmProj();
			ReloadFirearm(firearmProj);
			return firearmProj;
		}

		public void ReloadFirearm(BaseProjectile proj)
		{
			if ((bool)proj && _context.IsBodyAlive() && proj.primaryMagazine.contents < proj.primaryMagazine.capacity)
			{
				StartCoroutine(ReloadHandler(proj));
			}
		}

		private IEnumerator ReloadHandler(BaseProjectile proj)
		{
			_context.SetFact(Facts.IsReloading, true);
			proj.ServerReload();
			yield return CoroutineEx.waitForSeconds(proj.reloadTime);
			_context.SetFact(Facts.IsReloading, false);
		}

		private bool CanUseFirearmAtRange(float sqrRange)
		{
			AttackEntity firearm = GetFirearm();
			if (firearm == null)
			{
				return false;
			}
			if (sqrRange <= _context.Body.AiDefinition.Engagement.SqrCloseRangeFirearm(firearm))
			{
				return true;
			}
			if (sqrRange <= _context.Body.AiDefinition.Engagement.SqrMediumRangeFirearm(firearm))
			{
				return firearm.CanUseAtMediumRange;
			}
			return firearm.CanUseAtLongRange;
		}

		public override void ForceProjectileOrientation()
		{
			if (_context.OrientationType == NpcOrientation.LookAtAnimal || _context.OrientationType == NpcOrientation.PrimaryTargetBody || _context.OrientationType == NpcOrientation.PrimaryTargetHead)
			{
				return;
			}
			if (_context.Memory.PrimaryKnownEnemyPlayer.PlayerInfo.Player != null)
			{
				if (!_context.Memory.PrimaryKnownEnemyPlayer.PlayerInfo.BodyVisible && _context.Memory.PrimaryKnownEnemyPlayer.PlayerInfo.HeadVisible)
				{
					_context.OrientationType = NpcOrientation.PrimaryTargetHead;
				}
				else
				{
					_context.OrientationType = NpcOrientation.PrimaryTargetBody;
				}
			}
			else if (_context.Memory.PrimaryKnownAnimal.Animal != null)
			{
				_context.OrientationType = NpcOrientation.LookAtAnimal;
			}
		}

		public Vector3 ModifyFirearmAim(Vector3 heading, Vector3 target, Vector3 origin, float swayModifier = 1f)
		{
			if (!ConVar.AI.npc_use_new_aim_system)
			{
				AttackEntity firearm = GetFirearm();
				if ((bool)firearm)
				{
					return firearm.ModifyAIAim(heading, swayModifier);
				}
			}
			float sqrMagnitude = (target - origin).sqrMagnitude;
			float num = (int)_context.GetFact(Facts.Alertness);
			if (num > 10f)
			{
				num = 10f;
			}
			AttackEntity firearm2 = GetFirearm();
			if (sqrMagnitude <= _context.Body.AiDefinition.Engagement.SqrCloseRangeFirearm(firearm2) + 2f)
			{
				return heading;
			}
			float maxTime = 2f;
			float missOffsetMultiplier = 1f;
			if (ReducedLongRangeAccuracy && sqrMagnitude > _context.Body.AiDefinition.Engagement.SqrMediumRangeFirearm(firearm2))
			{
				num *= 0.05f;
				maxTime = 5f;
				missOffsetMultiplier = 5f;
			}
			return GetMissVector(heading, target, origin, maxTime, num * 2f, missOffsetMultiplier);
		}

		private Vector3 GetMissVector(Vector3 heading, Vector3 target, Vector3 origin, float maxTime, float repeatTime, float missOffsetMultiplier)
		{
			float time = UnityEngine.Time.time;
			if (!isMissing && repeatMissTime < time)
			{
				if (!recalculateMissOffset)
				{
					return heading;
				}
				missOffset = Vector3.zero;
				missOffset.x = ((UnityEngine.Random.value > 0.5f) ? 1f : (-1f));
				missOffset *= missOffsetMultiplier;
				missToHeadingAlignmentTime = time + maxTime;
				repeatMissTime = missToHeadingAlignmentTime + repeatTime;
				recalculateMissOffset = false;
				isMissing = true;
			}
			Vector3 vector = target + missOffset - origin;
			float num = Mathf.Max(missToHeadingAlignmentTime - time, 0f);
			float num2 = (Mathf.Approximately(num, 0f) ? 1f : (1f - Mathf.Min(num / maxTime, 1f)));
			if (Mathf.Approximately(num2, 1f))
			{
				recalculateMissOffset = true;
				isMissing = false;
				return Vector3.Lerp(vector.normalized, heading, 0.5f + UnityEngine.Random.value * 0.5f);
			}
			return Vector3.Lerp(vector.normalized, heading, num2);
		}

		public override void TickDestinationTracker()
		{
		}

		public override Vector3 GetHeadingDirection()
		{
			return _context.Body.eyes.rotation.eulerAngles.normalized;
		}

		public override Vector3 GetHomeDirection()
		{
			return _context.Body.eyes.rotation.eulerAngles.normalized;
		}

		public override float SqrDistanceToSpawn()
		{
			return 0f;
		}

		public override bool AllowedMovementDestination(Vector3 destination)
		{
			return false;
		}

		public override Vector3 GetNextPosition(float delta)
		{
			return _context.BodyPosition;
		}

		protected override void AbortPlan()
		{
			base.AbortPlan();
			OnPlanAbortedEvent?.Invoke(this);
		}

		protected override void CompletePlan()
		{
			base.CompletePlan();
			OnPlanCompletedEvent?.Invoke(this);
		}

		protected override void TickReasoner(INpcReasoner reasoner, float deltaTime, float time)
		{
			reasoner.Tick(_context.Body, deltaTime, time);
		}

		public override void OnSensation(Sensation sensation)
		{
			switch (sensation.Type)
			{
			case SensationType.Gunshot:
				OnGunshotSensation(ref sensation);
				break;
			case SensationType.ThrownWeapon:
				OnThrownWeaponSensation(ref sensation);
				break;
			case SensationType.Explosion:
				OnExplosionSensation(ref sensation);
				break;
			}
		}

		private void OnGunshotSensation(ref Sensation info)
		{
			BasePlayer initiatorPlayer = info.InitiatorPlayer;
			if (!(initiatorPlayer != null) || !(initiatorPlayer != _context.Body))
			{
				return;
			}
			bool flag = false;
			foreach (NpcPlayerInfo item in _context.EnemyPlayersInRange)
			{
				if (RememberGunshot(ref info, item, initiatorPlayer))
				{
					if (_context.Memory.PrimaryKnownEnemyPlayer.PlayerInfo.Player == null || _context.Memory.PrimaryKnownEnemyPlayer.PlayerInfo.Player == initiatorPlayer)
					{
						_context.Memory.RememberPrimaryEnemyPlayer(initiatorPlayer);
					}
					_context.IncrementFact(Facts.Alertness, 1);
					flag = true;
					break;
				}
			}
			if (!flag)
			{
				_context.IncrementFact(Facts.Alertness, 1);
				_context.PlayersOutsideDetectionRange.Add(new NpcPlayerInfo
				{
					Player = initiatorPlayer,
					Time = UnityEngine.Time.time
				});
			}
		}

		private void OnThrownWeaponSensation(ref Sensation info)
		{
			RememberEntityOfInterest(ref info);
			if (!_context.IsFact(Facts.CanSeeEnemy))
			{
				return;
			}
			BasePlayer initiatorPlayer = info.InitiatorPlayer;
			if (!(initiatorPlayer != null) || !(initiatorPlayer != _context.Body))
			{
				return;
			}
			bool flag = false;
			foreach (NpcPlayerInfo item in _context.EnemyPlayersInRange)
			{
				if (RememberThrownItem(ref info, item, initiatorPlayer))
				{
					if (_context.Memory.PrimaryKnownEnemyPlayer.PlayerInfo.Player == null)
					{
						_context.Memory.RememberPrimaryEnemyPlayer(initiatorPlayer);
					}
					_context.IncrementFact(Facts.Alertness, 1);
					flag = true;
					break;
				}
			}
			if (!flag)
			{
				_context.PlayersOutsideDetectionRange.Add(new NpcPlayerInfo
				{
					Player = initiatorPlayer,
					Time = UnityEngine.Time.time
				});
			}
		}

		private void OnExplosionSensation(ref Sensation info)
		{
			BasePlayer initiatorPlayer = info.InitiatorPlayer;
			if (!(initiatorPlayer != null) || !(initiatorPlayer != _context.Body))
			{
				return;
			}
			bool flag = false;
			foreach (NpcPlayerInfo item in _context.EnemyPlayersInRange)
			{
				if (RememberExplosion(ref info, item, initiatorPlayer))
				{
					_context.IncrementFact(Facts.Alertness, 1);
					flag = true;
					break;
				}
			}
			if (!flag)
			{
				_context.IncrementFact(Facts.Alertness, 1);
			}
		}

		private void RememberEntityOfInterest(ref Sensation info)
		{
			if (info.UsedEntity != null)
			{
				_context.Memory.RememberEntityOfInterest(_context.Body, info.UsedEntity, UnityEngine.Time.time, info.UsedEntity.name);
			}
		}

		private bool RememberGunshot(ref Sensation info, NpcPlayerInfo player, BasePlayer initiator)
		{
			if (player.Player == initiator)
			{
				float uncertainty = info.Radius * 0.1f;
				_context.Memory.RememberEnemyPlayer(_context.Body, ref player, UnityEngine.Time.time, uncertainty, "GUNSHOT!");
				return true;
			}
			return false;
		}

		private bool RememberExplosion(ref Sensation info, NpcPlayerInfo player, BasePlayer initiator)
		{
			return false;
		}

		private bool RememberThrownItem(ref Sensation info, NpcPlayerInfo player, BasePlayer initiator)
		{
			if (player.Player == initiator)
			{
				float uncertainty = info.Radius * 0.1f;
				_context.Memory.RememberEnemyPlayer(_context.Body, ref player, UnityEngine.Time.time, uncertainty, "THROW!");
				return true;
			}
			return false;
		}

		protected override void TickSensor(INpcSensor sensor, float deltaTime, float time)
		{
			sensor.Tick(_context.Body, deltaTime, time);
		}

		public override IAIContext GetContext(Guid aiId)
		{
			return _context;
		}

		public override void Initialize(BaseEntity body)
		{
			if (_aiClient == null || _aiClient.ai == null || _aiClient.ai.id != AINameMap.HTNDomainNPCTurret)
			{
				_aiClient = new HTNUtilityAiClient(AINameMap.HTNDomainNPCTurret, this);
			}
			if (_context == null || _context.Body != body)
			{
				_context = new NPCTurretContext(body as HTNPlayer, this);
			}
			_spawnPosition = body.transform.position;
			_aiClient.Initialize();
			_context.Body.Resume();
			InitializeAgency();
		}

		public override void Dispose()
		{
			_aiClient?.Kill();
			RemoveAgency();
		}

		public override void ResetState()
		{
			base.ResetState();
			_lastFirearmUsageTime = 0f;
			_isFiring = false;
		}

		public override void Tick(float time)
		{
			base.Tick(time);
			TickFirearm(time);
			_context.Memory.Forget(_context.Body.AiDefinition.Memory.ForgetTime);
		}

		public override void OnHurt(HitInfo info)
		{
			BasePlayer initiatorPlayer = info.InitiatorPlayer;
			if (!(initiatorPlayer != null) || !(initiatorPlayer != _context.Body))
			{
				return;
			}
			bool flag = false;
			foreach (NpcPlayerInfo item in _context.EnemyPlayersInRange)
			{
				if (RememberPlayerThatHurtUs(item, initiatorPlayer))
				{
					if (_context.Memory.PrimaryKnownEnemyPlayer.PlayerInfo.Player == null)
					{
						_context.Memory.RememberPrimaryEnemyPlayer(initiatorPlayer);
					}
					_context.IncrementFact(Facts.Alertness, 1);
					flag = true;
					break;
				}
			}
			if (!flag)
			{
				_context.IncrementFact(Facts.Alertness, 1);
				_context.PlayersOutsideDetectionRange.Add(new NpcPlayerInfo
				{
					Player = initiatorPlayer,
					Time = UnityEngine.Time.time
				});
			}
		}

		private bool RememberPlayerThatHurtUs(NpcPlayerInfo player, BasePlayer initiator)
		{
			if (player.Player == initiator)
			{
				float num = 0f;
				NpcPlayerInfo info = player;
				BaseProjectile baseProjectile = initiator.GetHeldEntity() as BaseProjectile;
				if (baseProjectile != null)
				{
					num = baseProjectile.NoiseRadius * 0.1f;
					if (baseProjectile.IsSilenced())
					{
						num *= 3f;
					}
				}
				_context.Memory.RememberEnemyPlayer(_context.Body, ref info, UnityEngine.Time.time, num, "HURT!");
				return true;
			}
			return false;
		}
	}
}
