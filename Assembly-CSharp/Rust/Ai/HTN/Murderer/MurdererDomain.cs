using System;
using System.Collections;
using System.Collections.Generic;
using Apex.AI;
using Apex.AI.Components;
using Apex.Ai.HTN;
using Apex.Serialization;
using ConVar;
using Rust.Ai.HTN.Murderer.Reasoners;
using Rust.Ai.HTN.Murderer.Sensors;
using Rust.Ai.HTN.Reasoning;
using Rust.Ai.HTN.Sensors;
using UnityEngine;
using UnityEngine.AI;

namespace Rust.Ai.HTN.Murderer
{
	public class MurdererDomain : HTNDomain
	{
		public class MurdererWorldStateEffect : EffectBase<MurdererContext>
		{
			[ApexSerialization]
			public Facts Fact;

			[ApexSerialization]
			public byte Value;

			public override void Apply(MurdererContext context, bool fromPlanner, bool temporary)
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

			public override void Reverse(MurdererContext context, bool fromPlanner)
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

		public class MurdererWorldStateBoolEffect : EffectBase<MurdererContext>
		{
			[ApexSerialization]
			public Facts Fact;

			[ApexSerialization]
			public bool Value;

			public override void Apply(MurdererContext context, bool fromPlanner, bool temporary)
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

			public override void Reverse(MurdererContext context, bool fromPlanner)
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

		public class MurdererWorldStateIncrementEffect : EffectBase<MurdererContext>
		{
			[ApexSerialization]
			public Facts Fact;

			[ApexSerialization]
			public byte Value;

			public override void Apply(MurdererContext context, bool fromPlanner, bool temporary)
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

			public override void Reverse(MurdererContext context, bool fromPlanner)
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

		public class MurdererIsNavigatingEffect : EffectBase<MurdererContext>
		{
			public override void Apply(MurdererContext context, bool fromPlanner, bool temporary)
			{
				if (fromPlanner)
				{
					context.PushFactChangeDuringPlanning(Facts.IsNavigating, 1, temporary);
					return;
				}
				context.PreviousWorldState[5] = context.WorldState[5];
				context.WorldState[5] = 1;
			}

			public override void Reverse(MurdererContext context, bool fromPlanner)
			{
				if (fromPlanner)
				{
					context.PopFactChangeDuringPlanning(Facts.IsNavigating);
				}
				else
				{
					context.WorldState[5] = context.PreviousWorldState[5];
				}
			}
		}

		public class MurdererIsNotNavigatingEffect : EffectBase<MurdererContext>
		{
			public override void Apply(MurdererContext context, bool fromPlanner, bool temporary)
			{
				ApplyStatic(context, fromPlanner, temporary);
			}

			public override void Reverse(MurdererContext context, bool fromPlanner)
			{
				if (fromPlanner)
				{
					context.PopFactChangeDuringPlanning(Facts.IsNavigating);
				}
				else
				{
					context.WorldState[5] = context.PreviousWorldState[5];
				}
			}

			public static void ApplyStatic(MurdererContext context, bool fromPlanner, bool temporary)
			{
				if (fromPlanner)
				{
					context.PushFactChangeDuringPlanning(Facts.IsNavigating, (byte)0, temporary);
					return;
				}
				context.PreviousWorldState[5] = context.WorldState[5];
				context.WorldState[5] = 0;
			}
		}

		public class MurdererHoldItemOfTypeEffect : EffectBase<MurdererContext>
		{
			[ApexSerialization]
			public ItemType Value;

			public override void Apply(MurdererContext context, bool fromPlanner, bool temporary)
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

			public override void Reverse(MurdererContext context, bool fromPlanner)
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

		public class MurdererChangeFirearmOrder : EffectBase<MurdererContext>
		{
			[ApexSerialization]
			public FirearmOrders Order;

			public override void Apply(MurdererContext context, bool fromPlanner, bool temporary)
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

			public override void Reverse(MurdererContext context, bool fromPlanner)
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

		public abstract class MurdererBaseNavigateTo : OperatorBase<MurdererContext>
		{
			[ApexSerialization]
			public bool RunUntilArrival = true;

			protected abstract Vector3 _GetDestination(MurdererContext context);

			protected virtual void OnPreStart(MurdererContext context)
			{
			}

			protected virtual void OnStart(MurdererContext context)
			{
			}

			protected virtual void OnPathFailed(MurdererContext context)
			{
			}

			protected virtual void OnPathComplete(MurdererContext context)
			{
			}

			public override void Execute(MurdererContext context)
			{
				OnPreStart(context);
				context.ReserveCoverPoint(null);
				context.Domain.SetDestination(_GetDestination(context));
				if (!RunUntilArrival)
				{
					context.OnWorldStateChangedEvent = (MurdererContext.WorldStateChangedEvent)Delegate.Combine(context.OnWorldStateChangedEvent, new MurdererContext.WorldStateChangedEvent(TrackWorldState));
				}
				OnStart(context);
			}

			protected void TrackWorldState(MurdererContext context, Facts fact, byte oldValue, byte newValue)
			{
				if (fact == Facts.PathStatus)
				{
					switch (newValue)
					{
					case 2:
						context.OnWorldStateChangedEvent = (MurdererContext.WorldStateChangedEvent)Delegate.Remove(context.OnWorldStateChangedEvent, new MurdererContext.WorldStateChangedEvent(TrackWorldState));
						MurdererIsNotNavigatingEffect.ApplyStatic(context, false, false);
						ApplyExpectedEffects(context, context.CurrentTask);
						context.Domain.StopNavigating();
						OnPathComplete(context);
						break;
					case 3:
						context.OnWorldStateChangedEvent = (MurdererContext.WorldStateChangedEvent)Delegate.Remove(context.OnWorldStateChangedEvent, new MurdererContext.WorldStateChangedEvent(TrackWorldState));
						MurdererIsNotNavigatingEffect.ApplyStatic(context, false, false);
						context.Domain.StopNavigating();
						OnPathFailed(context);
						break;
					}
				}
			}

			public override OperatorStateType Tick(MurdererContext context, PrimitiveTaskSelector task)
			{
				switch (context.GetFact(Facts.PathStatus))
				{
				default:
					context.Domain.StopNavigating();
					OnPathFailed(context);
					return OperatorStateType.Aborted;
				case 0:
				case 2:
					MurdererIsNotNavigatingEffect.ApplyStatic(context, false, false);
					ApplyExpectedEffects(context, task);
					context.Domain.StopNavigating();
					OnPathComplete(context);
					return OperatorStateType.Complete;
				case 1:
					if (RunUntilArrival)
					{
						return OperatorStateType.Running;
					}
					return OperatorStateType.Complete;
				}
			}

			public override void Abort(MurdererContext context, PrimitiveTaskSelector task)
			{
				MurdererIsNotNavigatingEffect.ApplyStatic(context, false, false);
				context.Domain.StopNavigating();
			}
		}

		public class MurdererRoamToRandomLocation : MurdererBaseNavigateTo
		{
			public static Vector3 GetDestination(MurdererContext context)
			{
				if (UnityEngine.Time.time - context.Memory.CachedRoamDestinationTime < 0.01f)
				{
					return context.Memory.CachedRoamDestination;
				}
				uint seed = (uint)((float)Mathf.Abs(context.Body.GetInstanceID()) + UnityEngine.Time.time);
				for (int i = 0; i < 10; i++)
				{
					Vector2 vector = SeedRandom.Value2D(seed) * 20f;
					if (vector.x < 0f)
					{
						vector.x -= 10f;
					}
					if (vector.x > 0f)
					{
						vector.x += 10f;
					}
					if (vector.y < 0f)
					{
						vector.y -= 10f;
					}
					if (vector.y > 0f)
					{
						vector.y += 10f;
					}
					Vector3 vector2 = context.BodyPosition + new Vector3(vector.x, 0f, vector.y);
					if (TerrainMeta.HeightMap != null)
					{
						vector2.y = TerrainMeta.HeightMap.GetHeight(vector2);
					}
					NavMeshHit hit;
					if (NavMesh.FindClosestEdge(vector2, out hit, context.Domain.NavAgent.areaMask))
					{
						vector2 = hit.position;
						if (!(WaterLevel.GetWaterDepth(vector2) > 0.01f))
						{
							context.Memory.CachedRoamDestination = vector2;
							context.Memory.CachedRoamDestinationTime = UnityEngine.Time.time;
							return vector2;
						}
					}
					else if (NavMesh.SamplePosition(vector2, out hit, 5f, context.Domain.NavAgent.areaMask))
					{
						vector2 = hit.position;
						if (!(WaterLevel.GetWaterDepth(vector2) > 0.01f))
						{
							context.Memory.CachedRoamDestination = vector2;
							context.Memory.CachedRoamDestinationTime = UnityEngine.Time.time;
							return vector2;
						}
					}
				}
				return context.Body.transform.position;
			}

			protected override Vector3 _GetDestination(MurdererContext context)
			{
				return GetDestination(context);
			}

			protected override void OnStart(MurdererContext context)
			{
				context.SetFact(Facts.IsRoaming, 1);
			}
		}

		public class MurdererNavigateCloserToPrimaryPlayerTarget : MurdererBaseNavigateTo
		{
			public static Vector3 GetDestination(MurdererContext context)
			{
				NpcPlayerInfo primaryEnemyPlayerTarget = context.GetPrimaryEnemyPlayerTarget();
				if (primaryEnemyPlayerTarget.Player != null)
				{
					return primaryEnemyPlayerTarget.Player.transform.position;
				}
				return context.Body.transform.position;
			}

