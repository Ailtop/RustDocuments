using System;
using System.Collections;
using System.Collections.Generic;
using Apex.AI;
using Apex.AI.Components;
using Apex.Ai.HTN;
using Apex.Serialization;
using ConVar;
using Rust.AI;
using Rust.Ai.HTN.Reasoning;
using Rust.Ai.HTN.ScientistAStar.Reasoners;
using Rust.Ai.HTN.ScientistAStar.Sensors;
using Rust.Ai.HTN.Sensors;
using UnityEngine;
using UnityEngine.AI;

namespace Rust.Ai.HTN.ScientistAStar
{
	public class ScientistAStarDomain : HTNDomain
	{
		public class AStarWorldStateEffect : EffectBase<ScientistAStarContext>
		{
			[ApexSerialization]
			public Facts Fact;

			[ApexSerialization]
			public byte Value;

			public override void Apply(ScientistAStarContext context, bool fromPlanner, bool temporary)
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

			public override void Reverse(ScientistAStarContext context, bool fromPlanner)
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

		public class AStarWorldStateBoolEffect : EffectBase<ScientistAStarContext>
		{
			[ApexSerialization]
			public Facts Fact;

			[ApexSerialization]
			public bool Value;

			public override void Apply(ScientistAStarContext context, bool fromPlanner, bool temporary)
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

			public override void Reverse(ScientistAStarContext context, bool fromPlanner)
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

		public class AStarWorldStateIncrementEffect : EffectBase<ScientistAStarContext>
		{
			[ApexSerialization]
			public Facts Fact;

			[ApexSerialization]
			public byte Value;

			public override void Apply(ScientistAStarContext context, bool fromPlanner, bool temporary)
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

			public override void Reverse(ScientistAStarContext context, bool fromPlanner)
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

		public class AStarHealEffect : EffectBase<ScientistAStarContext>
		{
			[ApexSerialization]
			public HealthState Health;

			public override void Apply(ScientistAStarContext context, bool fromPlanner, bool temporary)
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

			public override void Reverse(ScientistAStarContext context, bool fromPlanner)
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

		public class AStarIsNavigatingEffect : EffectBase<ScientistAStarContext>
		{
			public override void Apply(ScientistAStarContext context, bool fromPlanner, bool temporary)
			{
				if (fromPlanner)
				{
					context.PushFactChangeDuringPlanning(Facts.IsNavigating, 1, temporary);
					return;
				}
				context.PreviousWorldState[5] = context.WorldState[5];
				context.WorldState[5] = 1;
			}

			public override void Reverse(ScientistAStarContext context, bool fromPlanner)
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

		public class AStarIsNotNavigatingEffect : EffectBase<ScientistAStarContext>
		{
			public override void Apply(ScientistAStarContext context, bool fromPlanner, bool temporary)
			{
				ApplyStatic(context, fromPlanner, temporary);
			}

			public override void Reverse(ScientistAStarContext context, bool fromPlanner)
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

			public static void ApplyStatic(ScientistAStarContext context, bool fromPlanner, bool temporary)
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

		public class AStarHoldItemOfTypeEffect : EffectBase<ScientistAStarContext>
		{
			[ApexSerialization]
			public ItemType Value;

			public override void Apply(ScientistAStarContext context, bool fromPlanner, bool temporary)
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

			public override void Reverse(ScientistAStarContext context, bool fromPlanner)
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

		public class AStarChangeFirearmOrder : EffectBase<ScientistAStarContext>
		{
			[ApexSerialization]
			public FirearmOrders Order;

			public override void Apply(ScientistAStarContext context, bool fromPlanner, bool temporary)
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

			public override void Reverse(ScientistAStarContext context, bool fromPlanner)
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

		public class AStarFutureCoverState : EffectBase<ScientistAStarContext>
		{
			[ApexSerialization]
			public CoverTactic Tactic;

			public override void Apply(ScientistAStarContext context, bool fromPlanner, bool temporary)
			{
				CoverPoint cover = AStarNavigateToCover.GetCover(Tactic, context);
				if (fromPlanner)
				{
					context.PushFactChangeDuringPlanning(Facts.CoverState, (cover != null) ? ((cover.NormalCoverType == CoverPoint.CoverType.Partial) ? CoverState.Partial : CoverState.Full) : CoverState.None, temporary);
				}
				else
				{
					context.SetFact(Facts.CoverState, (cover != null) ? ((cover.NormalCoverType == CoverPoint.CoverType.Partial) ? CoverState.Partial : CoverState.Full) : CoverState.None);
				}
			}

			public override void Reverse(ScientistAStarContext context, bool fromPlanner)
			{
				if (fromPlanner)
				{
					context.PopFactChangeDuringPlanning(Facts.CoverState);
				}
				else
				{
					context.SetFact(Facts.CoverState, context.GetPreviousFact(Facts.CoverState));
				}
			}
		}

		public abstract class BaseNavigateTo : OperatorBase<ScientistAStarContext>
		{
			[ApexSerialization]
			public bool RunUntilArrival = true;

			protected abstract Vector3 _GetDestination(ScientistAStarContext context);

			protected virtual void OnPreStart(ScientistAStarContext context)
			{
			}

			protected virtual void OnStart(ScientistAStarContext context)
			{
			}

			protected virtual void OnPathFailed(ScientistAStarContext context)
			{
			}

			protected virtual void OnPathComplete(ScientistAStarContext context)
			{
			}

			public override void Execute(ScientistAStarContext context)
			{
				OnPreStart(context);
				context.ReserveCoverPoint(null);
				context.Domain.SetDestination(_GetDestination(context));
				if (!RunUntilArrival)
				{
					context.OnWorldStateChangedEvent = (ScientistAStarContext.WorldStateChangedEvent)Delegate.Combine(context.OnWorldStateChangedEvent, new ScientistAStarContext.WorldStateChangedEvent(TrackWorldState));
				}
				OnStart(context);
			}

			private void TrackWorldState(ScientistAStarContext context, Facts fact, byte oldValue, byte newValue)
			{
				if (fact == Facts.PathStatus)
				{
					switch (newValue)
					{
					case 2:
						context.OnWorldStateChangedEvent = (ScientistAStarContext.WorldStateChangedEvent)Delegate.Remove(context.OnWorldStateChangedEvent, new ScientistAStarContext.WorldStateChangedEvent(TrackWorldState));
						AStarIsNotNavigatingEffect.ApplyStatic(context, false, false);
						ApplyExpectedEffects(context, context.CurrentTask);
						context.Domain.StopNavigating();
						OnPathComplete(context);
						break;
					case 3:
						context.OnWorldStateChangedEvent = (ScientistAStarContext.WorldStateChangedEvent)Delegate.Remove(context.OnWorldStateChangedEvent, new ScientistAStarContext.WorldStateChangedEvent(TrackWorldState));
						AStarIsNotNavigatingEffect.ApplyStatic(context, false, false);
						context.Domain.StopNavigating();
						OnPathFailed(context);
						break;
					}
				}
			}

			public override OperatorStateType Tick(ScientistAStarContext context, PrimitiveTaskSelector task)
			{
				switch (context.GetFact(Facts.PathStatus))
				{
				default:
					context.Domain.StopNavigating();
					OnPathFailed(context);
					return OperatorStateType.Aborted;
				case 0:
				case 2:
					AStarIsNotNavigatingEffect.ApplyStatic(context, false, false);
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

			public override void Abort(ScientistAStarContext context, PrimitiveTaskSelector task)
			{
				AStarIsNotNavigatingEffect.ApplyStatic(context, false, false);
				context.Domain.StopNavigating();
			}
		}

		public class AStarNavigateToCover : BaseNavigateTo
		{
			[ApexSerialization]
			private CoverTactic _preferredTactic;

			public static CoverPoint GetCover(CoverTactic tactic, ScientistAStarContext context)
			{
				switch (tactic)
				{
				case CoverTactic.Retreat:
					if (context.BestRetreatCover != null && context.BestRetreatCover.IsValidFor(context.Body))
					{
						return context.BestRetreatCover;
					}
					if (context.BestFlankCover != null && context.BestFlankCover.IsValidFor(context.Body))
					{
						return context.BestFlankCover;
					}
					if (context.BestAdvanceCover != null && context.BestAdvanceCover.IsValidFor(context.Body))
					{
						return context.BestAdvanceCover;
					}
					break;
				case CoverTactic.Flank:
					if (context.BestFlankCover != null && context.BestFlankCover.IsValidFor(context.Body))
					{
						return context.BestFlankCover;
					}
					if (context.BestRetreatCover != null && context.BestRetreatCover.IsValidFor(context.Body))
					{
						return context.BestRetreatCover;
					}
					if (context.BestAdvanceCover != null && context.BestAdvanceCover.IsValidFor(context.Body))
					{
						return context.BestAdvanceCover;
					}
					break;
				case CoverTactic.Advance:
					if (context.BestAdvanceCover != null && context.BestAdvanceCover.IsValidFor(context.Body))
					{
						return context.BestAdvanceCover;
					}
					if (context.BestFlankCover != null && context.BestFlankCover.IsValidFor(context.Body))
					{
						return context.BestFlankCover;
					}
					if (context.BestRetreatCover != null && context.BestRetreatCover.IsValidFor(context.Body))
					{
						return context.BestRetreatCover;
					}
					break;
				case CoverTactic.Closest:
					if (context.ClosestCover != null && context.ClosestCover.IsValidFor(context.Body))
					{
						return context.ClosestCover;
					}
					break;
				}
				return null;
			}

			private static Vector3 _GetCoverPosition(CoverTactic tactic, ScientistAStarContext context)
			{
				switch (tactic)
				{
				case CoverTactic.Retreat:
					if (context.BestRetreatCover != null && context.BestRetreatCover.IsValidFor(context.Body))
					{
						context.SetFact(Facts.CoverTactic, CoverTactic.Retreat);
						context.ReserveCoverPoint(context.BestRetreatCover);
						return context.BestRetreatCover.Position;
					}
					if (context.BestFlankCover != null && context.BestFlankCover.IsValidFor(context.Body))
					{
						context.SetFact(Facts.CoverTactic, CoverTactic.Flank);
						context.ReserveCoverPoint(context.BestFlankCover);
						return context.BestFlankCover.Position;
					}
					if (context.BestAdvanceCover != null && context.BestAdvanceCover.IsValidFor(context.Body))
					{
						context.SetFact(Facts.CoverTactic, CoverTactic.Advance);
						context.ReserveCoverPoint(context.BestAdvanceCover);
						return context.BestAdvanceCover.Position;
					}
					break;
				case CoverTactic.Flank:
					if (context.BestFlankCover != null && context.BestFlankCover.IsValidFor(context.Body))
					{
						context.SetFact(Facts.CoverTactic, CoverTactic.Flank);
						context.ReserveCoverPoint(context.BestFlankCover);
						return context.BestFlankCover.Position;
					}
					if (context.BestRetreatCover != null && context.BestRetreatCover.IsValidFor(context.Body))
					{
						context.SetFact(Facts.CoverTactic, CoverTactic.Retreat);
						context.ReserveCoverPoint(context.BestRetreatCover);
						return context.BestRetreatCover.Position;
					}
					if (context.BestAdvanceCover != null && context.BestAdvanceCover.IsValidFor(context.Body))
					{
						context.SetFact(Facts.CoverTactic, CoverTactic.Advance);
						context.ReserveCoverPoint(context.BestAdvanceCover);
						return context.BestAdvanceCover.Position;
					}
					break;
				case CoverTactic.Advance:
					if (context.BestAdvanceCover != null && context.BestAdvanceCover.IsValidFor(context.Body))
					{
						context.SetFact(Facts.CoverTactic, CoverTactic.Advance);
						context.ReserveCoverPoint(context.BestAdvanceCover);
						return context.BestAdvanceCover.Position;
					}
					if (context.BestFlankCover != null && context.BestFlankCover.IsValidFor(context.Body))
					{
						context.SetFact(Facts.CoverTactic, CoverTactic.Flank);
						context.ReserveCoverPoint(context.BestFlankCover);
						return context.BestFlankCover.Position;
					}
					if (context.BestRetreatCover != null && context.BestRetreatCover.IsValidFor(context.Body))
					{
						context.SetFact(Facts.CoverTactic, CoverTactic.Retreat);
						context.ReserveCoverPoint(context.BestRetreatCover);
						return context.BestRetreatCover.Position;
					}
					break;
				case CoverTactic.Closest:
					if (context.ClosestCover != null && context.ClosestCover.IsValidFor(context.Body))
					{
						context.SetFact(Facts.CoverTactic, CoverTactic.Closest);
						context.ReserveCoverPoint(context.ClosestCover);
						return context.ClosestCover.Position;
					}
					break;
				}
				return context.BodyPosition;
			}

			public static Vector3 GetCoverPosition(CoverTactic tactic, ScientistAStarContext context)
			{
				return _GetCoverPosition(tactic, context);
			}

			protected override Vector3 _GetDestination(ScientistAStarContext context)
			{
				return GetCoverPosition(_preferredTactic, context);
			}

			protected override void OnPathFailed(ScientistAStarContext context)
			{
				context.SetFact(Facts.CoverTactic, CoverTactic.None);
			}

			protected override void OnPathComplete(ScientistAStarContext context)
			{
				context.SetFact(Facts.CoverTactic, CoverTactic.None);
			}
		}

		public class AStarNavigateToWaypoint : BaseNavigateTo
		{
			public static Vector3 GetNextWaypointPosition(ScientistAStarContext context)
			{
				return context.BodyPosition + Vector3.forward * 10f;
			}

			protected override Vector3 _GetDestination(ScientistAStarContext context)
			{
				return GetNextWaypointPosition(context);
			}
		}

		public class AStarNavigateToPreferredFightingRange : BaseNavigateTo
		{
			public static Vector3 GetPreferredFightingPosition(ScientistAStarContext context)
			{
				if (UnityEngine.Time.time - context.Memory.CachedPreferredDistanceDestinationTime < 0.01f)
				{
					return context.Memory.CachedPreferredDistanceDestination;
				}
				NpcPlayerInfo primaryEnemyPlayerTarget = context.GetPrimaryEnemyPlayerTarget();
				if (primaryEnemyPlayerTarget.Player != null)
				{
					AttackEntity firearm = context.Domain.GetFirearm();
					float num = context.Body.AiDefinition.Engagement.CenterOfMediumRangeFirearm(firearm);
					float num2 = num * num;
					Vector3 vector = ((!(primaryEnemyPlayerTarget.SqrDistance < num2)) ? (primaryEnemyPlayerTarget.Player.transform.position - context.BodyPosition).normalized : (context.BodyPosition - primaryEnemyPlayerTarget.Player.transform.position).normalized);
					return context.BodyPosition + vector * num;
				}
				return context.BodyPosition;
			}

			protected override Vector3 _GetDestination(ScientistAStarContext context)
			{
				return GetPreferredFightingPosition(context);
			}
		}

		public class AStarNavigateToLastKnownLocationOfPrimaryEnemyPlayer : BaseNavigateTo
		{
			[ApexSerialization]
			private bool DisableIsSearchingOnComplete = true;

			public static Vector3 GetDestination(ScientistAStarContext context)
			{
				BaseNpcMemory.EnemyPlayerInfo primaryKnownEnemyPlayer = context.Memory.PrimaryKnownEnemyPlayer;
				if (primaryKnownEnemyPlayer.PlayerInfo.Player != null && !context.HasVisitedLastKnownEnemyPlayerLocation)
				{
					BasePathNode closestToPoint = context.Domain.Path.GetClosestToPoint(primaryKnownEnemyPlayer.LastKnownPosition);
					if (closestToPoint != null && closestToPoint.transform != null)
					{
						return closestToPoint.transform.position;
					}
				}
				return context.BodyPosition;
			}

			protected override Vector3 _GetDestination(ScientistAStarContext context)
			{
				return GetDestination(context);
			}

			protected override void OnPreStart(ScientistAStarContext context)
			{
				context.Domain.StoppingDistance = 0.25f;
			}

			protected override void OnStart(ScientistAStarContext context)
			{
				context.SetFact(Facts.IsSearching, true);
			}

			protected override void OnPathFailed(ScientistAStarContext context)
			{
				context.SetFact(Facts.IsSearching, false);
				context.Domain.StoppingDistance = 1f;
				context.HasVisitedLastKnownEnemyPlayerLocation = false;
			}

			protected override void OnPathComplete(ScientistAStarContext context)
			{
				if (DisableIsSearchingOnComplete)
				{
					context.SetFact(Facts.IsSearching, false);
				}
				context.Domain.StoppingDistance = 1f;
				context.HasVisitedLastKnownEnemyPlayerLocation = true;
			}
		}

		public class AStarNavigateInDirectionOfLastKnownHeadingOfPrimaryEnemyPlayer : BaseNavigateTo
		{
			[ApexSerialization]
			private bool DisableIsSearchingOnComplete = true;

			public static Vector3 GetDestination(ScientistAStarContext context)
			{
				BaseNpcMemory.EnemyPlayerInfo primaryKnownEnemyPlayer = context.Memory.PrimaryKnownEnemyPlayer;
				if (primaryKnownEnemyPlayer.PlayerInfo.Player != null)
				{
					Vector3 point = primaryKnownEnemyPlayer.LastKnownPosition + primaryKnownEnemyPlayer.LastKnownHeading * 2f;
					BasePathNode closestToPoint = context.Domain.Path.GetClosestToPoint(point);
					if (closestToPoint != null && closestToPoint.transform != null)
					{
						return closestToPoint.transform.position;
					}
				}
				return context.BodyPosition;
			}

			public static Vector3 GetContinuousDestinationFromBody(ScientistAStarContext context)
			{
				if (context.Memory.LastClosestEdgeNormal.sqrMagnitude < 0.01f)
				{
					return context.BodyPosition;
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
					return context.BodyPosition + vector * 2f;
				}
				return context.BodyPosition;
			}

			public override OperatorStateType Tick(ScientistAStarContext context, PrimitiveTaskSelector task)
			{
				OperatorStateType result = base.Tick(context, task);
				int num = 1;
				return result;
			}

			private void OnContinuePath(ScientistAStarContext context, PrimitiveTaskSelector task)
			{
				Vector3 continuousDestinationFromBody = GetContinuousDestinationFromBody(context);
				if (!((context.BodyPosition - continuousDestinationFromBody).sqrMagnitude <= 0.2f))
				{
					OnPreStart(context);
					context.Domain.SetDestination(continuousDestinationFromBody);
					OnStart(context);
				}
			}

			protected override Vector3 _GetDestination(ScientistAStarContext context)
			{
				return GetDestination(context);
			}

			protected override void OnPreStart(ScientistAStarContext context)
			{
				context.Domain.StoppingDistance = 0.25f;
			}

			protected override void OnStart(ScientistAStarContext context)
			{
				context.SetFact(Facts.IsSearching, true);
			}

			protected override void OnPathFailed(ScientistAStarContext context)
			{
				context.SetFact(Facts.IsSearching, false);
				context.Domain.StoppingDistance = 1f;
			}

			protected override void OnPathComplete(ScientistAStarContext context)
			{
				if (DisableIsSearchingOnComplete)
				{
					context.SetFact(Facts.IsSearching, false);
				}
				context.Domain.StoppingDistance = 1f;
			}
		}

		public class AStarNavigateToPositionWhereWeLastSawPrimaryEnemyPlayer : BaseNavigateTo
		{
			[ApexSerialization]
			private bool DisableIsSearchingOnComplete = true;

			public static Vector3 GetDestination(ScientistAStarContext context)
			{
				BaseNpcMemory.EnemyPlayerInfo primaryKnownEnemyPlayer = context.Memory.PrimaryKnownEnemyPlayer;
				if (primaryKnownEnemyPlayer.PlayerInfo.Player != null)
				{
					return primaryKnownEnemyPlayer.OurLastPositionWhenLastSeen;
				}
				return context.BodyPosition;
			}

			protected override Vector3 _GetDestination(ScientistAStarContext context)
			{
				return GetDestination(context);
			}

			protected override void OnPreStart(ScientistAStarContext context)
			{
				context.Domain.StoppingDistance = 0.25f;
			}

			protected override void OnStart(ScientistAStarContext context)
			{
				context.SetFact(Facts.IsSearching, true);
			}

			protected override void OnPathFailed(ScientistAStarContext context)
			{
				context.SetFact(Facts.IsSearching, false);
				context.Domain.StoppingDistance = 1f;
			}

			protected override void OnPathComplete(ScientistAStarContext context)
			{
				if (DisableIsSearchingOnComplete)
				{
					context.SetFact(Facts.IsSearching, false);
				}
				context.Domain.StoppingDistance = 1f;
			}
		}

		public class AStarNavigateAwayFromExplosive : BaseNavigateTo
		{
			[ApexSerialization]
			private bool DisableIsAvoidingExplosiveOnComplete = true;

			public static Vector3 GetDestination(ScientistAStarContext context)
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
					return context.BodyPosition + vector * 10f;
				}
				return context.BodyPosition;
			}