			protected override Vector3 _GetDestination(MurdererContext context)
			{
				return GetDestination(context);
			}

			public override void Execute(MurdererContext context)
			{
				OnPreStart(context);
				context.ReserveCoverPoint(null);
				context.Domain.SetDestination(_GetDestination(context), true);
				if (!RunUntilArrival)
				{
					context.OnWorldStateChangedEvent = (MurdererContext.WorldStateChangedEvent)Delegate.Combine(context.OnWorldStateChangedEvent, new MurdererContext.WorldStateChangedEvent(base.TrackWorldState));
				}
				OnStart(context);
			}
		}

		public class MurdererChasePrimaryPlayerTarget : MurdererBaseNavigateTo
		{
			public static Vector3 GetPreferredFightingPosition(MurdererContext context)
			{
				return MurdererNavigateToPreferredFightingRange.GetPreferredFightingPosition(context);
			}

			protected override Vector3 _GetDestination(MurdererContext context)
			{
				return GetPreferredFightingPosition(context);
			}

			public override OperatorStateType Tick(MurdererContext context, PrimitiveTaskSelector task)
			{
				if (context.Memory == null || context.Memory.PrimaryKnownEnemyPlayer.PlayerInfo.Player == null || context.Memory.PrimaryKnownEnemyPlayer.PlayerInfo.Player.transform == null || context.Memory.PrimaryKnownEnemyPlayer.PlayerInfo.Player.IsDestroyed || context.Memory.PrimaryKnownEnemyPlayer.PlayerInfo.Player.IsWounded() || context.Memory.PrimaryKnownEnemyPlayer.PlayerInfo.Player.IsDead())
				{
					return OperatorStateType.Aborted;
				}
				Vector3 vector = _GetDestination(context);
				if (context.Memory != null && context.Memory.PrimaryKnownEnemyPlayer.PlayerInfo.Player != null)
				{
					if (context.Memory.PrimaryKnownEnemyPlayer.PlayerInfo.Player.estimatedSpeed2D < 0.01f)
					{
						context.Domain.NavAgent.stoppingDistance = 1f;
					}
					else
					{
						context.Domain.NavAgent.stoppingDistance = Halloween.scarecrow_chase_stopping_distance;
					}
					if ((context.Memory.PrimaryKnownEnemyPlayer.PlayerInfo.Player.transform.position - vector).SqrMagnitudeXZ() > 0.5f)
					{
						context.Domain.SetDestination(vector);
					}
				}
				return base.Tick(context, task);
			}

			protected override void OnPreStart(MurdererContext context)
			{
				context.Domain.NavAgent.stoppingDistance = Halloween.scarecrow_chase_stopping_distance;
			}

			protected override void OnPathFailed(MurdererContext context)
			{
				context.Domain.NavAgent.stoppingDistance = 1f;
			}

			protected override void OnPathComplete(MurdererContext context)
			{
				context.Domain.NavAgent.stoppingDistance = 1f;
			}
		}

		public class MurdererNavigateToPreferredFightingRange : MurdererBaseNavigateTo
		{
			public static Vector3 GetPreferredFightingPosition(MurdererContext context)
			{
				if (UnityEngine.Time.time - context.Memory.CachedPreferredDistanceDestinationTime < 0.01f)
				{
					return context.Memory.CachedPreferredDistanceDestination;
				}
				NpcPlayerInfo primaryEnemyPlayerTarget = context.GetPrimaryEnemyPlayerTarget();
				if (primaryEnemyPlayerTarget.Player != null)
				{
					float num = 1.5f;
					AttackEntity firearm = context.Domain.GetFirearm();
					if (firearm != null)
					{
						NPCPlayerApex.WeaponTypeEnum effectiveRangeType = firearm.effectiveRangeType;
						num = ((effectiveRangeType != NPCPlayerApex.WeaponTypeEnum.CloseRange) ? context.Body.AiDefinition.Engagement.CenterOfMediumRangeFirearm(firearm) : context.Body.AiDefinition.Engagement.CenterOfCloseRangeFirearm(firearm));
					}
					float num2 = num * num;
					Vector3 vector;
					if (primaryEnemyPlayerTarget.Player.estimatedSpeed2D > 5f)
					{
						num += 1.5f;
						vector = ((!(primaryEnemyPlayerTarget.SqrDistance <= num2)) ? (primaryEnemyPlayerTarget.Player.transform.position - context.Body.transform.position).normalized : (context.Body.transform.position - primaryEnemyPlayerTarget.Player.transform.position).normalized);
						if (Vector3.Dot(primaryEnemyPlayerTarget.Player.estimatedVelocity, vector) < 0f)
						{
							vector = ((!(primaryEnemyPlayerTarget.SqrDistance <= num2)) ? (context.Body.transform.position - primaryEnemyPlayerTarget.Player.transform.position).normalized : (primaryEnemyPlayerTarget.Player.transform.position - context.Body.transform.position).normalized);
						}
					}
					else
					{
						num -= 0.1f;
						vector = ((!(primaryEnemyPlayerTarget.SqrDistance <= num2)) ? (context.Body.transform.position - primaryEnemyPlayerTarget.Player.transform.position).normalized : (primaryEnemyPlayerTarget.Player.transform.position - context.Body.transform.position).normalized);
					}
					Vector3 vector2 = primaryEnemyPlayerTarget.Player.transform.position + vector * num;
					NavMeshHit hit;
					if (NavMesh.SamplePosition(vector2 + Vector3.up * 0.1f, out hit, 2f * context.Domain.NavAgent.height, -1))
					{
						Vector3 vector3 = context.Domain.ToAllowedMovementDestination(hit.position);
						if (context.Memory.IsValid(vector3))
						{
							context.Memory.CachedPreferredDistanceDestination = vector3;
							context.Memory.CachedPreferredDistanceDestinationTime = UnityEngine.Time.time;
							return vector3;
						}
					}
					else
					{
						context.Memory.AddFailedDestination(vector2);
					}
				}
				return context.Body.transform.position;
			}

			protected override Vector3 _GetDestination(MurdererContext context)
			{
				return GetPreferredFightingPosition(context);
			}
		}

		public class MurdererNavigateToLastKnownLocationOfPrimaryEnemyPlayer : MurdererBaseNavigateTo
		{
			[ApexSerialization]
			private bool DisableIsSearchingOnComplete = true;

			public static Vector3 GetDestination(MurdererContext context)
			{
				BaseNpcMemory.EnemyPlayerInfo primaryKnownEnemyPlayer = context.Memory.PrimaryKnownEnemyPlayer;
				NavMeshHit hit;
				if (primaryKnownEnemyPlayer.PlayerInfo.Player != null && !context.HasVisitedLastKnownEnemyPlayerLocation && NavMesh.FindClosestEdge(primaryKnownEnemyPlayer.LastKnownPosition, out hit, context.Domain.NavAgent.areaMask))
				{
					return hit.position;
				}
				return context.Body.transform.position;
			}

			protected override Vector3 _GetDestination(MurdererContext context)
			{
				return GetDestination(context);
			}

			protected override void OnPreStart(MurdererContext context)
			{
				context.Domain.NavAgent.stoppingDistance = 0.1f;
			}

			protected override void OnStart(MurdererContext context)
			{
				context.SetFact(Facts.IsSearching, true);
			}

			protected override void OnPathFailed(MurdererContext context)
			{
				context.SetFact(Facts.IsSearching, false);
				context.Domain.NavAgent.stoppingDistance = 1f;
				context.HasVisitedLastKnownEnemyPlayerLocation = false;
			}

			protected override void OnPathComplete(MurdererContext context)
			{
				if (DisableIsSearchingOnComplete)
				{
					context.SetFact(Facts.IsSearching, false);
				}
				context.Domain.NavAgent.stoppingDistance = 1f;
				context.HasVisitedLastKnownEnemyPlayerLocation = true;
			}
		}

		public class MurdererNavigateInDirectionOfLastKnownHeadingOfPrimaryEnemyPlayer : MurdererBaseNavigateTo
		{
			[ApexSerialization]
			private bool DisableIsSearchingOnComplete = true;

			public static Vector3 GetDestination(MurdererContext context)
			{
				BaseNpcMemory.EnemyPlayerInfo primaryKnownEnemyPlayer = context.Memory.PrimaryKnownEnemyPlayer;
				NavMeshHit hit;
				if (primaryKnownEnemyPlayer.PlayerInfo.Player != null && NavMesh.FindClosestEdge(primaryKnownEnemyPlayer.LastKnownPosition + primaryKnownEnemyPlayer.LastKnownHeading * 2f, out hit, context.Domain.NavAgent.areaMask))
				{
					Vector3 position = hit.position;
					context.Memory.LastClosestEdgeNormal = hit.normal;
					return position;
				}
				return context.Body.transform.position;
			}