			protected override Vector3 _GetDestination(ScientistAStarContext context)
			{
				return GetDestination(context);
			}

			protected override void OnPreStart(ScientistAStarContext context)
			{
				context.Domain.StoppingDistance = 0.25f;
			}

			protected override void OnStart(ScientistAStarContext context)
			{
				context.SetFact(Facts.IsAvoidingExplosive, true);
			}

			protected override void OnPathFailed(ScientistAStarContext context)
			{
				context.SetFact(Facts.IsAvoidingExplosive, false);
				context.Domain.StoppingDistance = 1f;
			}

			protected override void OnPathComplete(ScientistAStarContext context)
			{
				if (DisableIsAvoidingExplosiveOnComplete)
				{
					context.SetFact(Facts.IsAvoidingExplosive, false);
				}
				context.Domain.StoppingDistance = 1f;
			}
		}

		public class AStarNavigateAwayFromAnimal : BaseNavigateTo
		{
			[ApexSerialization]
			private bool DisableIsAvoidingAnimalOnComplete = true;

			public static Vector3 GetDestination(ScientistAStarContext context)
			{
				if (context.Memory.PrimaryKnownAnimal.Animal != null)
				{
					Vector3 normalized = (context.BodyPosition - context.Memory.PrimaryKnownAnimal.Animal.transform.position).normalized;
					return context.BodyPosition + normalized * 10f;
				}
				return context.BodyPosition;
			}

			protected override Vector3 _GetDestination(ScientistAStarContext context)
			{
				return GetDestination(context);
			}

			protected override void OnPreStart(ScientistAStarContext context)
			{
				context.Domain.StoppingDistance = 0.25f;
			}

			protected override void OnStart(ScientistAStarContext context)
			{
				context.SetFact(Facts.IsAvoidingAnimal, true);
			}

			protected override void OnPathFailed(ScientistAStarContext context)
			{
				context.SetFact(Facts.IsAvoidingAnimal, false);
				context.Domain.StoppingDistance = 1f;
			}

			protected override void OnPathComplete(ScientistAStarContext context)
			{
				if (DisableIsAvoidingAnimalOnComplete)
				{
					context.SetFact(Facts.IsAvoidingAnimal, false);
				}
				context.Domain.StoppingDistance = 1f;
			}
		}

		public class AStarArrivedAtLocation : OperatorBase<ScientistAStarContext>
		{
			public override void Execute(ScientistAStarContext context)
			{
			}

			public override OperatorStateType Tick(ScientistAStarContext context, PrimitiveTaskSelector task)
			{
				ApplyExpectedEffects(context, task);
				return OperatorStateType.Complete;
			}

			public override void Abort(ScientistAStarContext context, PrimitiveTaskSelector task)
			{
			}
		}

		public class AStarStopMoving : OperatorBase<ScientistAStarContext>
		{
			public override void Execute(ScientistAStarContext context)
			{
				AStarIsNotNavigatingEffect.ApplyStatic(context, false, false);
				context.Domain.StopNavigating();
			}

			public override OperatorStateType Tick(ScientistAStarContext context, PrimitiveTaskSelector task)
			{
				ApplyExpectedEffects(context, task);
				return OperatorStateType.Complete;
			}

			public override void Abort(ScientistAStarContext context, PrimitiveTaskSelector task)
			{
			}
		}