			public static Vector3 GetContinuousDestinationFromBody(MurdererContext context)
			{
				if (context.Memory.LastClosestEdgeNormal.sqrMagnitude < 0.01f)
				{
					return context.Body.transform.position;
				}
				BaseNpcMemory.EnemyPlayerInfo primaryKnownEnemyPlayer = context.Memory.PrimaryKnownEnemyPlayer;
				if (primaryKnownEnemyPlayer.PlayerInfo.Player != null)
				{
					Vector3 vector = context.Body.estimatedVelocity.normalized;
					if (vector.sqrMagnitude < 0.01f)
					{
						vector = context.Body.estimatedVelocity.normalized;
					}
					if (vector.sqrMagnitude < 0.01f)
					{
						vector = primaryKnownEnemyPlayer.LastKnownHeading;
					}
					NavMeshHit hit;
					if (NavMesh.FindClosestEdge(context.Body.transform.position + vector * 2f, out hit, context.Domain.NavAgent.areaMask))
					{
						if (Vector3.Dot(context.Memory.LastClosestEdgeNormal, hit.normal) > 0.9f)
						{
							return hit.position;
						}
						context.Memory.LastClosestEdgeNormal = hit.normal;
						return hit.position + hit.normal * 0.25f;
					}
				}
				return context.Body.transform.position;
			}

			public override OperatorStateType Tick(MurdererContext context, PrimitiveTaskSelector task)
			{
				OperatorStateType operatorStateType = base.Tick(context, task);
				if (operatorStateType == OperatorStateType.Running)
				{
					if (context.Domain.NavAgent.remainingDistance < context.Domain.NavAgent.stoppingDistance + 0.5f)
					{
						OnContinuePath(context, task);
					}
					return operatorStateType;
				}
				return operatorStateType;
			}

			private void OnContinuePath(MurdererContext context, PrimitiveTaskSelector task)
			{
				Vector3 continuousDestinationFromBody = GetContinuousDestinationFromBody(context);
				if (!((context.Body.transform.position - continuousDestinationFromBody).sqrMagnitude <= 0.2f))
				{
					OnPreStart(context);
					context.Domain.SetDestination(continuousDestinationFromBody);
					OnStart(context);
				}
			}

			protected override Vector3 _GetDestination(MurdererContext context)
			{
				return GetDestination(context);
			}

			protected override void OnPreStart(MurdererContext context)
			{
				context.Domain.NavAgent.stoppingDistance = 0.1f;
			}

			protected override void OnStart(MurdererContext context)
			{
				context.SetFact(Facts.IsSearching, true);
			}

			protected override void OnPathFailed(MurdererContext context)
			{
				context.SetFact(Facts.IsSearching, false);
				context.Domain.NavAgent.stoppingDistance = 1f;
			}

			protected override void OnPathComplete(MurdererContext context)
			{
				if (DisableIsSearchingOnComplete)
				{
					context.SetFact(Facts.IsSearching, false);
				}
				context.Domain.NavAgent.stoppingDistance = 1f;
			}
		}

		public class MurdererNavigateToPositionWhereWeLastSawPrimaryEnemyPlayer : MurdererBaseNavigateTo
		{
			[ApexSerialization]
			private bool DisableIsSearchingOnComplete = true;

			public static Vector3 GetDestination(MurdererContext context)
			{
				BaseNpcMemory.EnemyPlayerInfo primaryKnownEnemyPlayer = context.Memory.PrimaryKnownEnemyPlayer;
				NavMeshHit hit;
				if (primaryKnownEnemyPlayer.PlayerInfo.Player != null && NavMesh.FindClosestEdge(primaryKnownEnemyPlayer.OurLastPositionWhenLastSeen, out hit, context.Domain.NavAgent.areaMask))
				{
					return context.Domain.ToAllowedMovementDestination(hit.position);
				}
				return context.Body.transform.position;
			}

			protected override Vector3 _GetDestination(MurdererContext context)
			{
				return GetDestination(context);
			}

			protected override void OnPreStart(MurdererContext context)
			{
				context.Domain.NavAgent.stoppingDistance = 0.1f;
			}

			protected override void OnStart(MurdererContext context)
			{
				context.SetFact(Facts.IsSearching, true);
			}

			protected override void OnPathFailed(MurdererContext context)
			{
				context.SetFact(Facts.IsSearching, false);
				context.Domain.NavAgent.stoppingDistance = 1f;
			}

			protected override void OnPathComplete(MurdererContext context)
			{
				if (DisableIsSearchingOnComplete)
				{
					context.SetFact(Facts.IsSearching, false);
				}
				context.Domain.NavAgent.stoppingDistance = 1f;
			}
		}

		public class MurdererNavigateAwayFromExplosive : MurdererBaseNavigateTo
		{
			[ApexSerialization]
			private bool DisableIsAvoidingExplosiveOnComplete = true;

			public static Vector3 GetDestination(MurdererContext context)
			{
				BaseEntity baseEntity = null;
				Vector3 vector = Vector3.zero;
				float num = float.MaxValue;
				for (int i = 0; i < context.Memory.KnownTimedExplosives.Count; i++)
				{
					BaseNpcMemory.EntityOfInterestInfo entityOfInterestInfo = context.Memory.KnownTimedExplosives[i];
					if (entityOfInterestInfo.Entity != null)
					{
						Vector3 vector2 = context.BodyPosition - entityOfInterestInfo.Entity.transform.position;
						float sqrMagnitude = vector2.sqrMagnitude;
						if (sqrMagnitude < num)
						{
							vector = vector2;
							num = sqrMagnitude;
							baseEntity = entityOfInterestInfo.Entity;
						}
					}
				}
				if (baseEntity != null)
				{
					vector.Normalize();
					NavMeshHit hit;
					if (NavMesh.FindClosestEdge(context.BodyPosition + vector * 10f, out hit, context.Domain.NavAgent.areaMask))
					{
						context.Memory.LastClosestEdgeNormal = hit.normal;
						return hit.position;
					}
				}
				return context.Body.transform.position;
			}

			protected override Vector3 _GetDestination(MurdererContext context)
			{
				return GetDestination(context);
			}

			protected override void OnPreStart(MurdererContext context)
			{
				context.Domain.NavAgent.stoppingDistance = 0.1f;
			}

			protected override void OnStart(MurdererContext context)
			{
				context.SetFact(Facts.IsAvoidingExplosive, true);
			}

			protected override void OnPathFailed(MurdererContext context)
			{
				context.SetFact(Facts.IsAvoidingExplosive, false);
				context.Domain.NavAgent.stoppingDistance = 1f;
			}

			protected override void OnPathComplete(MurdererContext context)
			{
				if (DisableIsAvoidingExplosiveOnComplete)
				{
					context.SetFact(Facts.IsAvoidingExplosive, false);
				}
				context.Domain.NavAgent.stoppingDistance = 1f;
			}
		}

		public class MurdererNavigateAwayFromAnimal : MurdererBaseNavigateTo
		{
			[ApexSerialization]
			private bool DisableIsAvoidingAnimalOnComplete = true;

			public static Vector3 GetDestination(MurdererContext context)
			{
				if (context.Memory.PrimaryKnownAnimal.Animal != null)
				{
					Vector3 normalized = (context.BodyPosition - context.Memory.PrimaryKnownAnimal.Animal.transform.position).normalized;
					NavMeshHit hit;
					if (NavMesh.FindClosestEdge(context.BodyPosition + normalized * 10f, out hit, context.Domain.NavAgent.areaMask))
					{
						context.Memory.LastClosestEdgeNormal = hit.normal;
						return hit.position;
					}
				}
				return context.Body.transform.position;
			}

			protected override Vector3 _GetDestination(MurdererContext context)
			{
				return GetDestination(context);
			}

			protected override void OnPreStart(MurdererContext context)
			{
				context.Domain.NavAgent.stoppingDistance = 0.1f;
			}

			protected override void OnStart(MurdererContext context)
			{
				context.SetFact(Facts.IsAvoidingAnimal, true);
			}

			protected override void OnPathFailed(MurdererContext context)
			{
				context.SetFact(Facts.IsAvoidingAnimal, false);
				context.Domain.NavAgent.stoppingDistance = 1f;
			}

			protected override void OnPathComplete(MurdererContext context)
			{
				if (DisableIsAvoidingAnimalOnComplete)
				{
					context.SetFact(Facts.IsAvoidingAnimal, false);
				}
				context.Domain.NavAgent.stoppingDistance = 1f;
			}
		}

		public class MurdererArrivedAtLocation : OperatorBase<MurdererContext>
		{
			public override void Execute(MurdererContext context)
			{
			}

			public override OperatorStateType Tick(MurdererContext context, PrimitiveTaskSelector task)
			{
				ApplyExpectedEffects(context, task);
				return OperatorStateType.Complete;
			}

			public override void Abort(MurdererContext context, PrimitiveTaskSelector task)
			{
			}
		}

		public class MurdererStopMoving : OperatorBase<MurdererContext>
		{
			public override void Execute(MurdererContext context)
			{
				MurdererIsNotNavigatingEffect.ApplyStatic(context, false, false);
				context.Domain.StopNavigating();
			}

			public override OperatorStateType Tick(MurdererContext context, PrimitiveTaskSelector task)
			{
				ApplyExpectedEffects(context, task);
				return OperatorStateType.Complete;
			}

			public override void Abort(MurdererContext context, PrimitiveTaskSelector task)
			{
			}
		}

		public class MurdererDuck : OperatorBase<MurdererContext>
		{
			public override void Execute(MurdererContext context)
			{
				context.Body.modelState.ducked = true;
				MurdererIsNotNavigatingEffect.ApplyStatic(context, false, false);
				context.Domain.StopNavigating();
			}