		public class AStarNavigateToNextAStarWaypoint : BaseNavigateTo
		{
			private static int index;

			public static Vector3 GetDestination(ScientistAStarContext context)
			{
				Vector3 position = context.Domain.Path.nodes[index].transform.position;
				index++;
				if (index >= context.Domain.Path.nodes.Count)
				{
					index = 0;
				}
				return position;
			}

			protected override Vector3 _GetDestination(ScientistAStarContext context)
			{
				return GetDestination(context);
			}

			protected override void OnPreStart(ScientistAStarContext context)
			{
				context.Domain.StoppingDistance = 0.8f;
			}

			protected override void OnStart(ScientistAStarContext context)
			{
			}

			protected override void OnPathFailed(ScientistAStarContext context)
			{
				context.Domain.StoppingDistance = 1f;
			}

			protected override void OnPathComplete(ScientistAStarContext context)
			{
				context.Domain.StoppingDistance = 1f;
			}
		}

		public class AStarDuck : OperatorBase<ScientistAStarContext>
		{
			public override void Execute(ScientistAStarContext context)
			{
				context.Body.modelState.ducked = true;
				AStarIsNotNavigatingEffect.ApplyStatic(context, false, false);
				context.Domain.StopNavigating();
			}

			public override OperatorStateType Tick(ScientistAStarContext context, PrimitiveTaskSelector task)
			{
				ApplyExpectedEffects(context, task);
				return OperatorStateType.Complete;
			}

			public override void Abort(ScientistAStarContext context, PrimitiveTaskSelector task)
			{
				context.Body.modelState.ducked = false;
			}
		}

		public class AStarDuckTimed : OperatorBase<ScientistAStarContext>
		{
			[ApexSerialization]
			private float _duckTimeMin = 1f;

			[ApexSerialization]
			private float _duckTimeMax = 1f;

			public override void Execute(ScientistAStarContext context)
			{
				context.Body.modelState.ducked = true;
				context.SetFact(Facts.IsDucking, true);
				AStarIsNotNavigatingEffect.ApplyStatic(context, false, false);
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

			public override OperatorStateType Tick(ScientistAStarContext context, PrimitiveTaskSelector task)
			{
				if (context.IsFact(Facts.IsDucking))
				{
					return OperatorStateType.Running;
				}
				ApplyExpectedEffects(context, task);
				return OperatorStateType.Complete;
			}

			public override void Abort(ScientistAStarContext context, PrimitiveTaskSelector task)
			{
				context.Body.StopCoroutine(AsyncTimer(context, 0f));
				Reset(context);
			}

			private IEnumerator AsyncTimer(ScientistAStarContext context, float time)
			{
				yield return CoroutineEx.waitForSeconds(time);
				Reset(context);
			}

			private void Reset(ScientistAStarContext context)
			{
				context.Body.modelState.ducked = false;
				context.SetFact(Facts.IsDucking, false);
			}
		}

		public class AStarStand : OperatorBase<ScientistAStarContext>
		{
			public override void Execute(ScientistAStarContext context)
			{
				context.SetFact(Facts.IsStandingUp, true);
				context.Body.StartCoroutine(AsyncTimer(context, 0.2f));
			}

			public override OperatorStateType Tick(ScientistAStarContext context, PrimitiveTaskSelector task)
			{
				if (context.IsFact(Facts.IsStandingUp))
				{
					return OperatorStateType.Running;
				}
				ApplyExpectedEffects(context, task);
				return OperatorStateType.Complete;
			}

			public override void Abort(ScientistAStarContext context, PrimitiveTaskSelector task)
			{
				context.Body.StopCoroutine(AsyncTimer(context, 0f));
				Reset(context);
			}

			private IEnumerator AsyncTimer(ScientistAStarContext context, float time)
			{
				yield return CoroutineEx.waitForSeconds(time);
				context.Body.modelState.ducked = false;
				context.SetFact(Facts.IsDucking, false);
				yield return CoroutineEx.waitForSeconds(time * 2f);
				context.SetFact(Facts.IsStandingUp, false);
			}

			private void Reset(ScientistAStarContext context)
			{
				context.Body.modelState.ducked = false;
				context.SetFact(Facts.IsDucking, false);
				context.SetFact(Facts.IsStandingUp, false);
			}
		}

		public class AStarIdle_JustStandAround : OperatorBase<ScientistAStarContext>
		{
			public override void Execute(ScientistAStarContext context)
			{
				ResetWorldState(context);
				context.SetFact(Facts.IsIdle, true);
				context.Domain.ReloadFirearm();
			}

			public override OperatorStateType Tick(ScientistAStarContext context, PrimitiveTaskSelector task)
			{
				return OperatorStateType.Running;
			}

			public override void Abort(ScientistAStarContext context, PrimitiveTaskSelector task)
			{
				context.SetFact(Facts.IsIdle, false);
			}

			private void ResetWorldState(ScientistAStarContext context)
			{
				context.SetFact(Facts.IsSearching, false);
				context.SetFact(Facts.IsNavigating, false);
				context.SetFact(Facts.IsLookingAround, false);
			}
		}

		public class AStarHoldLocation : OperatorBase<ScientistAStarContext>
		{
			public override void Execute(ScientistAStarContext context)
			{
				AStarIsNotNavigatingEffect.ApplyStatic(context, false, false);
				context.Domain.StopNavigating();
			}

			public override OperatorStateType Tick(ScientistAStarContext context, PrimitiveTaskSelector task)
			{
				return OperatorStateType.Running;
			}

			public override void Abort(ScientistAStarContext context, PrimitiveTaskSelector task)
			{
			}
		}

		public class AStarHoldLocationTimed : OperatorBase<ScientistAStarContext>
		{
			[ApexSerialization]
			private float _duckTimeMin = 1f;

			[ApexSerialization]
			private float _duckTimeMax = 1f;

			public override void Execute(ScientistAStarContext context)
			{
				AStarIsNotNavigatingEffect.ApplyStatic(context, false, false);
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

			public override OperatorStateType Tick(ScientistAStarContext context, PrimitiveTaskSelector task)
			{
				if (context.IsFact(Facts.IsWaiting))
				{
					return OperatorStateType.Running;
				}
				ApplyExpectedEffects(context, task);
				return OperatorStateType.Complete;
			}

			public override void Abort(ScientistAStarContext context, PrimitiveTaskSelector task)
			{
				context.SetFact(Facts.IsWaiting, false);
			}

			private IEnumerator AsyncTimer(ScientistAStarContext context, float time)
			{
				yield return CoroutineEx.waitForSeconds(time);
				context.SetFact(Facts.IsWaiting, false);
			}
		}

		public class AStarApplyFirearmOrder : OperatorBase<ScientistAStarContext>
		{
			public override void Execute(ScientistAStarContext context)
			{
			}

			public override OperatorStateType Tick(ScientistAStarContext context, PrimitiveTaskSelector task)
			{
				ApplyExpectedEffects(context, task);
				return OperatorStateType.Complete;
			}

			public override void Abort(ScientistAStarContext context, PrimitiveTaskSelector task)
			{
			}
		}

		public class AStarLookAround : OperatorBase<ScientistAStarContext>
		{
			[ApexSerialization]
			private float _lookAroundTime = 1f;

			public override void Execute(ScientistAStarContext context)
			{
				context.SetFact(Facts.IsLookingAround, true);
				context.Body.StartCoroutine(LookAroundAsync(context));
			}

			public override OperatorStateType Tick(ScientistAStarContext context, PrimitiveTaskSelector task)
			{
				if (context.IsFact(Facts.IsLookingAround))
				{
					return OperatorStateType.Running;
				}
				ApplyExpectedEffects(context, task);
				return OperatorStateType.Complete;
			}

			private IEnumerator LookAroundAsync(ScientistAStarContext context)
			{
				yield return CoroutineEx.waitForSeconds(_lookAroundTime);
				if (context.IsFact(Facts.CanSeeEnemy))
				{
					context.SetFact(Facts.IsSearching, false);
				}
				context.SetFact(Facts.IsLookingAround, false);
			}

			public override void Abort(ScientistAStarContext context, PrimitiveTaskSelector task)
			{
				context.SetFact(Facts.IsSearching, false);
				context.SetFact(Facts.IsLookingAround, false);
			}
		}

		public class AStarHoldItemOfType : OperatorBase<ScientistAStarContext>
		{
			[ApexSerialization]
			private ItemType _item;

			[ApexSerialization]
			private float _switchTime = 0.2f;

			public override void Execute(ScientistAStarContext context)
			{
				SwitchToItem(context, _item);
				context.Body.StartCoroutine(WaitAsync(context));
			}

			public override OperatorStateType Tick(ScientistAStarContext context, PrimitiveTaskSelector task)
			{
				if (context.IsFact(Facts.IsWaiting))
				{
					return OperatorStateType.Running;
				}
				ApplyExpectedEffects(context, task);
				return OperatorStateType.Complete;
			}

			private IEnumerator WaitAsync(ScientistAStarContext context)
			{
				context.SetFact(Facts.IsWaiting, true);
				yield return CoroutineEx.waitForSeconds(_switchTime);
				context.SetFact(Facts.IsWaiting, false);
			}

			public override void Abort(ScientistAStarContext context, PrimitiveTaskSelector task)
			{
				_item = (ItemType)context.GetPreviousFact(Facts.HeldItemType);
				SwitchToItem(context, _item);
				context.SetFact(Facts.IsWaiting, false);
			}

			public static void SwitchToItem(ScientistAStarContext context, ItemType _item)
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

		public class AStarUseMedicalTool : OperatorBase<ScientistAStarContext>
		{
			[ApexSerialization]
			public HealthState Health;

			public override void Execute(ScientistAStarContext context)
			{
				context.Body.StartCoroutine(UseItem(context));
			}

			public override OperatorStateType Tick(ScientistAStarContext context, PrimitiveTaskSelector task)
			{
				if (context.IsFact(Facts.IsApplyingMedical))
				{
					return OperatorStateType.Running;
				}
				ApplyExpectedEffects(context, task);
				return OperatorStateType.Complete;
			}

			public override void Abort(ScientistAStarContext context, PrimitiveTaskSelector task)
			{
				context.SetFact(Facts.IsApplyingMedical, false);
				ItemType previousFact = (ItemType)context.GetPreviousFact(Facts.HeldItemType);
				AStarHoldItemOfType.SwitchToItem(context, previousFact);
			}

			private IEnumerator UseItem(ScientistAStarContext context)
			{
				Item activeItem = context.Body.GetActiveItem();
				if (activeItem != null)
				{
					MedicalTool medicalTool = activeItem.GetHeldEntity() as MedicalTool;
					if (medicalTool != null)
					{
						context.SetFact(Facts.IsApplyingMedical, true);
						medicalTool.ServerUse();
						if (Health == HealthState.FullHealth)
						{
							context.Body.Heal(context.Body.MaxHealth());
						}
						yield return CoroutineEx.waitForSeconds(medicalTool.repeatDelay * 4f);
					}
				}
				context.SetFact(Facts.IsApplyingMedical, false);
				ItemType previousFact = (ItemType)context.GetPreviousFact(Facts.HeldItemType);
				AStarHoldItemOfType.SwitchToItem(context, previousFact);
			}
		}

		public class AStarReloadFirearmOperator : OperatorBase<ScientistAStarContext>
		{
			public override void Execute(ScientistAStarContext context)
			{
				context.Domain.ReloadFirearm();
			}

			public override OperatorStateType Tick(ScientistAStarContext context, PrimitiveTaskSelector task)
			{
				if (context.IsFact(Facts.IsReloading))
				{
					return OperatorStateType.Running;
				}
				ApplyExpectedEffects(context, task);
				return OperatorStateType.Complete;
			}

			public override void Abort(ScientistAStarContext context, PrimitiveTaskSelector task)
			{
			}
		}

		public class AStarApplyFrustration : OperatorBase<ScientistAStarContext>
		{
			public override void Execute(ScientistAStarContext context)
			{
			}

			public override OperatorStateType Tick(ScientistAStarContext context, PrimitiveTaskSelector task)
			{
				ApplyExpectedEffects(context, task);
				return OperatorStateType.Complete;
			}

			public override void Abort(ScientistAStarContext context, PrimitiveTaskSelector task)
			{
			}
		}

		public class AStarUseThrowableWeapon : OperatorBase<ScientistAStarContext>
		{
			[ApexSerialization]
			private NpcOrientation _orientation = NpcOrientation.LastKnownPrimaryTargetLocation;

			public static float LastTimeThrown;

			public override void Execute(ScientistAStarContext context)
			{
				if (context.Memory.PrimaryKnownEnemyPlayer.PlayerInfo.Player != null)
				{
					context.Body.StartCoroutine(UseItem(context));
				}
			}

			public override OperatorStateType Tick(ScientistAStarContext context, PrimitiveTaskSelector task)
			{
				if (context.IsFact(Facts.IsThrowingWeapon))
				{
					return OperatorStateType.Running;
				}
				ApplyExpectedEffects(context, task);
				return OperatorStateType.Complete;
			}

			public override void Abort(ScientistAStarContext context, PrimitiveTaskSelector task)
			{
				context.SetFact(Facts.IsThrowingWeapon, false);
				ItemType previousFact = (ItemType)context.GetPreviousFact(Facts.HeldItemType);
				AStarHoldItemOfType.SwitchToItem(context, previousFact);
			}

			private IEnumerator UseItem(ScientistAStarContext context)
			{
				Item activeItem = context.Body.GetActiveItem();
				if (activeItem != null)
				{
					ThrownWeapon thrownWeapon = activeItem.GetHeldEntity() as ThrownWeapon;
					if (thrownWeapon != null)
					{
						context.SetFact(Facts.IsThrowingWeapon, true);
						LastTimeThrown = UnityEngine.Time.time;
						context.OrientationType = _orientation;
						context.Body.ForceOrientationTick();
						yield return null;
						thrownWeapon.ServerThrow(context.Memory.PrimaryKnownEnemyPlayer.LastKnownPosition);
						yield return null;
					}
				}
				context.SetFact(Facts.IsThrowingWeapon, false);
				AStarHoldItemOfType.SwitchToItem(context, ItemType.ProjectileWeapon);
			}
		}

		public delegate void OnPlanAborted(ScientistAStarDomain domain);

		public delegate void OnPlanCompleted(ScientistAStarDomain domain);

		public class AStarHasWorldState : ContextualScorerBase<ScientistAStarContext>
		{
			[ApexSerialization]
			public Facts Fact;

			[ApexSerialization]
			public byte Value;

			public override float Score(ScientistAStarContext c)
			{
				if (c.GetWorldState(Fact) != Value)
				{
					return 0f;
				}
				return score;
			}
		}

		public class AStarHasWorldStateBool : ContextualScorerBase<ScientistAStarContext>
		{
			[ApexSerialization]
			public Facts Fact;

			[ApexSerialization]
			public bool Value;

			public override float Score(ScientistAStarContext c)
			{
				byte b = (byte)(Value ? 1u : 0u);
				if (c.GetWorldState(Fact) != b)
				{
					return 0f;
				}
				return score;
			}
		}

		public class AStarHasWorldStateGreaterThan : ContextualScorerBase<ScientistAStarContext>
		{
			[ApexSerialization]
			public Facts Fact;

			[ApexSerialization]
			public byte Value;

			public override float Score(ScientistAStarContext c)
			{
				if (c.GetWorldState(Fact) <= Value)
				{
					return 0f;
				}
				return score;
			}
		}

		public class AStarHasWorldStateLessThan : ContextualScorerBase<ScientistAStarContext>
		{
			[ApexSerialization]
			public Facts Fact;

			[ApexSerialization]
			public byte Value;

			public override float Score(ScientistAStarContext c)
			{
				if (c.GetWorldState(Fact) >= Value)
				{
					return 0f;
				}
				return score;
			}
		}

		public class AStarHasWorldStateEnemyRange : ContextualScorerBase<ScientistAStarContext>
		{
			[ApexSerialization]
			public EnemyRange Value;

			public override float Score(ScientistAStarContext c)
			{
				if ((uint)c.GetWorldState(Facts.EnemyRange) != (uint)Value)
				{
					return 0f;
				}
				return score;
			}
		}