			public override OperatorStateType Tick(MurdererContext context, PrimitiveTaskSelector task)
			{
				ApplyExpectedEffects(context, task);
				return OperatorStateType.Complete;
			}

			public override void Abort(MurdererContext context, PrimitiveTaskSelector task)
			{
				context.Body.modelState.ducked = false;
			}
		}

		public class MurdererDuckTimed : OperatorBase<MurdererContext>
		{
			[ApexSerialization]
			private float _duckTimeMin = 1f;

			[ApexSerialization]
			private float _duckTimeMax = 1f;

			public override void Execute(MurdererContext context)
			{
				context.Body.modelState.ducked = true;
				context.SetFact(Facts.IsDucking, true);
				MurdererIsNotNavigatingEffect.ApplyStatic(context, false, false);
				context.Domain.StopNavigating();
				if (_duckTimeMin > _duckTimeMax)
				{
					float duckTimeMin = _duckTimeMin;
					_duckTimeMin = _duckTimeMax;
					_duckTimeMax = duckTimeMin;
				}
				float time = UnityEngine.Random.value * (_duckTimeMax - _duckTimeMin) + _duckTimeMin;
				context.Body.StartCoroutine(AsyncTimer(context, time));
			}

			public override OperatorStateType Tick(MurdererContext context, PrimitiveTaskSelector task)
			{
				if (context.IsFact(Facts.IsDucking))
				{
					return OperatorStateType.Running;
				}
				ApplyExpectedEffects(context, task);
				return OperatorStateType.Complete;
			}

			public override void Abort(MurdererContext context, PrimitiveTaskSelector task)
			{
				context.Body.StopCoroutine(AsyncTimer(context, 0f));
				Reset(context);
			}

			private IEnumerator AsyncTimer(MurdererContext context, float time)
			{
				yield return CoroutineEx.waitForSeconds(time);
				Reset(context);
			}

			private void Reset(MurdererContext context)
			{
				context.Body.modelState.ducked = false;
				context.SetFact(Facts.IsDucking, false);
			}
		}

		public class MurdererStand : OperatorBase<MurdererContext>
		{
			public override void Execute(MurdererContext context)
			{
				context.SetFact(Facts.IsStandingUp, true);
				context.Body.StartCoroutine(AsyncTimer(context, 0.2f));
			}

			public override OperatorStateType Tick(MurdererContext context, PrimitiveTaskSelector task)
			{
				if (context.IsFact(Facts.IsStandingUp))
				{
					return OperatorStateType.Running;
				}
				ApplyExpectedEffects(context, task);
				return OperatorStateType.Complete;
			}

			public override void Abort(MurdererContext context, PrimitiveTaskSelector task)
			{
				context.Body.StopCoroutine(AsyncTimer(context, 0f));
				Reset(context);
			}

			private IEnumerator AsyncTimer(MurdererContext context, float time)
			{
				yield return CoroutineEx.waitForSeconds(time);
				context.Body.modelState.ducked = false;
				context.SetFact(Facts.IsDucking, false);
				yield return CoroutineEx.waitForSeconds(time * 2f);
				context.SetFact(Facts.IsStandingUp, false);
			}

			private void Reset(MurdererContext context)
			{
				context.Body.modelState.ducked = false;
				context.SetFact(Facts.IsDucking, false);
				context.SetFact(Facts.IsStandingUp, false);
			}
		}

		public class MurdererIdle_JustStandAround : OperatorBase<MurdererContext>
		{
			public override void Execute(MurdererContext context)
			{
				ResetWorldState(context);
				context.SetFact(Facts.IsIdle, true);
				context.Domain.ReloadFirearm();
			}

			public override OperatorStateType Tick(MurdererContext context, PrimitiveTaskSelector task)
			{
				return OperatorStateType.Running;
			}

			public override void Abort(MurdererContext context, PrimitiveTaskSelector task)
			{
				context.SetFact(Facts.IsIdle, false);
			}

			private void ResetWorldState(MurdererContext context)
			{
				context.SetFact(Facts.IsSearching, false);
				context.SetFact(Facts.IsNavigating, false);
				context.SetFact(Facts.IsLookingAround, false);
			}
		}

		public class MurdererHoldLocation : OperatorBase<MurdererContext>
		{
			public override void Execute(MurdererContext context)
			{
				MurdererIsNotNavigatingEffect.ApplyStatic(context, false, false);
				context.Domain.StopNavigating();
			}

			public override OperatorStateType Tick(MurdererContext context, PrimitiveTaskSelector task)
			{
				return OperatorStateType.Running;
			}

			public override void Abort(MurdererContext context, PrimitiveTaskSelector task)
			{
			}
		}

		public class MurdererHoldLocationTimed : OperatorBase<MurdererContext>
		{
			[ApexSerialization]
			private float _duckTimeMin = 1f;

			[ApexSerialization]
			private float _duckTimeMax = 1f;

			public override void Execute(MurdererContext context)
			{
				MurdererIsNotNavigatingEffect.ApplyStatic(context, false, false);
				context.Domain.StopNavigating();
				context.SetFact(Facts.IsWaiting, true);
				if (_duckTimeMin > _duckTimeMax)
				{
					float duckTimeMin = _duckTimeMin;
					_duckTimeMin = _duckTimeMax;
					_duckTimeMax = duckTimeMin;
				}
				float time = UnityEngine.Random.value * (_duckTimeMax - _duckTimeMin) + _duckTimeMin;
				context.Body.StartCoroutine(AsyncTimer(context, time));
			}

			public override OperatorStateType Tick(MurdererContext context, PrimitiveTaskSelector task)
			{
				if (context.IsFact(Facts.IsWaiting))
				{
					return OperatorStateType.Running;
				}
				ApplyExpectedEffects(context, task);
				return OperatorStateType.Complete;
			}

			public override void Abort(MurdererContext context, PrimitiveTaskSelector task)
			{
				context.SetFact(Facts.IsWaiting, false);
			}

			private IEnumerator AsyncTimer(MurdererContext context, float time)
			{
				yield return CoroutineEx.waitForSeconds(time);
				context.SetFact(Facts.IsWaiting, false);
			}
		}

		public class MurdererApplyFirearmOrder : OperatorBase<MurdererContext>
		{
			public override void Execute(MurdererContext context)
			{
			}

			public override OperatorStateType Tick(MurdererContext context, PrimitiveTaskSelector task)
			{
				ApplyExpectedEffects(context, task);
				return OperatorStateType.Complete;
			}

			public override void Abort(MurdererContext context, PrimitiveTaskSelector task)
			{
			}
		}

		public class MurdererLookAround : OperatorBase<MurdererContext>
		{
			[ApexSerialization]
			private float _lookAroundTime = 1f;

			public override void Execute(MurdererContext context)
			{
				context.SetFact(Facts.IsLookingAround, true);
				context.Body.StartCoroutine(LookAroundAsync(context));
			}

			public override OperatorStateType Tick(MurdererContext context, PrimitiveTaskSelector task)
			{
				if (context.IsFact(Facts.IsLookingAround))
				{
					return OperatorStateType.Running;
				}
				ApplyExpectedEffects(context, task);
				return OperatorStateType.Complete;
			}

			private IEnumerator LookAroundAsync(MurdererContext context)
			{
				yield return CoroutineEx.waitForSeconds(_lookAroundTime);
				if (context.IsFact(Facts.CanSeeEnemy))
				{
					context.SetFact(Facts.IsSearching, false);
				}
				context.SetFact(Facts.IsLookingAround, false);
			}

			public override void Abort(MurdererContext context, PrimitiveTaskSelector task)
			{
				context.SetFact(Facts.IsSearching, false);
				context.SetFact(Facts.IsLookingAround, false);
			}
		}

		public class MurdererHoldItemOfType : OperatorBase<MurdererContext>
		{
			[ApexSerialization]
			private ItemType _item;

			[ApexSerialization]
			private float _switchTime = 0.2f;

			public override void Execute(MurdererContext context)
			{
				SwitchToItem(context, _item);
				context.Body.StartCoroutine(WaitAsync(context));
			}

			public override OperatorStateType Tick(MurdererContext context, PrimitiveTaskSelector task)
			{
				if (context.IsFact(Facts.IsWaiting))
				{
					return OperatorStateType.Running;
				}
				ApplyExpectedEffects(context, task);
				return OperatorStateType.Complete;
			}

			private IEnumerator WaitAsync(MurdererContext context)
			{
				context.SetFact(Facts.IsWaiting, true);
				yield return CoroutineEx.waitForSeconds(_switchTime);
				context.SetFact(Facts.IsWaiting, false);
			}

			public override void Abort(MurdererContext context, PrimitiveTaskSelector task)
			{
				ItemType previousFact = (ItemType)context.GetPreviousFact(Facts.HeldItemType);
				SwitchToItem(context, previousFact);
				context.SetFact(Facts.IsWaiting, false);
			}