		public class AStarHasWorldStateAmmo : ContextualScorerBase<ScientistAStarContext>
		{
			[ApexSerialization]
			public AmmoState Value;

			public override float Score(ScientistAStarContext c)
			{
				if ((uint)c.GetWorldState(Facts.AmmoState) != (uint)Value)
				{
					return 0f;
				}
				return score;
			}
		}

		public class AStarHasWorldStateHealth : ContextualScorerBase<ScientistAStarContext>
		{
			[ApexSerialization]
			public HealthState Value;

			public override float Score(ScientistAStarContext c)
			{
				if ((uint)c.GetWorldState(Facts.HealthState) != (uint)Value)
				{
					return 0f;
				}
				return score;
			}
		}

		public class AStarHasWorldStateCoverState : ContextualScorerBase<ScientistAStarContext>
		{
			[ApexSerialization]
			public CoverState Value;

			public override float Score(ScientistAStarContext c)
			{
				if ((uint)c.GetWorldState(Facts.CoverState) != (uint)Value)
				{
					return 0f;
				}
				return score;
			}
		}

		public class AStarHasItem : ContextualScorerBase<ScientistAStarContext>
		{
			[ApexSerialization]
			public ItemType Value;

			public override float Score(ScientistAStarContext c)
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

		public class AStarIsHoldingItem : ContextualScorerBase<ScientistAStarContext>
		{
			[ApexSerialization]
			public ItemType Value;

			public override float Score(ScientistAStarContext c)
			{
				if ((uint)c.GetWorldState(Facts.HeldItemType) == (uint)Value)
				{
					return score;
				}
				return 0f;
			}
		}

		public class AStarIsNavigationBlocked : ContextualScorerBase<ScientistAStarContext>
		{
			public override float Score(ScientistAStarContext c)
			{
				if (!CanNavigate(c))
				{
					return score;
				}
				return 0f;
			}

			public static bool CanNavigate(ScientistAStarContext c)
			{
				return false;
			}
		}

		public class AStarIsNavigationAllowed : ContextualScorerBase<ScientistAStarContext>
		{
			public override float Score(ScientistAStarContext c)
			{
				if (!AStarIsNavigationBlocked.CanNavigate(c))
				{
					return 0f;
				}
				return score;
			}
		}

		public class AStarIsReloadingBlocked : ContextualScorerBase<ScientistAStarContext>
		{
			public override float Score(ScientistAStarContext c)
			{
				return 0f;
			}
		}

		public class AStarIsReloadingAllowed : ContextualScorerBase<ScientistAStarContext>
		{
			public override float Score(ScientistAStarContext c)
			{
				return score;
			}
		}

		public class AStarIsShootingBlocked : ContextualScorerBase<ScientistAStarContext>
		{
			public override float Score(ScientistAStarContext c)
			{
				return 0f;
			}
		}

		public class AStarIsShootingAllowed : ContextualScorerBase<ScientistAStarContext>
		{
			public override float Score(ScientistAStarContext c)
			{
				return score;
			}
		}

		public class AStarHasFirearmOrder : ContextualScorerBase<ScientistAStarContext>
		{
			[ApexSerialization]
			public FirearmOrders Order;

			public override float Score(ScientistAStarContext c)
			{
				return score;
			}
		}

		public class AStarCanNavigateToWaypoint : ContextualScorerBase<ScientistAStarContext>
		{
			public override float Score(ScientistAStarContext c)
			{
				Vector3 nextWaypointPosition = AStarNavigateToWaypoint.GetNextWaypointPosition(c);
				if (!c.Memory.IsValid(nextWaypointPosition))
				{
					return 0f;
				}
				return score;
			}
		}

		public class AStarCanNavigateToPreferredFightingRange : ContextualScorerBase<ScientistAStarContext>
		{
			[ApexSerialization]
			private bool CanNot;