			public static void SwitchToItem(MurdererContext context, ItemType _item)
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
					if (_item == ItemType.MeleeWeapon && (item.info.category == ItemCategory.Weapon || item.info.category == ItemCategory.Tool || item.info.category == ItemCategory.Misc) && item.GetHeldEntity() is BaseMelee)
					{
						context.Body.UpdateActiveItem(item.uid);
						context.SetFact(Facts.HeldItemType, _item);
						Chainsaw chainsaw = item.GetHeldEntity() as Chainsaw;
						if ((bool)chainsaw)
						{
							chainsaw.ServerNPCStart();
						}
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

		public class MurdererApplyFrustration : OperatorBase<MurdererContext>
		{
			public override void Execute(MurdererContext context)
			{
			}

			public override OperatorStateType Tick(MurdererContext context, PrimitiveTaskSelector task)
			{
				ApplyExpectedEffects(context, task);
				return OperatorStateType.Complete;
			}

			public override void Abort(MurdererContext context, PrimitiveTaskSelector task)
			{
			}
		}

		public class MurdererUseThrowableWeapon : OperatorBase<MurdererContext>
		{
			[ApexSerialization]
			private NpcOrientation _orientation = NpcOrientation.LastKnownPrimaryTargetLocation;

			public static float LastTimeThrown;

			public override void Execute(MurdererContext context)
			{
				if (context.Memory.PrimaryKnownEnemyPlayer.PlayerInfo.Player != null)
				{
					context.Body.StartCoroutine(UseItem(context));
				}
			}

			public override OperatorStateType Tick(MurdererContext context, PrimitiveTaskSelector task)
			{
				if (context.IsFact(Facts.IsThrowingWeapon))
				{
					return OperatorStateType.Running;
				}
				ApplyExpectedEffects(context, task);
				return OperatorStateType.Complete;
			}

			public override void Abort(MurdererContext context, PrimitiveTaskSelector task)
			{
				context.SetFact(Facts.IsThrowingWeapon, false);
				ItemType previousFact = (ItemType)context.GetPreviousFact(Facts.HeldItemType);
				MurdererHoldItemOfType.SwitchToItem(context, previousFact);
			}

			private IEnumerator UseItem(MurdererContext context)
			{
				Item activeItem = context.Body.GetActiveItem();
				if (activeItem != null)
				{
					LastTimeThrown = UnityEngine.Time.time;
					ThrownWeapon thrownWeapon = activeItem.GetHeldEntity() as ThrownWeapon;
					if (thrownWeapon != null)
					{
						context.SetFact(Facts.IsThrowingWeapon, true);
						yield return CoroutineEx.waitForSeconds(1f + UnityEngine.Random.value);
						context.OrientationType = _orientation;
						context.Body.ForceOrientationTick();
						yield return null;
						thrownWeapon.ServerThrow(context.Memory.PrimaryKnownEnemyPlayer.LastKnownPosition);
						MurdererHoldItemOfType.SwitchToItem(context, ItemType.MeleeWeapon);
						yield return CoroutineEx.waitForSeconds(1f);
					}
				}
				else
				{
					LastTimeThrown = UnityEngine.Time.time;
				}
				context.SetFact(Facts.IsThrowingWeapon, false);
				MurdererHoldItemOfType.SwitchToItem(context, ItemType.MeleeWeapon);
			}
		}

		public delegate void OnPlanAborted(MurdererDomain domain);

		public delegate void OnPlanCompleted(MurdererDomain domain);

		public class MurdererHasWorldState : ContextualScorerBase<MurdererContext>
		{
			[ApexSerialization]
			public Facts Fact;

			[ApexSerialization]
			public byte Value;

			public override float Score(MurdererContext c)
			{
				if (c.GetWorldState(Fact) != Value)
				{
					return 0f;
				}
				return score;
			}
		}

		public class MurdererHasWorldStateBool : ContextualScorerBase<MurdererContext>
		{
			[ApexSerialization]
			public Facts Fact;

			[ApexSerialization]
			public bool Value;

			public override float Score(MurdererContext c)
			{
				byte b = (byte)(Value ? 1u : 0u);
				if (c.GetWorldState(Fact) != b)
				{
					return 0f;
				}
				return score;
			}
		}

		public class MurdererHasWorldStateGreaterThan : ContextualScorerBase<MurdererContext>
		{
			[ApexSerialization]
			public Facts Fact;

			[ApexSerialization]
			public byte Value;

			public override float Score(MurdererContext c)
			{
				if (c.GetWorldState(Fact) <= Value)
				{
					return 0f;
				}
				return score;
			}
		}

		public class MurdererHasWorldStateLessThan : ContextualScorerBase<MurdererContext>
		{
			[ApexSerialization]
			public Facts Fact;

			[ApexSerialization]
			public byte Value;

			public override float Score(MurdererContext c)
			{
				if (c.GetWorldState(Fact) >= Value)
				{
					return 0f;
				}
				return score;
			}
		}

		public class MurdererHasWorldStateEnemyRange : ContextualScorerBase<MurdererContext>
		{
			[ApexSerialization]
			public EnemyRange Value;

			public override float Score(MurdererContext c)
			{
				if ((uint)c.GetWorldState(Facts.EnemyRange) != (uint)Value)
				{
					return 0f;
				}
				return score;
			}
		}

		public class MurdererHasWorldStateHealth : ContextualScorerBase<MurdererContext>
		{
			[ApexSerialization]
			public HealthState Value;

			public override float Score(MurdererContext c)
			{
				if ((uint)c.GetWorldState(Facts.HealthState) != (uint)Value)
				{
					return 0f;
				}
				return score;
			}
		}

		public class MurdererHasItem : ContextualScorerBase<MurdererContext>
		{
			[ApexSerialization]
			public ItemType Value;

			public override float Score(MurdererContext c)
			{
				if (!Test(c, Value))
				{
					return 0f;
				}
				return score;
			}

			public static bool Test(MurdererContext c, ItemType Value)
			{
				c.Body.inventory.AllItemsNoAlloc(ref BaseNpcContext.InventoryLookupCache);
				foreach (Item item in BaseNpcContext.InventoryLookupCache)
				{
					if (Value == ItemType.HealingItem && item.info.category == ItemCategory.Medical)
					{
						return true;
					}
					if (Value == ItemType.MeleeWeapon && (item.info.category == ItemCategory.Weapon || item.info.category == ItemCategory.Tool || item.info.category == ItemCategory.Misc) && item.GetHeldEntity() is BaseMelee)
					{
						return true;
					}
					if (Value == ItemType.ProjectileWeapon && item.info.category == ItemCategory.Weapon && item.GetHeldEntity() is BaseProjectile)
					{
						return true;
					}
					if (Value == ItemType.ThrowableWeapon && item.info.category == ItemCategory.Weapon && item.GetHeldEntity() is ThrownWeapon)
					{
						return true;
					}
					if (Value == ItemType.LightSourceItem && item.info.category == ItemCategory.Tool)
					{
						return true;
					}
					if (Value == ItemType.ResearchItem && item.info.category == ItemCategory.Tool)
					{
						return true;
					}
				}
				return false;
			}
		}

		public class MurdererIsHoldingItem : ContextualScorerBase<MurdererContext>
		{
			[ApexSerialization]
			public ItemType Value;

			public override float Score(MurdererContext c)
			{
				if ((uint)c.GetWorldState(Facts.HeldItemType) == (uint)Value)
				{
					return score;
				}
				return 0f;
			}
		}

		public class MurdererHasFirearmOrder : ContextualScorerBase<MurdererContext>
		{
			[ApexSerialization]
			public FirearmOrders Order;

			public override float Score(MurdererContext c)
			{
				return score;
			}
		}

		public class MurdererCanNavigateToPreferredFightingRange : ContextualScorerBase<MurdererContext>
		{
			[ApexSerialization]
			private bool CanNot;

			public override float Score(MurdererContext c)
			{
				Vector3 preferredFightingPosition = MurdererNavigateToPreferredFightingRange.GetPreferredFightingPosition(c);
				if ((preferredFightingPosition - c.Body.transform.position).sqrMagnitude < 0.01f)
				{
					if (!CanNot)
					{
						return 0f;
					}
					return score;
				}
				bool flag = c.Memory.IsValid(preferredFightingPosition);
				if (CanNot)
				{
					if (!flag)
					{
						return score;
					}
					return 0f;
				}
				if (!flag)
				{
					return 0f;
				}
				return score;
			}
		}

		public class MurdererCanRememberPrimaryEnemyTarget : ContextualScorerBase<MurdererContext>
		{
			public override float Score(MurdererContext c)
			{
				if (!(c.Memory.PrimaryKnownEnemyPlayer.PlayerInfo.Player != null))
				{
					return 0f;
				}
				return score;
			}
		}

		public class MurdererCanNavigateToLastKnownPositionOfPrimaryEnemyTarget : ContextualScorerBase<MurdererContext>
		{
			public override float Score(MurdererContext c)
			{
				if (c.HasVisitedLastKnownEnemyPlayerLocation)
				{
					return score;
				}
				Vector3 destination = MurdererNavigateToLastKnownLocationOfPrimaryEnemyPlayer.GetDestination(c);
				if (!c.Domain.AllowedMovementDestination(destination))
				{
					return 0f;
				}
				if ((destination - c.BodyPosition).sqrMagnitude < 0.1f)
				{
					return 0f;
				}
				if (!c.Memory.IsValid(destination))
				{
					return 0f;
				}
				return score;
			}
		}

		public class MurdererCanNavigateAwayFromExplosive : ContextualScorerBase<MurdererContext>
		{
			public override float Score(MurdererContext c)
			{
				if (!Try(c))
				{
					return 0f;
				}
				return score;
			}

			public static bool Try(MurdererContext c)
			{
				Vector3 destination = MurdererNavigateAwayFromExplosive.GetDestination(c);
				if (!c.Domain.AllowedMovementDestination(destination))
				{
					return false;
				}
				if ((destination - c.BodyPosition).sqrMagnitude < 0.1f)
				{
					return false;
				}
				return c.Memory.IsValid(destination);
			}
		}

		public class MurdererCanNavigateAwayFromAnimal : ContextualScorerBase<MurdererContext>
		{
			public override float Score(MurdererContext c)
			{
				if (!Try(c))
				{
					return 0f;
				}
				return score;
			}

			public static bool Try(MurdererContext c)
			{
				Vector3 destination = MurdererNavigateAwayFromAnimal.GetDestination(c);
				if (!c.Domain.AllowedMovementDestination(destination))
				{
					return false;
				}
				if ((destination - c.BodyPosition).sqrMagnitude < 0.1f)
				{
					return false;
				}
				return c.Memory.IsValid(destination);
			}
		}

		public class MurdererCanNavigateCloserToPrimaryPlayerTarget : ContextualScorerBase<MurdererContext>
		{
			public override float Score(MurdererContext c)
			{
				if (!Try(c))
				{
					return 0f;
				}
				return score;
			}

			public static bool Try(MurdererContext c)
			{
				if ((MurdererNavigateCloserToPrimaryPlayerTarget.GetDestination(c) - c.BodyPosition).SqrMagnitudeXZ() < 1f)
				{
					return false;
				}
				return true;
			}
		}

		public class MurdererCanNavigateToRoamLocation : ContextualScorerBase<MurdererContext>
		{
			public override float Score(MurdererContext c)
			{
				if (!Try(c))
				{
					return 0f;
				}
				return score;
			}

			public static bool Try(MurdererContext c)
			{
				if ((MurdererRoamToRandomLocation.GetDestination(c) - c.BodyPosition).SqrMagnitudeXZ() < 1f)
				{
					return false;
				}
				return true;
			}
		}

		public class MurdererCanUseWeaponAtCurrentRange : ContextualScorerBase<MurdererContext>
		{
			public override float Score(MurdererContext c)
			{
				if (!Try(c))
				{
					return 0f;
				}
				return score;
			}

			public static bool Try(MurdererContext c)
			{
				AttackEntity firearm = c.Domain.GetFirearm();
				if (firearm == null)
				{
					return false;
				}
				switch (c.GetFact(Facts.EnemyRange))
				{
				case 2:
					return firearm.CanUseAtLongRange;
				case 1:
					return firearm.CanUseAtMediumRange;
				case 3:
					return firearm.CanUseAtLongRange;
				default:
					return true;
				}
			}
		}

		public class MurdererCanThrowAtLastKnownLocation : ContextualScorerBase<MurdererContext>
		{
			public override float Score(MurdererContext c)
			{
				if (!Try(c))
				{
					return 0f;
				}
				return score;
			}

			public static bool Try(MurdererContext c)
			{
				if (!ConVar.AI.npc_use_thrown_weapons || !Halloween.scarecrows_throw_beancans)
				{
					return false;
				}
				if (c.Memory.PrimaryKnownEnemyPlayer.PlayerInfo.Player == null)
				{
					return false;
				}
				if (UnityEngine.Time.time - MurdererUseThrowableWeapon.LastTimeThrown < Halloween.scarecrow_throw_beancan_global_delay)
				{
					return false;
				}
				Vector3 vector = MurdererNavigateToLastKnownLocationOfPrimaryEnemyPlayer.GetDestination(c);
				if ((vector - c.BodyPosition).sqrMagnitude < 0.1f)
				{
					vector = c.Memory.PrimaryKnownEnemyPlayer.PlayerInfo.Player.transform.position;
					if ((vector - c.BodyPosition).sqrMagnitude < 0.1f)
					{
						return false;
					}
				}
				Vector3 vector2 = vector + PlayerEyes.EyeOffset;
				Vector3 vector3 = c.Memory.PrimaryKnownEnemyPlayer.PlayerInfo.Player.transform.position + PlayerEyes.EyeOffset;
				if ((vector2 - vector3).sqrMagnitude > 8f)
				{
					return false;
				}
				Vector3 vector4 = c.BodyPosition + PlayerEyes.EyeOffset;
				if (Mathf.Abs(Vector3.Dot((vector4 - vector2).normalized, (vector4 - vector3).normalized)) < 0.75f)
				{
					return false;
				}
				if (!c.Body.IsVisible(vector2) && (!c.Memory.PrimaryKnownEnemyPlayer.HeadVisibleWhenLastNoticed || c.Memory.PrimaryKnownEnemyPlayer.BodyVisibleWhenLastNoticed))
				{
					return false;
				}
				return true;
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

		private bool _passPathValidity;

		private static Vector3[] pathCornerCache = new Vector3[128];

		private static NavMeshPath _pathCache = null;

		public OnPlanAborted OnPlanAbortedEvent;

		public OnPlanCompleted OnPlanCompletedEvent;

		[Header("Context")]
		[SerializeField]
		private MurdererContext _context;

		[ReadOnly]
		[SerializeField]
		[Header("Navigation")]
		private NavMeshAgent _navAgent;

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
				TickFrequency = 0.2f
			},
			new PlayersViewAngleSensor
			{
				TickFrequency = 0.25f
			},
			new EnemyPlayersInRangeSensor
			{
				TickFrequency = 0.2f
			},
			new EnemyPlayersLineOfSightSensor
			{
				TickFrequency = 0.25f,
				MaxVisible = 1
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
				TickFrequency = 0.2f
			},
			new EnemyPlayerHearingReasoner
			{
				TickFrequency = 0.2f
			},
			new EnemyTargetReasoner
			{
				TickFrequency = 0.2f
			},
			new FireTacticReasoner
			{
				TickFrequency = 0.2f
			},
			new OrientationReasoner
			{
				TickFrequency = 0.01f
			},
			new PreferredFightingRangeReasoner
			{
				TickFrequency = 0.2f
			},
			new AtLastKnownEnemyPlayerLocationReasoner
			{
				TickFrequency = 0.2f
			},
			new HealthReasoner
			{
				TickFrequency = 0.2f
			},
			new VulnerabilityReasoner
			{
				TickFrequency = 0.2f
			},
			new FrustrationReasoner
			{
				TickFrequency = 0.25f
			},
			new ReturnHomeReasoner
			{
				TickFrequency = 1f
			},
			new AtHomeLocationReasoner
			{
				TickFrequency = 5f
			},
			new AnimalReasoner
			{
				TickFrequency = 0.25f
			},
			new AlertnessReasoner
			{
				TickFrequency = 0.2f
			},
			new EnemyRangeReasoner
			{
				TickFrequency = 0.2f
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

		private HTNUtilityAiClient _aiClient;

		private MurdererDefinition _murdererDefinition;

		public MurdererDefinition MurdererDefinition
		{
			get
			{
				if (_murdererDefinition == null)
				{
					_murdererDefinition = _context.Body.AiDefinition as MurdererDefinition;
				}
				return _murdererDefinition;
			}
		}

		public Vector3 SpawnPosition => _spawnPosition;

		public MurdererContext MurdererContext => _context;

		public override BaseNpcContext NpcContext => _context;

		public override IHTNContext PlannerContext => _context;

		public override IUtilityAI PlannerAi => _aiClient.ai;

		public override IUtilityAIClient PlannerAiClient => _aiClient;

		public override NavMeshAgent NavAgent => _navAgent;

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
			ResumeNavigation();
		}

		public override void Pause()
		{
			PauseNavigation();
		}

		private void TickFirearm(float time)
		{
			if (_context.GetFact(Facts.HasEnemyTarget) == 0 || _isFiring || !_context.IsBodyAlive())
			{
				_context.Body.modelState.aiming = _isFiring;
				return;
			}
			switch (_context.GetFact(Facts.FirearmOrder))
			{
			case 1:
				TickFirearm(time, 0f);
				return;
			case 2:
				TickFirearm(time, 0.2f);
				return;
			case 3:
				TickFirearm(time, 0.5f);
				return;
			}
			if (_context.GetFact(Facts.HeldItemType) == 2)
			{
				_context.Body.modelState.aiming = true;
			}
		}

		private void TickFirearm(float time, float interval)
		{
			AttackEntity attackEntity = ReloadFirearmIfEmpty();
			if (attackEntity == null || !(attackEntity is BaseMelee) || _context.GetFact(Facts.HeldItemType) == 2)
			{
				MurdererHoldItemOfType.SwitchToItem(_context, ItemType.MeleeWeapon);
				attackEntity = GetFirearm();
			}
			if (attackEntity == null)
			{
				return;
			}
			BaseMelee baseMelee = attackEntity as BaseMelee;
			if (baseMelee == null || baseMelee.effectiveRange > 2f)
			{
				_context.Body.modelState.aiming = false;
			}
			else
			{
				_context.Body.modelState.aiming = true;
			}
			if (time - _lastFirearmUsageTime < interval)
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
			if (_context.EnemyPlayersInLineOfSight.Count > 3)
			{
				attackEnt.ServerUse((1f + UnityEngine.Random.value * 0.5f) * ConVar.AI.npc_htn_player_base_damage_modifier);
			}
			else if (_context.PrimaryEnemyPlayerInLineOfSight.Player != null && _context.PrimaryEnemyPlayerInLineOfSight.Player.healthFraction < 0.2f)
			{
				attackEnt.ServerUse((0.1f + UnityEngine.Random.value * 0.5f) * ConVar.AI.npc_htn_player_base_damage_modifier);
			}
			else
			{
				attackEnt.ServerUse(ConVar.AI.npc_htn_player_base_damage_modifier);
			}
			_lastFirearmUsageTime = time + attackEnt.attackSpacing * (0.5f + UnityEngine.Random.value * 0.5f);
			_context.IncrementFact(Facts.Vulnerability, 1);
		}

		private IEnumerator HoldTriggerLogic(BaseProjectile proj, float startTime, float triggerDownInterval)
		{
			_isFiring = true;
			_lastFirearmUsageTime = startTime + triggerDownInterval + proj.attackSpacing;
			_context.IncrementFact(Facts.Vulnerability, 1);
			while (UnityEngine.Time.time - startTime < triggerDownInterval && _context.IsBodyAlive() && _context.IsFact(Facts.CanSeeEnemy))
			{
				if (_context.EnemyPlayersInLineOfSight.Count > 3)
				{
					proj.ServerUse((1f + UnityEngine.Random.value * 0.5f) * ConVar.AI.npc_htn_player_base_damage_modifier);
				}
				else if (_context.PrimaryEnemyPlayerInLineOfSight.Player != null && _context.PrimaryEnemyPlayerInLineOfSight.Player.healthFraction < 0.2f)
				{
					proj.ServerUse((0.1f + UnityEngine.Random.value * 0.5f) * ConVar.AI.npc_htn_player_base_damage_modifier);
				}
				else
				{
					proj.ServerUse(ConVar.AI.npc_htn_player_base_damage_modifier);
				}
				yield return CoroutineEx.waitForSeconds(proj.repeatDelay);
				if (proj.primaryMagazine.contents <= 0)
				{
					break;
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
			if (ReducedLongRangeAccuracy && sqrMagnitude > _context.Body.AiDefinition.Engagement.SqrMediumRangeFirearm(firearm2))
			{
				num *= 0.5f;
			}
			if (_context.PrimaryEnemyPlayerInLineOfSight.Player != null && (_context.PrimaryEnemyPlayerInLineOfSight.Player.modelState.jumped || (!_context.PrimaryEnemyPlayerInLineOfSight.BodyVisible && _context.PrimaryEnemyPlayerInLineOfSight.HeadVisible)))
			{
				num *= 0.5f;
			}
			return GetMissVector(heading, target, origin, ConVar.AI.npc_deliberate_miss_to_hit_alignment_time, num * ConVar.AI.npc_alertness_to_aim_modifier);
		}

		private Vector3 GetMissVector(Vector3 heading, Vector3 target, Vector3 origin, float maxTime, float repeatTime)
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
				missOffset *= ConVar.AI.npc_deliberate_miss_offset_multiplier;
				missToHeadingAlignmentTime = time + maxTime;
				repeatMissTime = missToHeadingAlignmentTime + repeatTime;
				recalculateMissOffset = false;
				isMissing = true;
			}
			Vector3 vector = target + missOffset - origin;
			float num = Mathf.Max(missToHeadingAlignmentTime - time, 0f);
			float num2 = MurdererDefinition.MissFunction.Evaluate(Mathf.Approximately(num, 0f) ? 1f : (1f - Mathf.Min(num / maxTime, 1f)));
			if (Mathf.Approximately(num2, 1f))
			{
				recalculateMissOffset = true;
				isMissing = false;
				float num3 = Mathf.Min(1f, ConVar.AI.npc_deliberate_hit_randomizer);
				return Vector3.Lerp(vector.normalized, heading, 1f - num3 + UnityEngine.Random.value * num3);
			}
			return Vector3.Lerp(vector.normalized, heading, num2);
		}

		public void PauseNavigation()
		{
			if (NavAgent != null && NavAgent.enabled)
			{
				NavAgent.enabled = false;
			}
		}

		public void ResumeNavigation()
		{
			if (!(NavAgent == null))
			{
				if (!NavAgent.isOnNavMesh)
				{
					StartCoroutine(TryForceToNavmesh());
					return;
				}
				NavAgent.enabled = true;
				NavAgent.stoppingDistance = 1f;
				UpdateNavmeshOffset();
			}
		}

		public override Vector3 GetNextPosition(float delta)
		{
			if (NavAgent == null || !NavAgent.isOnNavMesh || !NavAgent.hasPath)
			{
				return _context.BodyPosition;
			}
			return NavAgent.nextPosition;
		}

		private void UpdateNavmeshOffset()
		{
			float num = _spawnPosition.y - _context.BodyPosition.y;
			if (num < 0f)
			{
				num = Mathf.Max(num, -0.25f);
				NavAgent.baseOffset = num;
			}
		}

		private IEnumerator TryForceToNavmesh()
		{
			yield return null;
			int numTries = 0;
			float waitForRetryTime2 = 1f;
			float maxDistanceMultiplier = 2f;
			if (SingletonComponent<DynamicNavMesh>.Instance != null)
			{
				while (SingletonComponent<DynamicNavMesh>.Instance.IsBuilding)
				{
					yield return CoroutineEx.waitForSecondsRealtime(waitForRetryTime2);
					waitForRetryTime2 += 0.5f;
				}
			}
			waitForRetryTime2 = 1f;
			for (; numTries < 3; numTries++)
			{
				if (NavAgent != null && !NavAgent.isOnNavMesh)
				{
					NavMeshHit hit;
					if (NavMesh.SamplePosition(_context.Body.transform.position, out hit, NavAgent.height * maxDistanceMultiplier, NavAgent.areaMask))
					{
						_context.Body.transform.position = hit.position;
						NavAgent.Warp(_context.Body.transform.position);
						NavAgent.enabled = true;
						NavAgent.stoppingDistance = 1f;
						UpdateNavmeshOffset();
						yield break;
					}
					yield return CoroutineEx.waitForSecondsRealtime(waitForRetryTime2);
					maxDistanceMultiplier *= 1.5f;
					continue;
				}
				NavAgent.enabled = true;
				NavAgent.stoppingDistance = 1f;
				yield break;
			}
			int areaFromName = NavMesh.GetAreaFromName("Walkable");
			if ((NavAgent.areaMask & (1 << areaFromName)) == 0)
			{
				NavMeshBuildSettings settingsByIndex = NavMesh.GetSettingsByIndex(1);
				NavAgent.agentTypeID = settingsByIndex.agentTypeID;
				NavAgent.areaMask = 1 << areaFromName;
				yield return TryForceToNavmesh();
			}
			else if (_context.Body.transform != null && !_context.Body.IsDestroyed)
			{
				Debug.LogWarningFormat("Failed to spawn {0} on a valid navmesh.", base.name);
				_context.Body.Kill();
			}
		}

		public bool SetDestination(Vector3 destination, bool passPathValidity = false)
		{
			_passPathValidity = passPathValidity;
			if (NavAgent == null || !NavAgent.isOnNavMesh)
			{
				_context.SetFact(Facts.PathStatus, (byte)3, true, false, true);
				return false;
			}
			destination = ToAllowedMovementDestination(destination);
			_context.Memory.HasTargetDestination = true;
			_context.Memory.TargetDestination = destination;
			_context.Domain.NavAgent.destination = destination;
			if (!_passPathValidity && !IsPathValid())
			{
				_context.Memory.AddFailedDestination(_context.Memory.TargetDestination);
				_context.Domain.NavAgent.isStopped = true;
				_context.SetFact(Facts.PathStatus, (byte)3, true, false, true);
				return false;
			}
			_context.Domain.NavAgent.isStopped = false;
			_context.SetFact(Facts.PathStatus, (byte)1, true, false, true);
			return true;
		}

		public override void TickDestinationTracker()
		{
			if (NavAgent == null || !NavAgent.isOnNavMesh)
			{
				_context.SetFact(Facts.PathStatus, (byte)0, true, false, true);
				return;
			}
			if (!_passPathValidity && !IsPathValid())
			{
				_context.Memory.AddFailedDestination(_context.Memory.TargetDestination);
				_context.Domain.NavAgent.isStopped = true;
				_context.Memory.HasTargetDestination = false;
				_context.SetFact(Facts.PathStatus, (byte)3, true, false, true);
			}
			if (_context.Memory.HasTargetDestination && _context.Domain.NavAgent.remainingDistance <= _context.Domain.NavAgent.stoppingDistance)
			{
				_context.Memory.HasTargetDestination = false;
				_context.SetFact(Facts.PathStatus, (byte)2, true, false, true);
			}
			if (_context.Memory.HasTargetDestination && NavAgent.hasPath)
			{
				_context.SetFact(Facts.PathStatus, (byte)1, true, false, true);
			}
			else
			{
				_context.SetFact(Facts.PathStatus, (byte)0, true, false, true);
			}
		}

		public bool IsPathValid()
		{
			if (!_context.IsBodyAlive())
			{
				return false;
			}
			if (_context.Memory.HasTargetDestination && !_context.Domain.NavAgent.pathPending && (_context.Domain.NavAgent.pathStatus != 0 || (_context.Domain.NavAgent.destination - _context.Memory.TargetDestination).sqrMagnitude > 0.01f || float.IsInfinity(_context.Domain.NavAgent.remainingDistance) || ((_context.OrientationType == NpcOrientation.PrimaryTargetBody || _context.OrientationType == NpcOrientation.PrimaryTargetHead) && _context.Domain.NavAgent.remainingDistance <= _context.Domain.NavAgent.stoppingDistance && !_context.IsFact(Facts.AtLocationPreferredFightingRange))))
			{
				return false;
			}
			if (!AllowedMovementDestination(_context.Memory.TargetDestination))
			{
				return false;
			}
			return true;
		}

		public override Vector3 GetHeadingDirection()
		{
			if (NavAgent != null && NavAgent.isOnNavMesh && _context.GetFact(Facts.IsNavigating) > 0)
			{
				return NavAgent.desiredVelocity.normalized;
			}
			return _context.Body.transform.forward;
		}

		public override Vector3 GetHomeDirection()
		{
			Vector3 v = SpawnPosition - _context.BodyPosition;
			if (v.SqrMagnitudeXZ() < 0.01f)
			{
				return _context.Body.transform.forward;
			}
			return v.normalized;
		}

		public void StopNavigating()
		{
			if (NavAgent != null && NavAgent.isOnNavMesh)
			{
				NavAgent.isStopped = true;
			}
			_context.Memory.HasTargetDestination = false;
			_context.SetFact(Facts.PathStatus, (byte)0, true, false, true);
		}

		public bool PathDistanceIsValid(Vector3 from, Vector3 to, bool allowCloseRange = false)
		{
			float sqrMagnitude = (from - to).sqrMagnitude;
			if (sqrMagnitude > MurdererContext.Body.AiDefinition.Engagement.SqrMediumRange || (!allowCloseRange && sqrMagnitude < MurdererContext.Body.AiDefinition.Engagement.SqrCloseRange))
			{
				return true;
			}
			float num = Mathf.Sqrt(sqrMagnitude);
			if (_pathCache == null)
			{
				_pathCache = new NavMeshPath();
			}
			if (NavMesh.CalculatePath(from, to, NavAgent.areaMask, _pathCache))
			{
				int cornersNonAlloc = _pathCache.GetCornersNonAlloc(pathCornerCache);
				if (_pathCache.status == NavMeshPathStatus.PathComplete && cornersNonAlloc > 1)
				{
					float num2 = PathDistance(cornersNonAlloc, ref pathCornerCache, num + ConVar.AI.npc_cover_path_vs_straight_dist_max_diff);
					if (Mathf.Abs(num - num2) > ConVar.AI.npc_cover_path_vs_straight_dist_max_diff)
					{
						return false;
					}
				}
			}
			return true;
		}

		private float PathDistance(int count, ref Vector3[] path, float maxDistance)
		{
			if (count < 2)
			{
				return 0f;
			}
			Vector3 a = path[0];
			float num = 0f;
			for (int i = 0; i < count; i++)
			{
				Vector3 vector = path[i];
				num += Vector3.Distance(a, vector);
				a = vector;
				if (num > maxDistance)
				{
					return num;
				}
			}
			return num;
		}

		public override float SqrDistanceToSpawn()
		{
			return (_context.BodyPosition - SpawnPosition).sqrMagnitude;
		}

		public override bool AllowedMovementDestination(Vector3 destination)
		{
			if (Movement == MovementRule.FreeMove)
			{
				return true;
			}
			if (Movement == MovementRule.NeverMove)
			{
				return false;
			}
			return (SpawnPosition - destination).sqrMagnitude <= base.SqrMovementRadius + 0.1f;
		}

		public Vector3 ToAllowedMovementDestination(Vector3 destination)
		{
			if (!AllowedMovementDestination(destination))
			{
				Vector3 normalized = (destination - _context.Domain.SpawnPosition).normalized;
				destination = _context.Domain.SpawnPosition + normalized * MovementRadius;
			}
			return destination;
		}

		protected override void AbortPlan()
		{
			base.AbortPlan();
			OnPlanAbortedEvent?.Invoke(this);
			_context.SetFact(Facts.MaintainCover, 0);
			_context.SetFact(Facts.IsRoaming, 0);
			_context.SetFact(Facts.IsSearching, 0);
			_context.SetFact(Facts.IsReturningHome, 0);
			_context.Body.modelState.ducked = false;
			MurdererHoldItemOfType.SwitchToItem(_context, ItemType.MeleeWeapon);
		}

		protected override void CompletePlan()
		{
			base.CompletePlan();
			OnPlanCompletedEvent?.Invoke(this);
			_context.SetFact(Facts.MaintainCover, 0);
			_context.SetFact(Facts.IsRoaming, 0);
			_context.SetFact(Facts.IsSearching, 0);
			_context.SetFact(Facts.IsReturningHome, 0);
			_context.Body.modelState.ducked = false;
			MurdererHoldItemOfType.SwitchToItem(_context, ItemType.MeleeWeapon);
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
					_context.IncrementFact(Facts.Vulnerability, (!_context.IsFact(Facts.CanSeeEnemy)) ? 1 : 0);
					_context.IncrementFact(Facts.Alertness, 1);
					flag = true;
					break;
				}
			}
			if (!flag)
			{
				_context.IncrementFact(Facts.Vulnerability, 1);
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
			if (!_context.IsFact(Facts.CanSeeEnemy) || !_context.IsFact(Facts.CanHearEnemy))
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
					_context.IncrementFact(Facts.Vulnerability, (!_context.IsFact(Facts.CanSeeEnemy)) ? 1 : 0);
					_context.IncrementFact(Facts.Alertness, 1);
					flag = true;
					break;
				}
			}
			if (!flag)
			{
				_context.IncrementFact(Facts.Vulnerability, 1);
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
					_context.IncrementFact(Facts.Vulnerability, _context.IsFact(Facts.CanSeeEnemy) ? 1 : 2);
					_context.IncrementFact(Facts.Alertness, 1);
					flag = true;
					break;
				}
			}
			if (!flag)
			{
				_context.IncrementFact(Facts.Vulnerability, 1);
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
				float uncertainty = info.Radius * 0.05f;
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
				float uncertainty = info.Radius * 0.05f;
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
			if (_aiClient == null || _aiClient.ai == null || _aiClient.ai.id != AINameMap.HTNDomainMurderer)
			{
				_aiClient = new HTNUtilityAiClient(AINameMap.HTNDomainMurderer, this);
			}
			if (_context == null || _context.Body != body)
			{
				_context = new MurdererContext(body as HTNPlayer, this);
			}
			if (_navAgent == null)
			{
				_navAgent = GetComponent<NavMeshAgent>();
			}
			if ((bool)_navAgent)
			{
				_navAgent.updateRotation = false;
				_navAgent.updatePosition = false;
				_navAgent.speed = _context.Body.AiDefinition.Movement.DuckSpeed;
			}
			_spawnPosition = body.transform.position;
			_aiClient.Initialize();
			_context.Body.Resume();
			InitializeAgency();
			StartCoroutine(DelayedForcedThink());
		}

		private IEnumerator DelayedForcedThink()
		{
			while (!_context.IsFact(Facts.IsRoaming) && !_context.IsFact(Facts.HasEnemyTarget))
			{
				yield return CoroutineEx.waitForSeconds(3f);
				Think();
			}
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
			if (_context.IsFact(Facts.CanSeeEnemy) || _context.IsFact(Facts.IsSearching))
			{
				_navAgent.speed = _context.Body.AiDefinition.Movement.RunSpeed;
			}
			else
			{
				_navAgent.speed = _context.Body.AiDefinition.Movement.DuckSpeed;
			}
			if (_context.Body != null && _context.Memory != null)
			{
				_context.Body.SetFlag(BaseEntity.Flags.Reserved3, _context.Memory.PrimaryKnownEnemyPlayer.PlayerInfo.Player != null && _context.Body.IsAlive());
			}
		}

		public override void OnPreHurt(HitInfo info)
		{
			if (!info.isHeadshot)
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
		}

		public override void OnHurt(HitInfo info)
		{
			BasePlayer initiatorPlayer = info.InitiatorPlayer;
			if (initiatorPlayer != null && initiatorPlayer != _context.Body)
			{
				bool flag = false;
				foreach (NpcPlayerInfo item in _context.EnemyPlayersInRange)
				{
					if (RememberPlayerThatHurtUs(item, initiatorPlayer))
					{
						if (_context.Memory.PrimaryKnownEnemyPlayer.PlayerInfo.Player == null)
						{
							_context.Memory.RememberPrimaryEnemyPlayer(initiatorPlayer);
						}
						_context.IncrementFact(Facts.Vulnerability, _context.IsFact(Facts.CanSeeEnemy) ? 1 : 10);
						_context.IncrementFact(Facts.Alertness, 1);
						flag = true;
						break;
					}
				}
				if (!flag)
				{
					_context.IncrementFact(Facts.Vulnerability, 10);
					_context.IncrementFact(Facts.Alertness, 1);
					_context.PlayersOutsideDetectionRange.Add(new NpcPlayerInfo
					{
						Player = initiatorPlayer,
						Time = UnityEngine.Time.time
					});
				}
			}
			if (_context.ReservedCoverPoint != null && _context.IsFact(Facts.AtLocationCover))
			{
				_context.ReservedCoverPoint.CoverIsCompromised(ConVar.AI.npc_cover_compromised_cooldown);
				_context.ReserveCoverPoint(null);
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