			public override float Score(ScientistAStarContext c)
			{
				Vector3 preferredFightingPosition = AStarNavigateToPreferredFightingRange.GetPreferredFightingPosition(c);
				if ((preferredFightingPosition - c.BodyPosition).sqrMagnitude < 0.01f)
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

		public class AStarCanRememberPrimaryEnemyTarget : ContextualScorerBase<ScientistAStarContext>
		{
			public override float Score(ScientistAStarContext c)
			{
				if (!(c.Memory.PrimaryKnownEnemyPlayer.PlayerInfo.Player != null))
				{
					return 0f;
				}
				return score;
			}
		}

		public class AStarCanNavigateToLastKnownPositionOfPrimaryEnemyTarget : ContextualScorerBase<ScientistAStarContext>
		{
			public override float Score(ScientistAStarContext c)
			{
				if (c.HasVisitedLastKnownEnemyPlayerLocation)
				{
					return score;
				}
				Vector3 destination = AStarNavigateToLastKnownLocationOfPrimaryEnemyPlayer.GetDestination(c);
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

		public class AStarCanNavigateToCoverLocation : ContextualScorerBase<ScientistAStarContext>
		{
			[ApexSerialization]
			private CoverTactic _preferredTactic;

			public override float Score(ScientistAStarContext c)
			{
				if (!Try(_preferredTactic, c))
				{
					return 0f;
				}
				return score;
			}

			public static bool Try(CoverTactic tactic, ScientistAStarContext c)
			{
				Vector3 coverPosition = AStarNavigateToCover.GetCoverPosition(tactic, c);
				if (!c.Domain.AllowedMovementDestination(coverPosition))
				{
					return false;
				}
				if ((coverPosition - c.BodyPosition).sqrMagnitude < 0.1f)
				{
					return false;
				}
				return c.Memory.IsValid(coverPosition);
			}
		}

		public class AStarCanNavigateAwayFromExplosive : ContextualScorerBase<ScientistAStarContext>
		{
			public override float Score(ScientistAStarContext c)
			{
				if (!Try(c))
				{
					return 0f;
				}
				return score;
			}

			public static bool Try(ScientistAStarContext c)
			{
				Vector3 destination = AStarNavigateAwayFromExplosive.GetDestination(c);
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

		public class AStarCanNavigateAwayFromAnimal : ContextualScorerBase<ScientistAStarContext>
		{
			public override float Score(ScientistAStarContext c)
			{
				if (!Try(c))
				{
					return 0f;
				}
				return score;
			}

			public static bool Try(ScientistAStarContext c)
			{
				Vector3 destination = AStarNavigateAwayFromAnimal.GetDestination(c);
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

		public class AStarCanUseWeaponAtCurrentRange : ContextualScorerBase<ScientistAStarContext>
		{
			public override float Score(ScientistAStarContext c)
			{
				if (!Try(c))
				{
					return 0f;
				}
				return score;
			}

			public static bool Try(ScientistAStarContext c)
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

		public class AStarCanThrowAtLastKnownLocation : ContextualScorerBase<ScientistAStarContext>
		{
			public override float Score(ScientistAStarContext c)
			{
				if (!Try(c))
				{
					return 0f;
				}
				return score;
			}

			public static bool Try(ScientistAStarContext c)
			{
				if (!ConVar.AI.npc_use_thrown_weapons)
				{
					return false;
				}
				if (c.Memory.PrimaryKnownEnemyPlayer.PlayerInfo.Player == null)
				{
					return false;
				}
				if (UnityEngine.Time.time - AStarUseThrowableWeapon.LastTimeThrown < 10f)
				{
					return false;
				}
				Vector3 destination = AStarNavigateToLastKnownLocationOfPrimaryEnemyPlayer.GetDestination(c);
				if ((destination - c.BodyPosition).sqrMagnitude < 0.1f)
				{
					return false;
				}
				Vector3 vector = destination + PlayerEyes.EyeOffset;
				Vector3 vector2 = c.Memory.PrimaryKnownEnemyPlayer.PlayerInfo.Player.transform.position + PlayerEyes.EyeOffset;
				if ((vector - vector2).sqrMagnitude > 5f)
				{
					return false;
				}
				Vector3 vector3 = c.BodyPosition + PlayerEyes.EyeOffset;
				if (Mathf.Abs(Vector3.Dot((vector3 - vector).normalized, (vector3 - vector2).normalized)) < 0.75f)
				{
					return false;
				}
				if (!c.Body.IsVisible(vector))
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

		private Vector3 _lastNavigationHeading = Vector3.zero;

		[Header("Pathfinding")]
		[ReadOnly]
		public BasePath Path;

		[ReadOnly]
		public List<BasePathNode> CurrentPath;

		[ReadOnly]
		public int CurrentPathIndex;

		[ReadOnly]
		public bool PathLooping;

		[ReadOnly]
		public BasePathNode FinalDestination;

		[ReadOnly]
		public float StoppingDistance = 1f;

		public OnPlanAborted OnPlanAbortedEvent;

		public OnPlanCompleted OnPlanCompletedEvent;

		[Header("Context")]
		[SerializeField]
		private ScientistAStarContext _context;

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
			new CoverPointsInRangeSensor
			{
				TickFrequency = 1f
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
			new EnemyPlayerHearingReasoner
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
			new PreferredFightingRangeReasoner
			{
				TickFrequency = 0.1f
			},
			new AtLastKnownEnemyPlayerLocationReasoner
			{
				TickFrequency = 0.1f
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
			new VulnerabilityReasoner
			{
				TickFrequency = 0.1f
			},
			new FrustrationReasoner
			{
				TickFrequency = 0.25f
			},
			new CoverPointsReasoner
			{
				TickFrequency = 0.5f
			},
			new AtCoverLocationReasoner
			{
				TickFrequency = 0.1f
			},
			new MaintainCoverReasoner
			{
				TickFrequency = 0.1f
			},
			new ReturnHomeReasoner
			{
				TickFrequency = 5f
			},
			new AtHomeLocationReasoner
			{
				TickFrequency = 5f
			},
			new ExplosivesReasoner
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
			},
			new EnemyRangeReasoner
			{
				TickFrequency = 0.1f
			},
			new AtNextAStarWaypointLocationReasoner
			{
				TickFrequency = 0.1f
			}
		};

		[SerializeField]
		[Header("Firearm Utility")]
		[ReadOnly]
		private float _lastFirearmUsageTime;

		[ReadOnly]
		[SerializeField]
		private bool _isFiring;

		private HTNUtilityAiClient _aiClient;

		private ScientistAStarDefinition _scientistDefinition;

		public bool HasPath
		{
			get
			{
				if (CurrentPath != null)
				{
					return CurrentPath.Count > 0;
				}
				return false;
			}
		}

		public float SqrStoppingDistance => StoppingDistance * StoppingDistance;

		public ScientistAStarDefinition ScientistDefinition
		{
			get
			{
				if (_scientistDefinition == null)
				{
					_scientistDefinition = _context.Body.AiDefinition as ScientistAStarDefinition;
				}
				return _scientistDefinition;
			}
		}

		public Vector3 SpawnPosition
		{
			get
			{
				BaseEntity parentEntity = _context.Body.GetParentEntity();
				if (parentEntity != null)
				{
					return parentEntity.transform.TransformPoint(_spawnPosition);
				}
				return _spawnPosition;
			}
		}

		public ScientistAStarContext ScientistContext => _context;

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
			ResumeNavigation();
		}

		public override void Pause()
		{
			PauseNavigation();
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
				AStarHoldItemOfType.SwitchToItem(_context, ItemType.ProjectileWeapon);
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
				proj.ServerUse(ConVar.AI.npc_htn_player_base_damage_modifier);
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
			if (sqrMagnitude <= _context.Body.AiDefinition.Engagement.SqrCloseRangeFirearm(GetFirearm()) + 2f)
			{
				return heading;
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
			float num2 = ScientistDefinition.MissFunction.Evaluate(Mathf.Approximately(num, 0f) ? 1f : (1f - Mathf.Min(num / maxTime, 1f)));
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
		}

		public void ResumeNavigation()
		{
		}

		public override Vector3 GetNextPosition(float delta)
		{
			if (!HasPath)
			{
				return _context.BodyPosition;
			}
			Vector3 vector = GetCurrentPathDestination() - base.transform.position;
			if (vector.sqrMagnitude <= SqrStoppingDistance)
			{
				return _context.BodyPosition;
			}
			vector.Normalize();
			float num = (_context.IsFact(Facts.IsDucking) ? ScientistDefinition.Movement.DuckSpeed : ScientistDefinition.Movement.WalkSpeed);
			float num2 = ((delta > 0.1f) ? 0.1f : delta);
			float acceleration = ScientistDefinition.Movement.Acceleration;
			float num3 = num * num2 - 0.5f * acceleration * num2 * num2;
			return _context.BodyPosition + vector * num3;
		}

		public bool SetDestination(Vector3 destination)
		{
			if (_SetDestination(destination))
			{
				_context.SetFact(Facts.PathStatus, (byte)1, true, false, true);
				return true;
			}
			_context.SetFact(Facts.PathStatus, (byte)3, true, false, true);
			return false;
		}

		private bool _SetDestination(Vector3 destination)
		{
			BasePathNode closestToPoint = Path.GetClosestToPoint(destination);
			if (closestToPoint == null || closestToPoint.transform == null)
			{
				return false;
			}
			BasePathNode closestToPoint2 = Path.GetClosestToPoint(base.transform.position);
			if (closestToPoint2 == null || closestToPoint2.transform == null)
			{
				return false;
			}
			_context.Memory.HasTargetDestination = true;
			_context.Memory.TargetDestination = destination;
			if (closestToPoint2 == closestToPoint || (closestToPoint2.transform.position - closestToPoint.transform.position).sqrMagnitude <= SqrStoppingDistance)
			{
				CurrentPath.Clear();
				CurrentPath.Add(closestToPoint);
				CurrentPathIndex = -1;
				PathLooping = false;
				FinalDestination = closestToPoint;
				return true;
			}
			Stack<BasePathNode> path;
			float pathCost;
			if (AStarPath.FindPath(closestToPoint2, closestToPoint, out path, out pathCost))
			{
				CurrentPath.Clear();
				while (path.Count > 0)
				{
					CurrentPath.Add(path.Pop());
				}
				CurrentPathIndex = -1;
				PathLooping = false;
				FinalDestination = closestToPoint;
				return true;
			}
			return false;
		}

		public override void TickDestinationTracker()
		{
			if (!IsPathValid())
			{
				_context.Memory.AddFailedDestination(_context.Memory.TargetDestination);
				_context.Memory.HasTargetDestination = false;
				_context.SetFact(Facts.PathStatus, (byte)3, true, false, true);
			}
			if (_context.Memory.HasTargetDestination && PathComplete())
			{
				_context.Memory.HasTargetDestination = false;
				_context.SetFact(Facts.PathStatus, (byte)2, true, false, true);
			}
			if (_context.Memory.HasTargetDestination && HasPath)
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
			if (!AllowedMovementDestination(_context.Memory.TargetDestination))
			{
				return false;
			}
			return true;
		}

		public override Vector3 GetHeadingDirection()
		{
			if (_context.GetFact(Facts.IsNavigating) > 0 && HasPath && FinalDestination != null)
			{
				_lastNavigationHeading = (FinalDestination.transform.position - _context.BodyPosition).normalized;
				return _lastNavigationHeading;
			}
			if (_lastNavigationHeading.sqrMagnitude > 0f)
			{
				return _lastNavigationHeading;
			}
			return _context.Body.eyes.rotation.eulerAngles.normalized;
		}

		public override Vector3 GetHomeDirection()
		{
			Vector3 v = SpawnPosition - _context.BodyPosition;
			if (v.SqrMagnitudeXZ() < 0.01f)
			{
				return _context.Body.eyes.rotation.eulerAngles.normalized;
			}
			return v.normalized;
		}

		public void StopNavigating()
		{
			_context.Memory.HasTargetDestination = false;
			_context.SetFact(Facts.PathStatus, (byte)0, true, false, true);
		}

		public bool PathDistanceIsValid(Vector3 from, Vector3 to, bool allowCloseRange = false)
		{
			float sqrMagnitude = (from - to).sqrMagnitude;
			if (!(sqrMagnitude > ScientistContext.Body.AiDefinition.Engagement.SqrMediumRange))
			{
				if (allowCloseRange)
				{
					return true;
				}
				float sqrCloseRange = ScientistContext.Body.AiDefinition.Engagement.SqrCloseRange;
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

		public float GetAllowedCoverRangeSqr()
		{
			float result = 225f;
			if (Movement == MovementRule.RestrainedMove && MovementRadius < 15f)
			{
				result = base.SqrMovementRadius;
			}
			return result;
		}

		public void InstallPath(BasePath path)
		{
			Path = path;
			CurrentPath = new List<BasePathNode>();
			CurrentPathIndex = -1;
			_context.Memory.TargetDestination = _context.BodyPosition;
			FinalDestination = null;
		}

		public void ClearPath()
		{
			CurrentPath.Clear();
			CurrentPathIndex = -1;
		}

		public bool IndexValid(int index)
		{
			if (!HasPath)
			{
				return false;
			}
			if (index >= 0)
			{
				return index < CurrentPath.Count;
			}
			return false;
		}

		public BasePathNode GetFinalDestination()
		{
			if (!HasPath)
			{
				return null;
			}
			return FinalDestination;
		}

		public Vector3 GetCurrentPathDestination()
		{
			if (!HasPath)
			{
				return base.transform.position;
			}
			if (AtCurrentPathNode() || CurrentPathIndex == -1)
			{
				CurrentPathIndex = GetLoopedIndex(CurrentPathIndex + 1);
			}
			if (CurrentPath[CurrentPathIndex] == null)
			{
				Debug.LogWarning("Scientist CurrentPathIndex was NULL (perhaps the path owner was destroyed but this was not?");
				return base.transform.position;
			}
			return CurrentPath[CurrentPathIndex].transform.position;
		}

		public bool PathComplete()
		{
			if (HasPath && !(FinalDestination == null))
			{
				return (FinalDestination.transform.position - base.transform.position).sqrMagnitude <= SqrStoppingDistance;
			}
			return true;
		}

		public bool AtCurrentPathNode()
		{
			if (base.transform == null || CurrentPath == null)
			{
				return false;
			}
			if (CurrentPathIndex < 0 || CurrentPathIndex >= CurrentPath.Count)
			{
				return false;
			}
			if (CurrentPath[CurrentPathIndex] == null || CurrentPath[CurrentPathIndex].transform == null)
			{
				return false;
			}
			return (base.transform.position - CurrentPath[CurrentPathIndex].transform.position).sqrMagnitude <= SqrStoppingDistance;
		}

		public int GetLoopedIndex(int index)
		{
			if (!HasPath)
			{
				Debug.LogWarning("Warning, GetLoopedIndex called without a path");
				return 0;
			}
			if (!PathLooping)
			{
				return Mathf.Clamp(index, 0, CurrentPath.Count - 1);
			}
			if (index >= CurrentPath.Count)
			{
				return index % CurrentPath.Count;
			}
			if (index < 0)
			{
				return CurrentPath.Count - Mathf.Abs(index % CurrentPath.Count);
			}
			return index;
		}

		public Vector3 PathDirection(int index)
		{
			if (!HasPath || CurrentPath.Count <= 1)
			{
				return base.transform.forward;
			}
			index = GetLoopedIndex(index);
			Vector3 zero = Vector3.zero;
			Vector3 zero2 = Vector3.zero;
			if (PathLooping)
			{
				int loopedIndex = GetLoopedIndex(index - 1);
				zero = CurrentPath[loopedIndex].transform.position;
				zero2 = CurrentPath[GetLoopedIndex(index)].transform.position;
			}
			else
			{
				zero = ((index - 1 >= 0) ? CurrentPath[index - 1].transform.position : base.transform.position);
				zero2 = CurrentPath[index].transform.position;
			}
			return (zero2 - zero).normalized;
		}

		public Vector3 IdealPathPosition()
		{
			if (!HasPath)
			{
				return base.transform.position;
			}
			int loopedIndex = GetLoopedIndex(CurrentPathIndex - 1);
			if (loopedIndex == CurrentPathIndex)
			{
				return CurrentPath[CurrentPathIndex].transform.position;
			}
			return ClosestPointAlongPath(CurrentPath[loopedIndex].transform.position, CurrentPath[CurrentPathIndex].transform.position, base.transform.position);
		}

		public bool AdvancePathMovement()
		{
			if (!HasPath)
			{
				return false;
			}
			if (AtCurrentPathNode() || CurrentPathIndex == -1)
			{
				CurrentPathIndex = GetLoopedIndex(CurrentPathIndex + 1);
			}
			if (PathComplete())
			{
				ClearPath();
				return false;
			}
			Vector3 vector = IdealPathPosition();
			float a = Vector3.Distance(vector, CurrentPath[CurrentPathIndex].transform.position);
			float value = Vector3.Distance(base.transform.position, vector);
			float num = Mathf.InverseLerp(8f, 0f, value);
			vector += Direction2D(CurrentPath[CurrentPathIndex].transform.position, vector) * Mathf.Min(a, num * 20f);
			SetDestination(vector);
			return true;
		}

		public static Vector3 Direction2D(Vector3 aimAt, Vector3 aimFrom)
		{
			return (new Vector3(aimAt.x, 0f, aimAt.z) - new Vector3(aimFrom.x, 0f, aimFrom.z)).normalized;
		}

		public bool GetPathToClosestTurnableNode(BasePathNode start, Vector3 forward, ref List<BasePathNode> nodes)
		{
			float num = float.NegativeInfinity;
			BasePathNode basePathNode = null;
			foreach (BasePathNode item in start.linked)
			{
				float num2 = Vector3.Dot(forward, (item.transform.position - start.transform.position).normalized);
				if (num2 > num)
				{
					num = num2;
					basePathNode = item;
				}
			}
			if (basePathNode != null)
			{
				nodes.Add(basePathNode);
				if (!basePathNode.straightaway)
				{
					return true;
				}
				return GetPathToClosestTurnableNode(basePathNode, (basePathNode.transform.position - start.transform.position).normalized, ref nodes);
			}
			return false;
		}

		public bool GetEngagementPath(ref List<BasePathNode> nodes)
		{
			BasePathNode closestToPoint = Path.GetClosestToPoint(base.transform.position);
			if (closestToPoint == null || closestToPoint.transform == null)
			{
				return false;
			}
			Vector3 normalized = (closestToPoint.transform.position - base.transform.position).normalized;
			if (Vector3.Dot(base.transform.forward, normalized) > 0f)
			{
				nodes.Add(closestToPoint);
				if (!closestToPoint.straightaway)
				{
					return true;
				}
			}
			return GetPathToClosestTurnableNode(closestToPoint, base.transform.forward, ref nodes);
		}

		public bool IsAtDestination()
		{
			return (base.transform.position - _context.Memory.TargetDestination).sqrMagnitude <= SqrStoppingDistance;
		}

		public bool IsAtFinalDestination()
		{
			if (FinalDestination != null)
			{
				return (base.transform.position - FinalDestination.transform.position).sqrMagnitude <= SqrStoppingDistance;
			}
			return true;
		}

		public Vector3 ClosestPointAlongPath(Vector3 start, Vector3 end, Vector3 fromPos)
		{
			Vector3 vector = end - start;
			Vector3 rhs = fromPos - start;
			float num = Vector3.Dot(vector, rhs);
			float num2 = Vector3.SqrMagnitude(end - start);
			float num3 = Mathf.Clamp01(num / num2);
			return start + vector * num3;
		}

		protected override void AbortPlan()
		{
			base.AbortPlan();
			OnPlanAbortedEvent?.Invoke(this);
			_context.SetFact(Facts.MaintainCover, 0);
			_context.Body.modelState.ducked = false;
		}

		protected override void CompletePlan()
		{
			base.CompletePlan();
			OnPlanCompletedEvent?.Invoke(this);
			_context.SetFact(Facts.MaintainCover, 0);
			_context.Body.modelState.ducked = false;
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
			if (_aiClient == null || _aiClient.ai == null || _aiClient.ai.id != AINameMap.HTNDomainScientistAStar)
			{
				_aiClient = new HTNUtilityAiClient(AINameMap.HTNDomainScientistAStar, this);
			}
			if (_context == null || _context.Body != body)
			{
				_context = new ScientistAStarContext(body as HTNPlayer, this);
			}
			_spawnPosition = _context.Body.transform.localPosition;
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
